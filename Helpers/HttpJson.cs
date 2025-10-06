using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetailOnlineFunctions.Helpers
{
    internal class HttpJson
    {
        static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public static async Task<T?> ReadAsync<T>(HttpRequestData req)
        {
            using var s = req.Body;
            return await JsonSerializer.DeserializeAsync<T>(s, _json);
        }

        // Change all methods to be async and return Task<HttpResponseData>
        public static async Task<HttpResponseData> Ok<T>(HttpRequestData req, T body)
            => await WriteAsync(req, HttpStatusCode.OK, body);

        public static async Task<HttpResponseData> Created<T>(HttpRequestData req, T body)
            => await WriteAsync(req, HttpStatusCode.Created, body);

        public static async Task<HttpResponseData> Bad(HttpRequestData req, string message)
            => await TextAsync(req, HttpStatusCode.BadRequest, message);

        public static async Task<HttpResponseData> NotFound(HttpRequestData req, string message = "Not Found")
            => await TextAsync(req, HttpStatusCode.NotFound, message);

        public static async Task<HttpResponseData> NoContent(HttpRequestData req)
        {
            var r = req.CreateResponse(HttpStatusCode.NoContent);
            return await Task.FromResult(r); // No content, just return the response
        }

        public static async Task<HttpResponseData> TextAsync(HttpRequestData req, HttpStatusCode code, string message)
        {
            var r = req.CreateResponse(code);
            r.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await r.WriteStringAsync(message, Encoding.UTF8); // Changed to async
            return r;
        }

        // Changed to async version
        private static async Task<HttpResponseData> WriteAsync<T>(HttpRequestData req, HttpStatusCode code, T body)
        {
            var r = req.CreateResponse(code);
            r.Headers.Add("Content-Type", "application/json; charset=utf-8");
            var json = JsonSerializer.Serialize(body, _json);
            await r.WriteStringAsync(json, Encoding.UTF8); // Changed to async
            return r;
        }
    }
}