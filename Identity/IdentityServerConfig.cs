using Duende.IdentityServer.Models;

namespace SSO.Auth.Api.Identity;

public static class IdentityServerConfig
{
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "vams_client",
                ClientName = "VAMS Client",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,

                ClientSecrets =
                {
                    new Secret("vams_secret".Sha256())
                },

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
                },

                AllowOfflineAccess = true
            }

        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("vams_api", "VAMS API")
        };

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };
}
