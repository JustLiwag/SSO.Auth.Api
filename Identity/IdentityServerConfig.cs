using Duende.IdentityServer;
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
                ClientName = "VAMS System",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,

                ClientSecrets = { new Secret("vams_secret".Sha256()) },

                RedirectUris = { "https://localhost:5003/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "vams_api"
                },

                AllowOfflineAccess = true
            },
            new Client
            {
                ClientId = "hris_client",
                ClientName = "HRIS System",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,

                ClientSecrets = { new Secret("hris_secret".Sha256()) },

                RedirectUris = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "hris_api"
                },

                AllowOfflineAccess = true
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("vams_api", "VAMS API"),
            new ApiScope("hris_api", "HRIS API")
        };

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };
}
