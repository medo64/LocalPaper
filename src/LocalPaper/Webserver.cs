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
    public WebServer(string host, int port, DeviceDisplay? defaultDisplay, IEnumerable<DeviceDisplay> displays) {
        Host = host;
        Port = port;

        DefaultDisplay = defaultDisplay;
        foreach (var display in displays) {
            Log.Debug($"Adding display {display.DeviceId} with interval {display.Interval.TotalSeconds} seconds");
            DisplaysById[display.DeviceId] = display;
        }

        ThreadCancelSource = new CancellationTokenSource();
        Thread = new Thread(Run);
    }


    private readonly string Host;
    private readonly int Port;
    private readonly DeviceDisplay? DefaultDisplay;
    private readonly Dictionary<string, DeviceDisplay> DisplaysById = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource ThreadCancelSource;
    private readonly Thread Thread;


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
        var prefix = $"http://{Host}:{Port}/";
        Log.Debug($"Starting web server at {prefix}");

        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{Port}/");
        listener.Prefixes.Add("http://127.0.0.1/");
        listener.Start();

        Log.Info($"Started web server at {prefix}");

        var cancelToken = ThreadCancelSource.Token;
        while (!cancelToken.IsCancellationRequested) {
            var getContextTask = listener.GetContextAsync();
            try {
                Task.WaitAny(new[] { getContextTask }, cancelToken);
            } catch (OperationCanceledException) { }
            if (cancelToken.IsCancellationRequested) { break; }
            var context = getContextTask.Result;

            var url = context.Request.Url?.AbsolutePath?.TrimEnd('/') ?? "";
            if (string.IsNullOrEmpty(url)) { url = "/"; }
            Log.Verbose($"Received request for {url}");

            if ("/".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                context.Response.ContentType = "text/plain";
                var buffer = Utf8.GetBytes("Hello World!");
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            } else if ("/health".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                context.Response.OutputStream.Close();
            } else if ("/api/setup".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                RespondToSetup(context.Request, context.Response);
            } else if ("/api/display".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                RespondToDisplay(context.Request, context.Response);
            } else if ("/api/log".Equals(url, StringComparison.OrdinalIgnoreCase)) {
                RespondToLog(context.Request, context.Response);
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
        if (Log.MinimumLogLevel <= LogLevel.Verbose) {
            foreach (var key in request.Headers.AllKeys) {
                var value = request.Headers[key];
                Log.Verbose($"Header: {key}: {value}");
            }
        }

        var macAddress = request.Headers["ID"] ?? "unknown";
        var id = macAddress.Replace(":", "").ToUpperInvariant().Trim();

        if (!DisplaysById.TryGetValue(id, out var _)) {
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
        if (Log.MinimumLogLevel <= LogLevel.Verbose) {
            foreach (var key in request.Headers.AllKeys) {
                var value = request.Headers[key];
                Log.Verbose($"Header: {key}: {value}");
            }
        }

        var id = request.Headers["ID"]?.Replace(":", "")?.ToUpperInvariant()?.Trim() ?? "unknown";
        var voltageText = request.Headers["Battery-Voltage"] ?? null;
        var rssiText = request.Headers["RSSI"] ?? null;
        var fwVersion = request.Headers["FW-Version"] ?? null;

        var batteryLevel = new BatteryLevel(null);
        if (double.TryParse(voltageText, NumberStyles.Float, CultureInfo.InvariantCulture, out var voltage)) {
            batteryLevel = new BatteryLevel(voltage);
            Recorder.RecordBattery(id, batteryLevel);
            if (batteryLevel.Percentage < 10) {
                Log.Error($"Battery for {id} is getting really low: {batteryLevel.Percentage}%");
            } else if (batteryLevel.Percentage < 30) {
                Log.Warning($"Battery for {id} is low: {batteryLevel.Percentage}%");
            }
        }

        if (int.TryParse(rssiText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rssi)) {
            Recorder.RecordWireless(id, new WirelessLevel(rssi));
        }

        if (!DisplaysById.TryGetValue(id, out var display)) {
            if (DefaultDisplay is not null) {
                Log.Info($"No composer found for ID {id}, using default composer");
                display = DefaultDisplay;
            } else {
                Log.Warning($"No composer found for ID {id}, and no default composer is set");
                response.StatusCode = 404;
                response.Close();
                return;
            }
        }

        var interval = (int)display.Interval.TotalSeconds;
        if (batteryLevel.Percentage <= 10) {  // if battery is below 10%, increase the interval to 1 hour
            Math.Max(interval, 3600);
        } else if (batteryLevel.Percentage <= 30) {  // if battery is below 30%, increase the interval to 15 minutes
            Math.Max(interval, 900);
        }

        var timeNow = DateTime.UtcNow;
        var ticksCurr = DateTime.UtcNow.Ticks;
        ticksCurr /= 10_000_000L * interval;
        ticksCurr *= 10_000_000L * interval;
        var timeCurr = new DateTime(ticksCurr, DateTimeKind.Utc);  // round to the nearest interval
        var timeNext = timeCurr.AddSeconds(interval);
        var nextInterval = (int)(timeNext - timeNow).TotalSeconds;
        if (nextInterval < 60) {  // don't bother adjusting for this, just show it now
            Log.Debug($"Interval skipped for {id} (using the following interval instead)");
            timeCurr = timeCurr.AddSeconds(interval);
            timeNext = timeCurr.AddSeconds(interval);
            nextInterval = interval + 30;
        }

        var prefix = request.Url?.GetLeftPart(UriPartial.Authority);
        var fileName = display.DeviceId + "_" + timeCurr.ToString("yyyy-MM-dd'T'HH-mm-ss", CultureInfo.InvariantCulture) + ".bmp";
        var imageUrl = prefix + "/" + fileName;
        int refreshRate = nextInterval;

        var json = "{ \"status\": 0, \"image_url\": \"" + imageUrl + "\", \"filename\": \"" + fileName + "\", \"refresh_rate\": " + refreshRate.ToString(CultureInfo.InvariantCulture) + ", \"reset_firmware\": false, \"update_firmware\": false, \"firmware_url\": null, \"special_function\": \"identify\" }";
        var buffer = Utf8.GetBytes(json);

        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        Log.Debug($"Responded to display request from {id} (battery: {batteryLevel.Voltage?.ToString("0.00", CultureInfo.InvariantCulture) ?? "?"}V; firmware: {fwVersion})");
    }

    private void RespondToLog(HttpListenerRequest request, HttpListenerResponse response) {
        if (Log.MinimumLogLevel <= LogLevel.Verbose) {
            foreach (var key in request.Headers.AllKeys) {
                var value = request.Headers[key];
                Log.Verbose($"Header: {key}: {value}");
            }
        }

        response.StatusCode = 204;
        response.Close();
    }

    private void RespondToFile(HttpListenerRequest request, HttpListenerResponse response) {
        if (Log.MinimumLogLevel <= LogLevel.Verbose) {
            foreach (var key in request.Headers.AllKeys) {
                var value = request.Headers[key];
                Log.Verbose($"Header: {key}: {value}");
            }
        }

        var imageName = request.Url?.AbsolutePath.TrimStart('/') ?? "";
        var buffer = GetResourceBuffer(imageName);

        if ((buffer == null) && imageName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)) {
            var imageNameParts = imageName.Split('_');
            if ((imageNameParts.Length == 2) && DateTime.TryParseExact(imageNameParts[1].Replace(".bmp", ""), "yyyy-MM-dd'T'HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var time)) {
                var displayId = imageNameParts[0];
                if (!DisplaysById.TryGetValue(displayId, out var display)) {
                    if (DefaultDisplay is not null) {
                        display = DefaultDisplay;
                    } else {
                        Log.Warning($"No composer found for ID {displayId}, and no default composer is set");
                        response.StatusCode = 404;
                        response.Close();
                        return;
                    }
                }

#if !DEBUG
                var diff = Math.Abs((time - DateTime.UtcNow).TotalSeconds);
                if (diff > display.Interval.TotalSeconds) {  // prevent composing images too far in the future or past
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }
#endif

                Log.Info($"Composing image for {displayId} at {time:yyyy-MM-dd' 'HH:mm:ss} using composer {display.DeviceId}");
                buffer = display.GetImageBytes(new DataBag() {
                    DisplayId = display.DeviceId,
                    UtcTime = time,
                    TimeZone = TimeZoneInfo.Utc,
                    BatteryLevel = Recorder.GetBatteryLevel(displayId),
                    WirelessLevel = Recorder.GetWirelessLevel(displayId),
                });
            }
        }

        if (buffer == null) {
            Log.Warning($"No file named {imageName}");
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
