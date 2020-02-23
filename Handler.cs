using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace LegendasTvHandler
{
    public class Handler : IDisposable
    {
        private string rootUri = string.Empty;
        private List<System.Net.Cookie> sessionCookies = new List<System.Net.Cookie>();

        public Handler(string rootUri)
        {
            this.rootUri = rootUri;

            sessionCookies.Add(new System.Net.Cookie { Name = "popup-mensagem", Value = "yes", Path = "/", Domain = "legendas.tv" });
            sessionCookies.Add(new System.Net.Cookie { Name = "popup-likebox", Value = "yes", Path = "/", Domain = "legendas.tv" });
        }

        public bool Login(string username, string password)
        {
            using (WebClientPlus webClient = new WebClientPlus())
            {
                try
                {
                    foreach (System.Net.Cookie sc in sessionCookies)
                        webClient.Cookies.Add(sc);

                    var dadosLogin = new NameValueCollection();
                    dadosLogin.Add("_method", "POST");
                    dadosLogin.Add("data[User][username]", username);
                    dadosLogin.Add("data[User][password]", password);
                    dadosLogin.Add("data[lembrar]", "on");

                    webClient.IgnoreRedirects = true;
                    webClient.Timeout = 10000;

                    webClient.UploadValues(rootUri + "/login", "POST", dadosLogin);

                    var cookie = webClient.ResponseHeaders["Set-Cookie"];

                    string cookieName = "au";
                    string cookieValue = cookie.Substring(cookie.LastIndexOf("au=") + 3, cookie.IndexOf(";", cookie.LastIndexOf("au=")) - (cookie.LastIndexOf("au=") + 3));
                    string cookiePath = "/";
                    string cookieDomain = "legendas.tv";

                    sessionCookies.Add(new System.Net.Cookie(cookieName, cookieValue, cookiePath, cookieDomain));

                    cookieName = "PHPSESSID";
                    cookieValue = cookie.Substring(cookie.LastIndexOf("PHPSESSID=") + 3, cookie.IndexOf(";", cookie.LastIndexOf("PHPSESSID=")) - (cookie.LastIndexOf("PHPSESSID=") + 3));

                    sessionCookies.Add(new System.Net.Cookie(cookieName, cookieValue, cookiePath, cookieDomain));

                    return true;
                }
                catch 
                {
                    return false;
                }
            }
        }

        public Subtitle[] Search(string movieName)
        {
            List<Subtitle> returnValue = new List<Subtitle>();

            movieName = movieName.Replace(":", "").Replace("-", "");
            string[] keyWords = movieName.ToLowerInvariant().Split(' ');

            using (WebClientPlus webClient = new WebClientPlus())
            {
                foreach (System.Net.Cookie sc in sessionCookies)
                    webClient.Cookies.Add(sc);

                webClient.IgnoreRedirects = true;
                webClient.Timeout = 10000;

                string pageContent = string.Empty;
                Stream responseStream = webClient.OpenRead(rootUri + "/legenda/busca/" + System.Web.HttpUtility.UrlEncode(movieName) + "/-/-/0/-");
                using (StreamReader sr = new StreamReader(responseStream))
                    pageContent = sr.ReadToEnd();

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(pageContent);

                HtmlNodeCollection articleNodes = htmlDoc.DocumentNode.SelectNodes("//article");
                foreach (HtmlNode articleNode in articleNodes)
                {
                    HtmlNodeCollection divNodes = articleNode.SelectNodes("div");
                    foreach (HtmlNode divNode in divNodes)
                    {
                        Subtitle subtitle = new Subtitle();
                        subtitle.Id = divNode.SelectSingleNode("span").InnerText;
                        subtitle.Name = divNode.SelectSingleNode("div/p/a").InnerText;
                        subtitle.Link = divNode.SelectSingleNode("div/p/a").Attributes["href"].Value;
                        subtitle.Downloads = divNode.SelectSingleNode("div/p[last()]").InnerText.Replace("downloads", "").Replace("nota", "").Split(',')[0].Trim();
                        subtitle.Score = divNode.SelectSingleNode("div/p[last()]").InnerText.Replace("downloads", "").Replace("nota", "").Split(',')[1].Trim();
                        subtitle.Language = divNode.SelectSingleNode("img").Attributes["title"].Value;

                        if (keyWords.All(kw => subtitle.Name.ToLowerInvariant().Contains(kw)))
                            returnValue.Add(subtitle);
                    }
                }
            }

            return returnValue.ToArray();
        }

        public string Download(string uri, string destinationFolder)
        {
            string returnValue = string.Empty;
            string[] uriParts = uri.Split('/');

            using (WebClientPlus webClient = new WebClientPlus())
            {
                foreach (System.Net.Cookie sc in sessionCookies)
                    webClient.Cookies.Add(sc);

                webClient.IgnoreRedirects = true;
                webClient.Timeout = 10000;

                string subtitleId = uriParts[2];

                webClient.DownloadData(rootUri + "/downloadarquivo/" + subtitleId);
                string location = webClient.ResponseHeaders["Location"];
                uriParts = location.Split('/');
                string destinationFileName = uriParts[uriParts.Length - 1];

                returnValue = Path.Combine(destinationFolder, destinationFileName);
                webClient.DownloadFile(location, returnValue);
            }

            return returnValue;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
