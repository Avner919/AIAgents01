using System.ComponentModel;

using Exchange.Api.Contracts;
using Exchange.Api.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Exchange.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("admin/exchanges")]
    public sealed class ExchangeController(UtilityLaunchPreparation preparation, IUtilityProcessLauncher launcher,
        ILogger<ExchangeController> logger) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(LaunchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public ActionResult<LaunchResponse> Launch([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LaunchRequest? request)
        {
            if (!string.IsNullOrWhiteSpace(request?.ExchangeProcessFileName))
            {
                return Failure(400, "unsupported_client_override");
            }
            UtilityLaunchSnapshot snapshot;
            try
            {
                snapshot = preparation.Prepare();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
            {
                logger.LogWarning("Utility configuration is unavailable.");
                return Failure(503, "utility_configuration_unavailable");
            }
            try
            {
                var pid = launcher.Start(snapshot);
                var s = snapshot.Settings;
                return Ok(new LaunchResponse(pid, new LaunchDefaults(s.ClientFileName, s.ClientExecutablePath,
                    s.ClientWorkingDirectory, s.InputDirectory, s.OutputDirectory, s.TimeoutMilliseconds)));
            }
            catch (Exception ex) when (ex is Win32Exception or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
            {
                logger.LogWarning("Utility process could not be started.");
                return Failure(500, "utility_start_failed");
            }
        }

        private ObjectResult Failure(int status, string code)
        {
            return StatusCode(status, new ProblemDetails { Status = status, Title = "Launch request failed", Extensions = { ["code"] = code } });
        }
    }
}