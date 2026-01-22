using Duende.IdentityServer.Models;

namespace SSO.Auth.Api.Identity
{
    /// Static configuration for IdentityServer used for development/demo scenarios.
    /// Contains in-memory client, scope and identity resource definitions.
    /// Replace with persisted stores for production scenarios.
    public static class IdentityServerConfig
    {
        // ===========================
        // CLIENTS (HRIS, PAYROLL, ETC)
        // ===========================
        /// In-memory client definitions. Example: an HRIS client that uses the
        /// Resource Owner Password grant to obtain tokens.
        public static IEnumerable<Client> Clients =>
    new List<Client>
    {
        // Existing API client (Resource Owner Password)
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
        },

        // App A - browser app
        new Client
        {
            ClientId = "appA_client",
            ClientName = "App A",
            AllowedGrantTypes = GrantTypes.Code,   // Authorization Code Flow
            RequirePkce = true,                     // required for browser apps
            RequireClientSecret = false,            // public client
            RedirectUris = { "https://localhost:5298/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5298/signout-callback-oidc" },
            AllowedScopes = { "openid", "profile", "sso_api" },
            AllowOfflineAccess = true,
            RequireConsent = true                   // will ask for authorization if already logged in
        },

        // App B - browser app
        new Client
        {
            ClientId = "appB_client",
            ClientName = "App B",
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false,
            RedirectUris = { "https://localhost:5004/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5004/signout-callback-oidc" },
            AllowedScopes = { "openid", "profile", "sso_api" },
            AllowOfflineAccess = true,
            RequireConsent = true
        }
    };


        // ===========================
        // API SCOPES
        // ===========================
        /// API scopes represent protected APIs that clients can request access to.
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("sso_api", "SSO Protected API")
            };

        // ===========================
        // IDENTITY RESOURCES
        // ===========================
        /// Standard OpenID Connect identity scopes exposed to clients.
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
    }
}
