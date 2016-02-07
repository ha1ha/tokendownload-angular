using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Newtonsoft.Json.Serialization;

namespace Fis.Common.Web.SecuredDownload
{
    public class RequiresSecureAccessTokenAttribute : ActionFilterAttribute
    {
        private string RequestAccessTokenMediaType = "application/vnd.secureaccesstoken+json";

        public string UrlParameterName { get; private set; }

        public RequiresSecureAccessTokenAttribute(string urlParameterName = "sat")
        {
            this.UrlParameterName = urlParameterName;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (this.UrlContainsAccessToken(actionContext))
            {
                var service = GetSecureTokenService(actionContext);

                var allParams = actionContext.Request.GetQueryNameValuePairs();
                var secureAccessToken = allParams.LastOrDefault(kvp => kvp.Key == this.UrlParameterName).Value;

                var cleanedUri = actionContext.Request.RequestUri.AbsoluteUri;
                var escapeDataString = Uri.EscapeDataString(secureAccessToken);
                    
                cleanedUri = cleanedUri.Replace(this.UrlParameterName + "=" + escapeDataString, "");

                if (cleanedUri.EndsWith("?") || cleanedUri.EndsWith("&"))
                {
                    cleanedUri = cleanedUri.Substring(0, cleanedUri.Length - 1);
                }

                if (service.ApplyAccessToken(actionContext, new Uri(cleanedUri), secureAccessToken))
                {

                }
                else
                {
                    throw new HttpRequestException("Invalid or expired access token provided");
                }
            }
            else if (this.IsAuthorizedAccessTokenRequest(actionContext))
            {
                var service = GetSecureTokenService(actionContext);

                var token = service.CreateToken(actionContext.RequestContext.Principal, actionContext.Request.RequestUri);
                var formatter = new JsonMediaTypeFormatter { SerializerSettings = { ContractResolver = new CamelCasePropertyNamesContractResolver() } };

                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, token, formatter);
            }
            else
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        private static SecureTokenService GetSecureTokenService(HttpActionContext actionContext)
        {
            var dependencyResolver = actionContext.RequestContext.Configuration.DependencyResolver;
            var service = (SecureTokenService)dependencyResolver.GetService(typeof(SecureTokenService));
            return service;
        }

        private bool IsAuthorizedAccessTokenRequest(HttpActionContext actionContext)
        {
            return actionContext.RequestContext.Principal.Identity.IsAuthenticated && actionContext.Request.Headers.Accept.First().MediaType == this.RequestAccessTokenMediaType;
        }

        private bool UrlContainsAccessToken(HttpActionContext actionContext)
        {
            return actionContext.Request.RequestUri.Query.Contains(this.UrlParameterName + "=");
        }
    }
}
