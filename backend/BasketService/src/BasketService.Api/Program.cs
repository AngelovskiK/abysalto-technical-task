using BasketService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureConfiguration(builder.Configuration);
builder.Services.ConfigureOpenApi();
builder.Services.ConfigureObservability();
builder.Services.ConfigureData(builder.Configuration);
builder.Services.ConfigureApplication();
builder.Services.ConfigureAuthAndCors();
builder.Services.ConfigureRateLimiting();
builder.Services.AddControllers();

var app = builder.Build();

app.ConfigureDeveloperTools();
app.ConfigureMiddleware();
app.MapHealthEndpoints();
app.MapControllers();

app.Run();
