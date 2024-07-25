using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace EthereumIntegration
{
    public static class RemoteDataFetcher
    {
        private static readonly Lazy<HttpClient> lazyHttpClient = new(() => new HttpClient());

        private static HttpClient HttpClient => lazyHttpClient.Value; //get

        public static async Task<string> FetchAbiFromRemoteUrlAsync(string url)
        {
            try
            {
                var response = await HttpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                return json["abi"]?.ToString() ?? throw new Exception("Incorrect ABI file: property 'abi' not found");
            }
            catch (HttpRequestException httpRequestException)
            {
                throw new Exception("HTTP request error while fetching ABI.", httpRequestException);
            }
            catch (JsonReaderException jsonReaderException)
            {
                throw new Exception("JSON parsing error while fetching ABI.", jsonReaderException);
            }
        }
    }
}
