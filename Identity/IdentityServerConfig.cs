using Duende.IdentityServer.Models;

namespace SSO.Auth.Api.Identity
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client
                {
                    ClientId = "vams_client",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =
                    {
                        "https://localhost:5003/signin-oidc"
                    },

                    PostLogoutRedirectUris =
                    {
                        "https://localhost:5003/signout-callback-oidc"
                    },

                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "vams_api"
                    }
                }


            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("vams_api", "VAMS API")
            };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
            };
    }

}
