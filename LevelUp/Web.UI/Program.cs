using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure =>
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

//add management middleware
builder.Services.AddOpenIdConnectAccessTokenManagement();
// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiRoot"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
//add token handler to client
}).AddUserAccessTokenHandler()
  .AddHttpMessageHandler(() => new LoggingHandler());

//add to configura authentication middleware
builder.Services.AddAuthentication(options =>
    {
        // Specifies the default authentication scheme used for authentication
        // Cookie-based authentication scheme is set as the default
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // Specifies the default challenge scheme used when authentication is required
        // OpenID Connect is used for authentication challenges
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })

    // Adds cookie authentication for managing user sessions
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.AccessDeniedPath = "/Authentication/AccessDenied";
    })

    // Adds OpenID Connect authentication for authenticating users
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // Specifies the sign-in scheme to use cookie authentication for maintaining sessions
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // Sets the authority (Identity Provider, IDP) URL for authentication
        options.Authority = "https://localhost:5001"; //our IDP 
        options.ClientId = "webuiclient"; //should match client id in IDP
        options.ClientSecret = "secret"; //should match client secret in IDP
        options.ResponseType = "code"; //code flow, PKCE auto enabled, later about that

        //options.Scope.Add("openid"); //<<<requested by middleware by default
        //options.Scope.Add("profile"); //<<<requested by middleware by default
        options.Scope.Add("roles");
        options.Scope.Add("demowebapi.fullaccess");
        //options.CallbackPath = new PathString("signin-oidc"); //redirect uri in IDP, also default
        
        //options.SignedOutCallbackPath : default = host/port/signout-callback-oidc - register in IDP
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true; //save tokens in cookie
        
        //remove filter for default claim, should load in our claims, not very intuitive ¯\_(ツ)_/¯
        options.ClaimActions.Remove("aud");
        //remove excessive Claims themselves
        options.ClaimActions.DeleteClaim("sid");
        options.ClaimActions.DeleteClaim("idp");
        
        //If only one key claim is present then it's fine
        //options.ClaimActions.MapUniqueJsonKey("role", "role");
        
        //VVV If user has multiple roles, then it could come up multiple times so this one is better VVV
        options.ClaimActions.MapJsonKey("role", "role");

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UI}/{action=Index}/{id?}");

app.Run();

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Authorization Header: " + request.Headers.Authorization);
        return await base.SendAsync(request, cancellationToken);
    }
}