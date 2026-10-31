using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

using Exchange.Api.Options;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Exchange.Api.Authentication
{
    public sealed class ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ApiOptions apiOptions) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "ApiKey";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var valid = Request.Headers.TryGetValue("X-Api-Key", out var values) && values.Count == 1
                && !string.IsNullOrWhiteSpace(values[0])
                && CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(values[0]!)),
                    SHA256.HashData(Encoding.UTF8.GetBytes(apiOptions.ApiKey)));
            if (!valid)
            {
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
            }
            var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "operator")], SchemeName));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Extensions = { ["code"] = "unauthorized" }
            });
        }
    }
}