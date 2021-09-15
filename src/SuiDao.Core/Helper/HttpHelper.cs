using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SuiDao.Client
{
    public static class HttpHelper
    {
        public static async Task<string> PostAsJsonAsync(string uri, string strContent)
        {
            using (var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                AutomaticDecompression = DecompressionMethods.None
            })

            using (var httpclient = new HttpClient(handler))
            {
                httpclient.BaseAddress = new Uri(uri);
                var content = new StringContent(strContent, Encoding.UTF8, "application/json");

                // api1.suidao.io
                var response = httpclient.PostAsync(uri, content).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
