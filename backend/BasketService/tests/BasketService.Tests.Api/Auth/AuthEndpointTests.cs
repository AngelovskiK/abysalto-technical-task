using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BasketService.Tests.Api.Auth;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:BasketDb"] = "Host=localhost;Port=5432;Database=basketdb;Username=retail;Password=retail_dev_pw"
                });
            });
        });
    }

    [Fact]
    public async Task Login_WhenRequestIsInvalid_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = string.Empty,
            name = string.Empty
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblem>();

        Assert.NotNull(problem);
        Assert.Equal("VALIDATION_ERROR", problem!.Title);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Contains("Email: Email is required", problem.Detail);
        Assert.Contains("Name: Name is required", problem.Detail);
    }

    private sealed class HttpValidationProblem
    {
        public string? Title { get; init; }
        public int? Status { get; init; }
        public string? Detail { get; init; }
    }
}