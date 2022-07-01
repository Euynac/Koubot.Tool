using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Koubot.Tool.Extensions;
using Koubot.Tool.Interfaces;
using Koubot.Tool.Web.RateLimiter;

namespace Koubot.Tool.Web;
/// <summary>
/// For create HTTP request and handle response easily.
/// <para>Work in progress</para>
/// </summary>
public class KouHttp :IKouErrorMsg
{
    /// <summary>
    /// Url to send request to.
    /// </summary>
    public string Url { get; }
    /// <summary>
    /// Body content encoding.
    /// </summary>
    public Encoding BodyEncoding { get; set; } = Encoding.UTF8;
    /// <summary>
    /// Response encoding.
    /// </summary>
    public Encoding ResponseEncoding { get; set; } = Encoding.UTF8;
    /// <summary>
    /// MaxQPS to send request.
    /// </summary>
    public double? MaxQPS { get; private set; }
    /// <summary>
    /// The web request object.
    /// </summary>
    private readonly HttpWebRequest _request;
    /// <summary>
    /// The web response object.
    /// </summary>
    private HttpWebResponse? _response;
   
    /// <summary>
    /// The request body.
    /// </summary>
    public string? RequestBody { get; private set; }

    private byte[]? _requestBuffer;

    private KouHttp(string url)
    {
        Url = url;
        _request = (HttpWebRequest)WebRequest.Create(url);
        _request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
    }

    /// <summary>
    /// The factory to create KouHttp.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="timeout">Request timeout (ms).</param>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static KouHttp Create(string url, int timeout = 10000, Action<HttpWebRequest>? setting = null)
    {
        var requester = new KouHttp(url);
        requester._request.CookieContainer ??= new CookieContainer();
        requester._request.Timeout = timeout;
        setting?.Invoke(requester._request);
        return requester;
    }
    /// <summary>
    /// Set Max QPS. Use LeakyBucket algorithm to send request based on request host.
    /// </summary>
    /// <param name="maxQPS"></param>
    /// <returns></returns>
    public KouHttp SetQPS(double maxQPS)
    {
        MaxQPS = maxQPS;
        return this;
    }
    /// <summary>
    /// Set request body.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="contentType">will auto add content type into hear</param>
    /// <returns></returns>
    public KouHttp SetBody(string? body = null, WebContentType contentType = WebContentType.General)
    {
        _request.ContentType = contentType.GetDescription();
        RequestBody = body;
        return this;
    }
    /// <summary>
    /// Set request body from binary file, and auto set WebContentType to application/octet-stream
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public KouHttp SetBinaryBody(byte[] bytes)
    {
        _request.ContentType = WebContentType.Stream.GetDescription();
        _requestBuffer = bytes;
        return this;
    }
    /// <summary>
    /// Set request body use obj in json format.
    /// </summary>
    /// <param name="content">class obj or anonymous class obj and will automatically serialize to json</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public KouHttp SetJsonBody(object content, Action<JsonSerializerOptions>? options = null)
    {
        var option = new JsonSerializerOptions();
        options?.Invoke(option);
        RequestBody = JsonSerializer.Serialize(content, option);
        _request.ContentType = WebContentType.Json.GetDescription();
        return this;
    }

    /// <summary>
    /// Add cookie to current request. (Domain use current request uri, and path set to '/')
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public KouHttp AddCookie(string name, string value)
    {
        _request.CookieContainer.Add(new Uri(Url), new Cookie(name, value));
        return this;
    }

    public KouHttp AddHeader(IReadOnlyDictionary<string, string> dict)
    {
        foreach (var (key, value) in dict)
        {
            _request.Headers.Add(key, value);
        }

        return this;
    }

    public KouHttp ClearHeader()
    {
        _request.Headers.Clear();
        return this;
    }

    /// <summary>
    /// Send request by post method asynchronously.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public async Task<Response> SendRequestAsync(HttpMethods method) => 
        await Task.Factory.StartNew(()=>SendRequest(method));

    /// <summary>
    /// Send request by post method.
    /// </summary>
    /// <returns></returns>
    public Response SendRequest(HttpMethods method)
    {
        if (MaxQPS != null)
        {
            if (_request.Host.IsNullOrWhiteSpace()) return new Response(WebExceptionStatus.NameResolutionFailure);
            using var limiter = new LeakyBucketRateLimiter(_request.Host, MaxQPS.Value);
            if (!limiter.CanRequest())
            {
                this.ReturnError(limiter.ErrorMsg);
                return new Response(WebExceptionStatus.Timeout);
            }

            return SendRequestInner(method);
        }

        return SendRequestInner(method);
    }
    /// <summary>
    /// Send request by post method.
    /// </summary>
    /// <returns></returns>
    private Response SendRequestInner(HttpMethods method)
    {
        _request.Method = method.ToString();
        if (RequestBody != null || _requestBuffer != null)
        {
            _requestBuffer ??= BodyEncoding.GetBytes(RequestBody!);
            _request.ContentLength = _requestBuffer.Length;
            _request.GetRequestStream().Write(_requestBuffer, 0, _requestBuffer.Length);
        }

        WebExceptionStatus? exceptionStatus = null;
        try
        {
            
            _response = (HttpWebResponse)_request.GetResponse();
        }
        catch (WebException ex)
        {
            _response = (HttpWebResponse?)ex.Response;
            exceptionStatus = ex.Status;
        }
        
        var response = _response?.GetResponseStream();
        var body = "";
        if (response != null)
        {
            using StreamReader reader = new StreamReader(response, ResponseEncoding);
            body = reader.ReadToEnd();
        }
        
        return new Response(body, _response)
        {
            ExceptionStatus = exceptionStatus
        };
    }

    /// <summary>
    /// KouHttp Response
    /// </summary>
    public class Response
    {
        /// <summary>
        /// 错误状态
        /// </summary>
        public WebExceptionStatus? ExceptionStatus { get; internal set; }

        internal Response(WebExceptionStatus status)
        {
            ExceptionStatus = status;
            Body = "";
            _response = null;
        }

        internal Response(string body, HttpWebResponse? response)
        {
            Body = body;
            _response = response;
        }

        private readonly HttpWebResponse? _response;
        /// <summary>
        /// Response body.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// The cookies the response given.
        /// </summary>
        public CookieCollection? Cookies => _response?.Cookies;

        /// <summary>
        /// The headers the response given.
        /// </summary>
        public WebHeaderCollection? Headers => _response?.Headers;

        public bool HasError => ExceptionStatus != null;
    }

    /// <summary>
    /// Error message.
    /// </summary>
    public string? ErrorMsg { get; set; }
}