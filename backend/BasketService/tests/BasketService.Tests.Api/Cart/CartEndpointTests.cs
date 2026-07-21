using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BasketService.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using EntityCart = BasketService.Domain.Entities.Cart;
using EntityUser = BasketService.Domain.Entities.User;

namespace BasketService.Tests.Api.Cart;

public class CartEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string SecretKey = "dev-secret-key-change-me-in-production";
    private readonly WebApplicationFactory<Program> _factory;
    private readonly InMemoryStore _store = new();

    public CartEndpointTests(WebApplicationFactory<Program> factory)
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

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_store);
                services.AddSingleton<IUserRepository, InMemoryUserRepository>();
                services.AddSingleton<ICartRepository, InMemoryCartRepository>();
            });
        });
    }

    [Fact]
    public async Task Get_WhenUnauthenticated_ReturnsUnauthorizedProblemDetails()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<HttpProblem>();

        Assert.NotNull(problem);
        Assert.Equal("UNAUTHORIZED", problem!.Title);
        Assert.Equal((int)HttpStatusCode.Unauthorized, problem.Status);
    }

    [Fact]
    public async Task CartEndpoints_WhenAuthenticated_ExecuteCrudFlowSuccessfully()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        _store.Users.Clear();
        _store.Carts.Clear();

        var user = EntityUser.Create($"cart-flow-{Guid.NewGuid():N}@example.com", "Cart Flow User");
        var cart = EntityCart.Create(user.Id);
        _store.Users.Add(user);
        _store.Carts.Add(cart);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwtToken(user.Id, user.Email));

        var addResponse = await client.PostAsJsonAsync("/api/cart/items", new
        {
            productId = Guid.NewGuid(),
            productName = "Desk Lamp",
            unitPrice = 35.75m,
            quantity = 2,
            imageUrl = "https://example.test/lamp.png"
        });

        addResponse.EnsureSuccessStatusCode();
        var addedCart = await addResponse.Content.ReadFromJsonAsync<HttpCartResponse>();
        Assert.NotNull(addedCart);
        Assert.Single(addedCart!.Items);
        Assert.Equal(71.50m, addedCart.Total);

        var cartItemId = addedCart.Items[0].Id;

        var updateResponse = await client.PutAsJsonAsync($"/api/cart/items/{cartItemId}", new
        {
            quantity = 3
        });

        updateResponse.EnsureSuccessStatusCode();
        var updatedCart = await updateResponse.Content.ReadFromJsonAsync<HttpCartResponse>();
        Assert.NotNull(updatedCart);
        Assert.Equal(3, updatedCart!.Items[0].Quantity);
        Assert.Equal(107.25m, updatedCart.Total);

        var getResponse = await client.GetAsync("/api/cart");
        getResponse.EnsureSuccessStatusCode();
        var currentCart = await getResponse.Content.ReadFromJsonAsync<HttpCartResponse>();
        Assert.NotNull(currentCart);
        Assert.Single(currentCart!.Items);
        Assert.Equal(cartItemId, currentCart.Items[0].Id);

        var removeResponse = await client.DeleteAsync($"/api/cart/items/{cartItemId}");
        removeResponse.EnsureSuccessStatusCode();
        var removedCart = await removeResponse.Content.ReadFromJsonAsync<HttpCartResponse>();
        Assert.NotNull(removedCart);
        Assert.Empty(removedCart!.Items);
        Assert.Equal(0m, removedCart.Total);

        var clearResponse = await client.DeleteAsync("/api/cart");
        Assert.Equal(HttpStatusCode.NoContent, clearResponse.StatusCode);
    }

    private static string GenerateJwtToken(Guid userId, string email)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "basket-api",
            audience: "basket-api-client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class HttpProblem
    {
        public string? Title { get; init; }
        public int? Status { get; init; }
        public string? Detail { get; init; }
    }

    private sealed class HttpCartResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public decimal Total { get; init; }
        public DateTime UpdatedAt { get; init; }
        public List<HttpCartItemResponse> Items { get; init; } = [];
    }

    private sealed class HttpCartItemResponse
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public string? ImageUrl { get; init; }
    }

    private sealed class InMemoryStore
    {
        public List<EntityUser> Users { get; } = [];
        public List<EntityCart> Carts { get; } = [];
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly InMemoryStore _store;

        public InMemoryUserRepository(InMemoryStore store)
        {
            _store = store;
        }

        public Task<EntityUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Users.FirstOrDefault(user => user.Email == email));
        }

        public Task<EntityUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Users.FirstOrDefault(user => user.Id == userId));
        }

        public Task AddAsync(EntityUser user, CancellationToken cancellationToken = default)
        {
            _store.Users.Add(user);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryCartRepository : ICartRepository
    {
        private readonly InMemoryStore _store;

        public InMemoryCartRepository(InMemoryStore store)
        {
            _store = store;
        }

        public Task<EntityCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Carts.FirstOrDefault(cart => cart.UserId == userId));
        }

        public Task<EntityCart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Carts.FirstOrDefault(cart => cart.Id == cartId));
        }

        public Task AddAsync(EntityCart cart, CancellationToken cancellationToken = default)
        {
            _store.Carts.Add(cart);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(EntityCart cart, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}