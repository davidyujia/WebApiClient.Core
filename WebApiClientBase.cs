using System.Net;

namespace WebApiClient.Core
{
    public abstract class WebApiClientBase
    {
        private readonly HttpClient _client;

        public WebApiClientBase(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Parse object from HttpResponseMessage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        protected abstract Task<T> ParseResponseToModelAsync<T>(HttpResponseMessage response);

        /// <summary>
        /// Parse error message from HttpResponseMessage.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        protected abstract Task<object> ParseResponseErrorMessage(HttpResponseMessage response);

        /// <summary>
        /// Parse data object to HttpContent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected abstract HttpContent ParseDataToHttpContent<T>(T data);

        private static string UriParse(string uri) => uri.StartsWith("/") ? uri[1..] : uri;

        private async Task<HttpResponseMessage> SendAsync(string uri, Func<string, Task<HttpResponseMessage>> func)
        {
            HttpResponseMessage response;

            var requestUri = UriParse(uri);

            try
            {
                response = await func(requestUri);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new WebApiClientException("Send HTTP Request had exception.", e, _client);
            }

            if (response.IsSuccessStatusCode || !IsThrowHttpStatusException(response.StatusCode))
            {
                return response;
            }

            var details = await ParseResponseErrorMessage(response);

            throw CreateException(response, details);
        }

        private static WebApiClientHttpResponseException CreateException(HttpResponseMessage response, object details)
        {
            return new WebApiClientHttpResponseException(details, GetExceptionMessage(response), response.StatusCode);
        }

        /// <summary>
        /// Is this HTTP Status Code throw exception?
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsThrowHttpStatusException(HttpStatusCode statusCode)
        {
            return true;
        }

        private static string GetExceptionMessage(HttpResponseMessage response)
        {
            return $"API call failed. (HTTP{(int)response.StatusCode}: {response.RequestMessage.RequestUri})";
        }

        /// <summary>
        /// HTTP GET
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> GetAsyncBase(string uri)
        {
            return await SendAsync(uri, async requestUri => await _client.GetAsync(requestUri));
        }

        /// <summary>
        /// HTTP GET
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        protected async Task<T> GetAsyncBase<T>(string uri)
        {
            var response = await GetAsyncBase(uri);

            var result = await ParseResponseToModelAsync<T>(response);

            return result;
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">The URI.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> PostAsyncBase<T>(string uri, T data)
        {
            var content = ParseDataToHttpContent(data);

            return await SendAsync(uri, async requestUri => await _client.PostAsync(requestUri, content));
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="uri">The URI.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected async Task<T> PostAsyncBase<T, TData>(string uri, TData data)
        {
            var response = await PostAsyncBase(uri, data);

            var result = await ParseResponseToModelAsync<T>(response);

            return result;
        }

        /// <summary>
        /// HTTP PUT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">The URI.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> PutAsyncBase<T>(string uri, T data)
        {
            var content = ParseDataToHttpContent(data);

            return await SendAsync(uri, async requestUri => await _client.PutAsync(requestUri, content));
        }

        /// <summary>
        /// HTTP PUT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="uri">The URI.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected async Task<T> PutAsyncBase<T, TData>(string uri, TData data)
        {
            var response = await PutAsyncBase(uri, data);

            var result = await ParseResponseToModelAsync<T>(response);

            return result;
        }

        /// <summary>
        /// HTTP DELETE
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> DeleteAsyncBase(string uri)
        {
            return await SendAsync(uri, async requestUri => await _client.DeleteAsync(requestUri));
        }

        /// <summary>
        /// HTTP DELETE
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        protected async Task<T> DeleteAsyncBase<T>(string uri)
        {
            var response = await DeleteAsyncBase(uri);

            var result = await ParseResponseToModelAsync<T>(response);

            return result;
        }
    }
}