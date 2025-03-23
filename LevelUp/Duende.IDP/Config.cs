using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Duende.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(), //user identifier
            new IdentityResources.Profile(), //user profile

            //custom identity resource for roles
            new IdentityResource("roles", "Your roles", new[] { "role" })
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("demowebapi", "Demo Web API", new[] { "role" })
            {
                Scopes = { "demowebapi.fullaccess" }
            },

            new ApiResource("efcorewebapiclient", "EFCoreWebAPI")
            {
                Scopes = { "api.read", "api.write" },
                ApiSecrets = { new Secret("efcorewebapiclient-secret".Sha256()) },
                UserClaims = { "role" }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("demowebapi.fullaccess"),
            new ApiScope("api.read"),
            new ApiScope("api.write")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // Define a new client configuration for IdentityServer
            new Client()
            {
                // The name of the client application
                ClientName = "WebUIClient",

                // Unique identifier for the client
                ClientId = "webuiclient",

                // Specifies the authentication flow to be used
                // 'Code' grant type is used for OAuth 2.0 authorization code flow
                AllowedGrantTypes = GrantTypes.Code,

                // List of allowed redirect URIs after authentication
                RedirectUris =
                {
                    "https://localhost:7188/signin-oidc" // Redirect URI for OpenID Connect authentication in webUI project
                },

                PostLogoutRedirectUris =
                {
                    "https://localhost:7188/signout-callback-oidc" // Redirect URI after logout
                },

                // Defines the scopes that this client can request
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId, // Includes the 'sub' (subject) claim
                    IdentityServerConstants.StandardScopes.Profile, // Includes profile-related claims
                    "roles",
                    "demowebapi.fullaccess"
                },

                // Defines the secret(s) associated with the client for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256()) // Hashed secret used for client authentication
                },
                RequireConsent = true // consent to share needed info
            },


            new Client()
            {
                // The name of the client application
                ClientName = "EFCoreWebAPIClient",

                // Unique identifier for the client
                ClientId = "efcorewebapiclient",

                // Specifies the authentication flow to be used
                // 'Code' grant type is used for OAuth 2.0 authorization code flow
                AllowedGrantTypes = GrantTypes.Code,

                // List of allowed redirect URIs after authentication
                RedirectUris =
                {
                    "https://localhost:7285/swagger/oauth2-redirect.html" // Redirect URI for OpenID Connect authentication in webUI project
                },

                PostLogoutRedirectUris =
                {
                    "https://localhost:7285/swagger" // Redirect URI after logout
                },

                // Defines the scopes that this client can request
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId, // Includes the 'sub' (subject) claim
                    IdentityServerConstants.StandardScopes.Profile, // Includes profile-related claims
                    "roles",
                    "api.read",
                    "api.write"
                },

                AllowAccessTokensViaBrowser = true,

                // Defines the secret(s) associated with the client for authentication
                ClientSecrets =
                {
                    new Secret("efcorewebapiclient-secret".Sha256()) // Hashed secret used for client authentication
                },
                RequireConsent = true // consent to share needed info
            }
        };
}