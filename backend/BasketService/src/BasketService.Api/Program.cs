using BasketService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBasketApiServices(builder.Configuration);

var app = builder.Build();
app.UseBasketApiPipeline();
app.MapBasketApiEndpoints();

app.Run();
