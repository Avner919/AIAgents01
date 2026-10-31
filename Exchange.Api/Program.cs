using Exchange.Api;

await using var app = ApiApplication.Build(args);
await app.RunAsync();