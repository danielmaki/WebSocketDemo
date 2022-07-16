using Microsoft.Extensions.Hosting;
using WebSocketDemo.Hosting;

var host = Host.CreateDefaultBuilder(args);

host.UseLifetime();

host.UseAppServices();

var app = host.Build();

await app.RunAsync();
