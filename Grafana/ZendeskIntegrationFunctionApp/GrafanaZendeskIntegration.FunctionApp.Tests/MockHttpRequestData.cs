namespace GrafanaZendeskIntegration.FunctionApp.Tests
{
    using System.Net;
    using System.Security.Claims;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;

    public class MockHttpRequestData : HttpRequestData
    {
        public MockHttpRequestData(FunctionContext functionContext, Stream body, string method) : base(functionContext)
        {
            Body = body;
            Method = method;
        }

        public override Stream Body { get; }

        public override IReadOnlyCollection<IHttpCookie> Cookies { get; }

        public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();

        public override Uri Url { get; }

        public override IEnumerable<ClaimsIdentity> Identities { get; }

        public override string Method { get; }

        public override HttpResponseData CreateResponse()
        {
            return new MockHttpResponseData(FunctionContext);
        }
    }

    public class MockHttpResponseData : HttpResponseData
    {
        public MockHttpResponseData(FunctionContext functionContext) : base(functionContext)
        {
        }

        public override Stream Body { get; set; } = new MemoryStream();

        public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();

        public override HttpStatusCode StatusCode { get; set; }

        public override HttpCookies Cookies { get; }
    }
}
