using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

class SimpleServer {
    static async Task Main(string[] args) {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Server started at http://localhost:8080/");

        while (true) {
            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            string filename = request.Url.LocalPath.TrimStart('/');
            if (string.IsNullOrEmpty(filename)) filename = "index.html";

            string path = Path.Combine(Directory.GetCurrentDirectory(), filename);

            if (File.Exists(path)) {
                try {
                    byte[] buffer = await File.ReadAllBytesAsync(path);
                    response.ContentLength64 = buffer.Length;
                    string extension = Path.GetExtension(path).ToLower();
                    response.ContentType = extension switch {
                        ".html" => "text/html",
                        ".css" => "text/css",
                        ".js" => "application/javascript",
                        ".png" => "image/png",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        _ => "application/octet-stream"
                    };
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                } catch (Exception ex) {
                    Console.WriteLine("Error: " + ex.Message);
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            } else {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            response.Close();
        }
    }
}
