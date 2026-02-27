using AlertService.Consumers;
using AlertService.Services;
using MassTransit;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);

// --- CONFIGURAÇÃO DO PROMETHEUS PARA WORKER ---
// Como não temos app.MapMetrics(), iniciamos um servidor de métricas dedicado
// Vamos usar a porta 8080, que é o padrão que configuramos no Kubernetes
var metricServer = new KestrelMetricServer(port: 8080); 
metricServer.Start();

builder.Services.AddHttpClient<IPropertyStatusClient, PropertyStatusClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PropertyServiceUrl"] ?? "http://property-service");
    var apiKey = builder.Configuration["InternalApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SensorDataConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("sensor-data-queue", e =>
        {
            e.ConfigureConsumer<SensorDataConsumer>(ctx);
        });
    });
});

builder.Services.AddSingleton<IAlertService, AlertServiceImpl>();

var host = builder.Build();
host.Run();