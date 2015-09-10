using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace WebApiClient
{

  class WebApiClient
  {
    static readonly string s_systemDir = "SupplierTracking"; // appended to SessionBase.BaseDatabasePath
    static readonly string s_wepApiServer = "http://localhost:4098/";
    static readonly string graphApi = "api/graph";
    public int UseWebApi()
    {
      HttpClient client = new HttpClient();
      //client.BaseAddress = new Uri(s_wepApiServer);
      Uri url = new Uri(s_wepApiServer + graphApi).AddQuery("path", s_systemDir);
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
// get a GraphJson json encoded VelocityGraph from WebApi server database "SupplierTracking"
      string grapJson = null;
      using (HttpResponseMessage response = client.GetAsync(url).Result)
      {
        if (response.IsSuccessStatusCode)
        {
          grapJson = response.Content.ReadAsStringAsync().Result;
        }
        else
        {
          Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
        }
      }
      url = new Uri(s_wepApiServer + graphApi).AddQuery("path", s_systemDir).AddQuery("id", "5");
      HttpContent contentPost = new StringContent(grapJson, Encoding.UTF8, "application/json");
      using (HttpResponseMessage response = client.PostAsync(url, contentPost).Result)
      {
        if (response.IsSuccessStatusCode)
        {
          //grapJson = response.Content.ReadAsStringAsync().Result;
        }
        else
        {
          Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
        }
      }
      return 0;
    }

    static int Main(string[] args)
    {
      WebApiClient webClient = new WebApiClient();
      return webClient.UseWebApi();
    }
  }

  public static class HttpExtensions
  {
    public static Uri AddQuery(this Uri uri, string name, string value)
    {
      var ub = new UriBuilder(uri);

      // decodes urlencoded pairs from uri.Query to HttpValueCollection
      var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

      httpValueCollection.Add(name, value);

      // urlencodes the whole HttpValueCollection
      ub.Query = httpValueCollection.ToString();

      return ub.Uri;
    }
  }
}
