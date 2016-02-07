using System;
using System.Runtime.Caching;
using System.Security.Cryptography;

using System.Security.Principal;
using System.Text;
using System.Web.Http.Controllers;

namespace Fis.Common.Web.SecuredDownload
{
    public class SecureTokenService
    {
        static SecureTokenService()
        {
            TokenCache = new MemoryCache("SecureTokenService");
            Salt = CreateSalt(512);
        }

        public static string Salt { get; set; }

        public static MemoryCache TokenCache { get; set; }

        public bool ApplyAccessToken(HttpActionContext actionContext, Uri requestUri, string accessToken)
        {
            var cachedElement = TokenCache.GetCacheItem(accessToken);

            if (cachedElement != null)
            {
                var cachedValue = cachedElement.Value as StoredAuthentication;

                if (cachedValue != null && string.Equals(requestUri.AbsoluteUri, cachedValue.RequestUri, StringComparison.InvariantCultureIgnoreCase))
                {
                    actionContext.RequestContext.Principal = cachedValue.Principal;

                    return true;
                }
            }

            return false;
        }

        public SecureAccessTokenDto CreateToken(IPrincipal principal, Uri requestUri)
        {
            var token = this.CreateHash(principal, requestUri);
            var expiration = DateTime.UtcNow.AddMinutes(5);

            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes((expiration - DateTime.UtcNow).TotalMinutes) };

            var data = new StoredAuthentication { RequestUri = requestUri.AbsoluteUri, Principal = principal};

            TokenCache.Add(new CacheItem(token, data), policy);

            return new SecureAccessTokenDto { Token = Uri.EscapeDataString(token), Exp = expiration.AsUnixTimeStamp() };
        }

        public string CreateHash(IPrincipal principal, Uri requestUri)
        {

            var concat = string.Format("{0}::{1}::{2}", principal.Identity.Name, requestUri.AbsoluteUri, Salt);
            var hashBytes = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(concat));

            var base64Hash = Convert.ToBase64String(hashBytes);

            return base64Hash;
        }

        private static string CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }
    }

    public class StoredAuthentication
    {
        public string RequestUri { get; set; }

        public IPrincipal Principal { get; set; }
    }
}