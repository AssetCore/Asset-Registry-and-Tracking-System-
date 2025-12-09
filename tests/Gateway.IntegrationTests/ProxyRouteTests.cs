using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Gateway.IntegrationTests;

public class ProxyRouteTests
{
    [Fact]
    public async Task AssetRegistry_Route_Forwards_To_Backend()
    {
        var port = GetFreeTcpPort();

        using var cts = new CancellationTokenSource();
        var backendTask = RunBackendAsync(port, cts.Token);

        try
        {
            var factory = new WebApplicationFactory<global::Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    var overrides = new Dictionary<string, string?>
                    {
                        ["ReverseProxy:Clusters:asset-registry:Destinations:d1:Address"] = $"http://localhost:{port}/"
                    };
                    config.AddInMemoryCollection(overrides);
                });
            });

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var resp = await client.GetAsync("/assetregistry/ping");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var body = await resp.Content.ReadAsStringAsync();
            Assert.Equal("pong", body);
        }
        finally
        {
            cts.Cancel();
            await backendTask; // ensure backend is shut down
        }
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task RunBackendAsync(int port, CancellationToken token)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel().UseUrls($"http://localhost:{port}");
        var app = builder.Build();
        app.MapGet("/ping", () => Results.Text("pong", "text/plain"));
        await app.RunAsync(token);
    }
}
