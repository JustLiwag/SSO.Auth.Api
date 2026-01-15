using Duende.IdentityServer.Models;
namespace SSO.Auth.Api.Identity
{

    public static class IdentityServerConfig
    {
        // ===========================
        // CLIENTS (HRIS, PAYROLL, ETC)
        // ===========================
        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
            new Client
            {
                ClientId = "hris_client",
                ClientName = "HRIS System",

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                ClientSecrets =
                {
                    new Secret("hris_secret".Sha256())
                },

                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "sso_api"
                }
            }
            };

        // ===========================
        // API SCOPES
        // ===========================
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
            new ApiScope("sso_api", "SSO Protected API")
            };

        // ===========================
        // IDENTITY RESOURCES
        // ===========================
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
            };
    }

}
