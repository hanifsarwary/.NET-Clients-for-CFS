using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace MVC_OpenID_Connect.Models
{
    public class OpenIdConnectClient : OAuth2Client
    {
        public string ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string ResourceEndpoint { get; set; }
        public string Scope { get; set; }

        public OpenIdConnectClient(string applicationId)
            : base("OpenIDConnect")
        {
            ApplicationId = applicationId;
            Scope = "openid";
        }

        public override AuthenticationResult VerifyAuthentication(HttpContextBase context, Uri returnPageUrl)
        {
            string code = context.Request.QueryString["code"];
            if (string.IsNullOrEmpty(code))
                return AuthenticationResult.Failed;

            string accessToken = QueryAccessToken(returnPageUrl, code);
            if (accessToken == null)
                return AuthenticationResult.Failed;

            IDictionary<string, string> userData = GetUserData(accessToken);
            if (userData == null)
                return AuthenticationResult.Failed;

            string fullname = userData["fullname"];
            string id = userData["id"];

            return new AuthenticationResult(true, ProviderName, id, fullname, userData);
        }

        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            UriBuilder builder = new UriBuilder(AuthorizationEndpoint);
            builder.AppendQueryArgument("client_id", ApplicationId);
            builder.AppendQueryArgument("response_type", "code");
            builder.AppendQueryArgument("scope", Scope);
            builder.AppendQueryArgument("redirect_uri", returnUrl.AbsoluteUri);
            builder.AppendQueryArgument("nonce", Guid.NewGuid().ToString().Replace("-", ""));

            return builder.Uri;
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            UriBuilder builder = new UriBuilder(ResourceEndpoint);
            builder.AppendQueryArgument("schema", Scope);

            var request = WebRequest.Create(builder.ToString());
            request.Headers["Authorization"] = string.Concat("Bearer ", accessToken);

            OpenIdConnectGraph data;
            using (WebResponse response = request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                data = JsonConvert.DeserializeObject<OpenIdConnectGraph>(reader.ReadToEnd());
            }

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.AddItemIfNotEmpty("fullname", data.Name);
            dictionary.AddItemIfNotEmpty("id", data.Email);
            dictionary.AddItemIfNotEmpty("email", data.Email);
            dictionary.AddItemIfNotEmpty("firstname", data.Firstname);
            dictionary.AddItemIfNotEmpty("lastname", data.Lastname);
            return dictionary;
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            StringBuilder postData = new StringBuilder();
            postData.AppendFormat("client_id={0}", ApplicationId);
            postData.AppendFormat("&redirect_uri={0}", HttpUtility.UrlEncode(returnUrl.ToString()));
            postData.AppendFormat("&client_secret={0}", ApplicationSecret);
            postData.AppendFormat("&grant_type={0}", "authorization_code");
            postData.AppendFormat("&code={0}", authorizationCode);

            var webRequest = (HttpWebRequest)WebRequest.Create(TokenEndpoint);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter sw = new StreamWriter(webRequest.GetRequestStream()))
                sw.Write(postData.ToString());

            using (WebResponse webResponse = webRequest.GetResponse())
            using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var token = JsonConvert.DeserializeObject<TokenGraph>(reader.ReadToEnd());
                return token.AccessToken;
            }
        }
    }

    public static class DictionaryExtensions
    {
        public static void AddItemIfNotEmpty(this IDictionary<string, string> dictionary, string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (!string.IsNullOrEmpty(value))
                dictionary[key] = value;
        }
    }

    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    [JsonObject]
    public class TokenGraph
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }

    [JsonObject]
    public class OpenIdConnectGraph
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "given_name")]
        public string Firstname { get; set; }

        [JsonProperty(PropertyName = "family_name")]
        public string Lastname { get; set; }
    }
}