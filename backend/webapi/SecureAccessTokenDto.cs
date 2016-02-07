namespace Fis.Common.Web.SecuredDownload
{
    public class SecureAccessTokenDto
    {
        public string Token { get; set; }

        public long Exp { get; set; }
    }
}