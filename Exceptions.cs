using System;
using System.Net;

namespace WebApiClient.Core
{
    public class WebApiClientException : Exception
    {
        public HttpClient HttpClient { get; }

        internal WebApiClientException(string message, Exception exception, HttpClient httpClient) : base(message, exception)
        {
            HttpClient = httpClient;
        }
    }

    public class WebApiClientHttpResponseException : Exception
    {
        public object ResponseDetails { get; }

        public HttpStatusCode StatusCode { get; }

        internal WebApiClientHttpResponseException(object responseErrorDetails, string message, HttpStatusCode statusCode) : base(message)
        {
            ResponseDetails = responseErrorDetails;
            StatusCode = statusCode;
        }
    }
}