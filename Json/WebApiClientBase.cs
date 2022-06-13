using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace WebApiClient.Core.Json
{
    /// <summary>
    /// Web API client JSON base.
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    public abstract class WebApiClientBase<TError> : WebApiClientBase
    {
        private readonly Lazy<JsonSerializerOptions> _jsonSerializerOptions;

        protected WebApiClientBase(HttpClient client) : base(client)
        {
            _jsonSerializerOptions = new Lazy<JsonSerializerOptions>(CreateJsonSerializerOptions);
        }

        /// <summary>
        /// Get default JsonSerializerOptions for System.Text.Json.
        /// </summary>
        /// <returns></returns>
        protected virtual JsonSerializerOptions CreateJsonSerializerOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Parse object from HttpResponseMessage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        protected override async Task<T> ParseResponseToModelAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStreamAsync();

            if (response.IsSuccessStatusCode && json.Length != 0)
            {
                return await JsonSerializer.DeserializeAsync<T>(json, _jsonSerializerOptions.Value);
            }

            var genericType = typeof(T);

            if (!genericType.IsArray)
            {
                return default;
            }

            var elementType = genericType.GetElementType();

            if (elementType != null && Array.CreateInstance(elementType, 0) is T arrayType)
            {
                return arrayType;
            }

            return default;
        }

        /// <summary>
        /// Parse data object to HttpContent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected override HttpContent ParseDataToHttpContent<T>(T data)
        {
            if (data is HttpContent content)
            {
                return content;
            }

            var json = JsonSerializer.Serialize(data, _jsonSerializerOptions.Value);

            return new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        /// <summary>
        /// Parse error message from HttpResponseMessage.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        protected override async Task<object> ParseResponseErrorMessage(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStreamAsync();

            if (json.Length == 0)
            {
                return default;
            }

            try
            {
                return await JsonSerializer.DeserializeAsync<TError>(json, _jsonSerializerOptions.Value);
            }
            catch
            {
                json.Position = 0;
                using var reader = new StreamReader(json);
                return await reader.ReadToEndAsync();
            }
        }
    }
}