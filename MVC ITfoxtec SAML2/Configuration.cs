namespace MVC_ITfoxtec_SAML2
{
    public static class Configuration
    {
        public const string CFS_ENDPOINT = "https://<CFS_ADDRESS>/cfs/Saml2/<TENANT>/<APPLICATION_ID>";
        public const string ISSUER = "https://<CFS_ADDRESS>";
        public const string PATH_TO_CERTIFICATE = "~/App_Data/certificate.cer";
    }
}