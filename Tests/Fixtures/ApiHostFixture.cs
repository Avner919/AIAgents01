using System.Net;

using Exchange.Api;
using Exchange.Api.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Exchange.Tests.Fixtures
{
    public sealed class ApiHostFixture : IAsyncDisposable
    {
        private readonly WebApplication _app;
        private ApiHostFixture(WebApplication app, HttpClient client)
        {
            _app = app;
            Client = client;
        }

        public const string Key = "synthetic-test-key";
        public HttpClient Client { get; }

        public static async Task<ApiHostFixture> StartAsync(UtilityDeploymentFixture deployment,
            Func<IReadOnlyDictionary<string, string>>? environment = null, IUtilityProcessLauncher? launcher = null)
        {
            var app = ApiApplication.Build([], builder =>
            {
                builder.Configuration.Sources.Clear();
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AdminApi:ApiKey"] = Key,
                    ["AdminApi:UtilityPath"] = deployment.UtilityPath
                });
                builder.Logging.ClearProviders();
                builder.WebHost.ConfigureKestrel(o => o.Listen(IPAddress.Loopback, 0));
                builder.Services.AddSingleton<Func<IReadOnlyDictionary<string, string>>>(environment ?? (() => deployment.EnvironmentValues()));
                if (launcher != null)
                {
                    builder.Services.AddSingleton(launcher);
                }
            });
            try
            {
                await app.StartAsync().WaitAsync(TimeSpan.FromSeconds(10));
                var url = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
                return new ApiHostFixture(app, new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(10) });
            }
            catch
            {
                await app.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _app.StopAsync(deadline.Token);
            await _app.DisposeAsync();
        }
    }

    public sealed class CountingLauncher : IUtilityProcessLauncher
    {
        private int _calls;
        public int Calls => _calls;
        public Exception? Failure { get; init; }
        public UtilityLaunchSnapshot? LastSnapshot { get; private set; }
        public int Start(UtilityLaunchSnapshot snapshot)
        {
            Interlocked.Increment(ref _calls);
            LastSnapshot = snapshot;
            if (Failure != null)
            {
                throw Failure;
            }
            return 42;
        }
    }
}