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
                ClientName = "VAMS",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris = { "https://localhost:5003/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5003/" },

                AllowedScopes =
                {
                    "openid",
                    "profile"
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
