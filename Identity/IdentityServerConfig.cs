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
                new Client
                {
                    ClientId = "hris_client",
                    ClientName = "HRIS System",

                    // Resource Owner Password grant
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        // Keep secrets out of source in production (use secret store)
                        new Secret("hris_secret".Sha256())
                    },

                    // Scopes requested by the client
                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "sso_api"
                    },

                    // Include user claims (from CustomPasswordValidator) in token
                    AlwaysIncludeUserClaimsInIdToken = true,

                    // Optional: allow refresh tokens
                    AllowOfflineAccess = true
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
