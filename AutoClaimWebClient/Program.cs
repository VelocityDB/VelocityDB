using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace AutoClaimWebClient
{
  class WebApiClient
  {
    static readonly string s_wepApiServer = "http://localhost:25148/";
    static readonly string autoClaimApi = "api/MitchellClaimTypes";

    static async Task<List<MitchellClaimType>> Claims(HttpClient client)
    {
      HttpResponseMessage response = await client.GetAsync(autoClaimApi);
      if (response.IsSuccessStatusCode)
        return await response.Content.ReadAsAsync<List<MitchellClaimType>>();
      else
        Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
      return null;
    }

    static async Task<MitchellClaimType> Claim(HttpClient client, int id)
    {
      HttpResponseMessage response = await client.GetAsync(autoClaimApi + "/" + id);
      if (response.IsSuccessStatusCode)
        return await response.Content.ReadAsAsync<MitchellClaimType>();
      else
        Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
      return null;
    }

    static async Task RunAsync()
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.BaseAddress = new Uri(s_wepApiServer);
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
          MitchellClaimType claim = null;
          HttpResponseMessage response;

          // Get all Claims
          List<MitchellClaimType> claims = Claims(client).Result;
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(MitchellClaimType));
          string[] files = Directory.GetFiles("../../NewClaims", "*.xml");

          // Add New Claims
          foreach (string fileName in files)
          {
            using (StreamReader reader = new StreamReader(fileName))
            {
              claim = (MitchellClaimType)xmlSerializer.Deserialize(reader);
              response = await client.PostAsJsonAsync<MitchellClaimType>(autoClaimApi, claim).ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode());
            }
          }
          claims = Claims(client).Result;
          claim = Claim(client, claims.First().MitchellClaimTypeId).Result;

          // Update Claims
          files = Directory.GetFiles("../../ClaimUpdates", "*.xml");
          foreach (string fileName in files)
          {
            using (StreamReader reader = new StreamReader(fileName))
            {
              MitchellClaimType claimUpdates = (MitchellClaimType)xmlSerializer.Deserialize(reader);
              claim = (from c in claims where c.ClaimNumber == claimUpdates.ClaimNumber select c).FirstOrDefault();
              if (claimUpdates.MitchellClaimTypeId != claim.MitchellClaimTypeId)
                claimUpdates.MitchellClaimTypeId = claim.MitchellClaimTypeId;
              // more to do response = await client.PutAsJsonAsync<MitchellClaimType>(autoClaimApi + "/" + claim.MitchellClaimTypeId, claimUpdates).ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode());
            }
          }

          // Read a Claim
          claim = Claim(client, claim.MitchellClaimTypeId).Result;

          // Delete a claim
          // more to do response = await client.DeleteAsync(autoClaimApi + "/" + claim.MitchellClaimTypeId).ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode());

          claim = Claim(client, claim.MitchellClaimTypeId).Result;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    static void Main(string[] args)
    {
      RunAsync().Wait();
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
