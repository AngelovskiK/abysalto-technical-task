using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
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

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblem>();

        problem.Should().NotBeNull();
        problem!.Title.Should().Be("VALIDATION_ERROR");
        problem.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problem.Detail.Should().Contain("Email: Email is required");
        problem.Detail.Should().Contain("Name: Name is required");
    }

    private sealed class HttpValidationProblem
    {
        public string? Title { get; init; }
        public int? Status { get; init; }
        public string? Detail { get; init; }
    }
}