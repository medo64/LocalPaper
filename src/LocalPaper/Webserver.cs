namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class WebServer : IDisposable {
    public WebServer(IEnumerable<IPAddress> ips, int port, IEnumerable<DeviceDisplay> displays) {
        foreach (var ip in ips) {
            var url = "http://" + ip.ToString() + ":" + port.ToString(CultureInfo.InvariantCulture);
            Urls.Add(url);
        }

        foreach (var composer in displays) {
            if (composer.DeviceId == "any") {
                DefaultDisplay = composer;
            } else {
                DisplaysById[composer.DeviceId] = composer;
            }
        }


        ThreadCancelSource = new CancellationTokenSource();
        Thread = new Thread(Run);
    }


    private readonly List<string> Urls = new();
    private readonly DeviceDisplay? DefaultDisplay;
    private readonly Dictionary<string, DeviceDisplay> DisplaysById = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource ThreadCancelSource;
    private readonly Thread Thread;
    private readonly int Interval = 300;


    public void Start() {
        if (Thread.IsAlive) { throw new InvalidOperationException("Already started"); }
        Thread.Start();
    }

    public void Stop() {
        if (Thread.IsAlive) {
            Log.Debug("Stopping web server");
            ThreadCancelSource.Cancel();
            Thread.Join();
            Log.Debug("Stopped web server");
        }
    }

    public void Run() {
        Log.Debug("Starting web server at " + string.Join(", ", Urls));

        using var listener = new HttpListener();
        foreach (var url in Urls) {
            listener.Prefixes.Add(url + "/");
        }
        listener.Start();

        Log.Info("Started web server at " + string.Join(", ", Urls));

        var cancelToken = ThreadCancelSource.Token;
        while (!cancelToken.IsCancellationRequested) {
            var getContextTask = listener.GetContextAsync();
            try {
                Task.WaitAny(new[] { getContextTask }, cancelToken);
            } catch (OperationCanceledException) { }
            if (cancelToken.IsCancellationRequested) { break; }
            var context = getContextTask.Result;

            Log.Verbose($"Received request at {context.Request.Url}");
            var url = context.Request.Url?.AbsolutePath;

            if ("/api/setup".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                RespondToSetup(context.Request, context.Response);
            } else if ("/api/display".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                RespondToDisplay(context.Request, context.Response);
            } else if ("/api/log".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                // ignore
            } else if (url is not null) {
                RespondToFile(context.Request, context.Response);
            }
        }

        listener.Stop();
    }

    public void Dispose() {
        Stop();
    }


    private Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private void RespondToSetup(HttpListenerRequest request, HttpListenerResponse response) {
        var macAddress = request.Headers["ID"] ?? "unknown";
        var id = macAddress.Replace(":", "").ToUpperInvariant();

        if (!DisplaysById.TryGetValue(macAddress, out var _)) {
            if (DefaultDisplay is not null) {
                Log.Warning($"No composer found for ID {id} ({macAddress}), using default composer");
            } else {
                Log.Error($"No composer found for ID {id} ({macAddress}), and no default composer is set");
                response.StatusCode = 404;
                response.Close();
                return;
            }
        }

        var prefix = request.Url?.GetLeftPart(UriPartial.Authority);
        var imageUrl = prefix + "/hello.bmp";
        var apiKey = id;  // randomize later

        var json = "{ \"status\": 200, \"api_key\": \"" + apiKey + "\", \"friendly_id\": \"" + id + "\", \"image_url\": \"" + imageUrl + "\", \"filename\": \"empty_state\" }";
        var buffer = Utf8.GetBytes(json);

        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        Log.Info("Responded to setup request");
    }

    private void RespondToDisplay(HttpListenerRequest request, HttpListenerResponse response) {
        var id = request.Headers["ID"] ?? "unknown";
        var voltage = request.Headers["Battery-Voltage"] ?? "?";
        var fwVersion = request.Headers["FW-Version"] ?? "?";

        if (!DisplaysById.TryGetValue(id, out var composer)) {
            if (DefaultDisplay is not null) {
                Log.Info($"No composer found for ID {id}, using default composer");
                composer = DefaultDisplay;
            } else {
                Log.Warning($"No composer found for ID {id}, and no default composer is set");
                response.StatusCode = 404;
                response.Close();
                return;
            }
        }

        var timeNow = DateTime.UtcNow;
        var ticksCurr = DateTime.UtcNow.Ticks;
        ticksCurr /= 10_000_000L * Interval;
        ticksCurr *= 10_000_000L * Interval;
        var timeCurr = new DateTime(ticksCurr, DateTimeKind.Utc);  // round to the nearest interval
        var timeNext = timeCurr.AddSeconds(Interval);
        var nextInterval = (int)(timeNext - timeNow).TotalSeconds;
        if (nextInterval < 60) {  // don't bother adjusting for this, just show it now
            Log.Debug($"Interval skipped for {id} (using the following interval instead)");
            timeCurr = timeCurr.AddSeconds(Interval);
            timeNext = timeCurr.AddSeconds(Interval);
            nextInterval = Interval + 30;
        }

        var prefix = request.Url?.GetLeftPart(UriPartial.Authority);
        var fileName = composer.DeviceId + "_" + timeCurr.ToString("yyyy-MM-dd'T'HH-mm-ss", CultureInfo.InvariantCulture) + ".bmp";
        var imageUrl = prefix + "/" + fileName;
        int refreshRate = nextInterval;

        var json = "{ \"status\": 0, \"image_url\": \"" + imageUrl + "\", \"filename\": \"" + fileName + "\", \"refresh_rate\": " + refreshRate.ToString(CultureInfo.InvariantCulture) + ", \"reset_firmware\": false, \"update_firmware\": false, \"firmware_url\": null, \"special_function\": \"identify\" }";
        var buffer = Utf8.GetBytes(json);

        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        Log.Debug($"Responded to display request from {id} (battery: {voltage}V; firmware: {fwVersion})");
    }

    private void RespondToFile(HttpListenerRequest request, HttpListenerResponse response) {
        var imageName = request.Url?.AbsolutePath.TrimStart('/') ?? "";

        var buffer = GetResourceBuffer(imageName);

        if ((buffer == null) && imageName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)) {
            var imageNameParts = imageName.Split('_');
            if ((imageNameParts.Length == 2) && DateTime.TryParseExact(imageNameParts[1].Replace(".bmp", ""), "yyyy-MM-dd'T'HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var time)) {
                var composerId = imageNameParts[0];
                if (!DisplaysById.TryGetValue(composerId, out var composer)) {
                    if (DefaultDisplay is not null) {
                        composer = DefaultDisplay;
                    } else {
                        Log.Warning($"No composer found for ID {composerId}, and no default composer is set");
                        response.StatusCode = 404;
                        response.Close();
                        return;
                    }
                }
                Log.Info($"Composing image for {composerId} at {time:yyyy-MM-dd' 'HH:mm:ss} using composer {composer.DeviceId}");
                buffer = composer.GetImageBytes(time);
            }
        }

        if (buffer == null) {
            Log.Warning($"No file at named {imageName}");
            response.StatusCode = 404;
            response.Close();
            return;
        }

        response.ContentType = "image/bmp";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        Log.Info($"Responded to image {imageName} request");
    }


    private static byte[]? GetResourceBuffer(string streamName) {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resName in assembly.GetManifestResourceNames()) {
            if (resName.EndsWith("." + streamName, StringComparison.Ordinal)) {
                var resStream = assembly.GetManifestResourceStream(resName);
                var buffer = new byte[(int)resStream!.Length];
                resStream.ReadExactly(buffer);
                return buffer;
            }
        }
        return null;
    }

}
