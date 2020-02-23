using System;
using System.Net;

namespace LegendasTvHandler
{
    public class WebClientPlus : WebClient
    {
        public int Timeout { get; set; }
        public bool IgnoreRedirects { get; set; }
        public CookieContainer Cookies { get; } = new CookieContainer();

        public WebClientPlus() : this(20000)
        {
        }
        public WebClientPlus(int timeout)
        {
            Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);

            if (request != null)
            {
                request.Timeout = Timeout;
            }

            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = Cookies;
                (request as HttpWebRequest).AllowAutoRedirect = !IgnoreRedirects;
            }

            return request;
        }

        private static bool TryAddCookie(WebRequest webRequest, Cookie cookie)
        {
            HttpWebRequest httpRequest = webRequest as HttpWebRequest;

            if (httpRequest == null)
            {
                return false;
            }

            if (httpRequest.CookieContainer == null)
            {
                httpRequest.CookieContainer = new CookieContainer();
            }

            httpRequest.CookieContainer.Add(cookie);

            return true;
        }
    }
}
