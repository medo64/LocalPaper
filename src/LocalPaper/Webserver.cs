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
    public WebServer(IEnumerable<IPAddress> ips, int port) {
        foreach (var ip in ips) {
            var url = "http://" + ip.ToString() + ":" + port.ToString(CultureInfo.InvariantCulture);
            Urls.Add(url);
        }
        ThreadCancelSource = new CancellationTokenSource();
        Thread = new Thread(Run);
    }


    private readonly List<string> Urls = new();
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

        var prefix = request.Url?.GetLeftPart(UriPartial.Authority);
        var imageUrl = prefix + "/hello.bmp";
        var id = macAddress.Replace(":", "").ToUpperInvariant();
        var apiKey = id;

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

        var prefix = request.Url?.GetLeftPart(UriPartial.Authority);
        var imageUrl = prefix + "/hello.bmp";  // TODO: use the actual image URL
        var fileName = DateTime.Now.ToString("yyyy-MM-dd'T'HH-mm-ss", CultureInfo.InvariantCulture) + ".bmp";
        var refreshRate = 300;

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
