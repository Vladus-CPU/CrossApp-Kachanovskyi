using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.Tests;

public class OrderEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task OrderCrud_Works()
    {
        using var client = factory.CreateClient();

        var request = new
        {
            customerId = "C-TEST-001",
            lines = new[]
            {
                new { productId = "P-001", name = "Keyboard", price = 1200m, quantity = 2 }
            }
        };

        var created = await client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var location = created.Headers.Location;

        Assert.NotNull(location);

        var received = await client.GetAsync(location!);

        Assert.Equal(HttpStatusCode.OK, received.StatusCode);

        var updated = await client.PutAsJsonAsync(location!, new
        {
            customerId = "C-TEST-001",
            lines = new[]
            {
                new { productId = "P-001", name = "Keyboard", price = 1200m, quantity = 3 }
            }
        });

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var order = await updated.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(3600m, order.GetProperty("total").GetDecimal());

        var deleted = await client.DeleteAsync(location!);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var notFound = await client.GetAsync(location!);

        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidOrder_Returns400()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new { customerId = "", lines = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = result.GetProperty("errors");

        Assert.True(errors.TryGetProperty("CustomerId", out _));
        Assert.True(errors.TryGetProperty("Lines", out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("""{"customerId":"C-001","lines":[{"productId":"P-001","name":"Keyboard","price":1200,"quantity":"abc"}]}""")]
    public async Task Create_InvalidBody_Returns400(string json)
    {
        using var client = factory.CreateClient();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/orders", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_TooManyOrders_Returns409()
    {
        using var client = factory.CreateClient();

        var request = new
        {
            customerId = Guid.NewGuid().ToString("N"),
            lines = new[]
            {
                new { productId = "P-001", name = "Mouse", price = 500m, quantity = 1 }
            }
        };

        for (int i = 0; i < 5; i++)
        {
            var created = await client.PostAsJsonAsync("/api/orders", request);

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        }

        var response = await client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_EmptyName_Returns400()
    {
        using var client = factory.CreateClient();

        var request = new
        {
            customerId = Guid.NewGuid().ToString("N"),
            lines = new[]
            {
                new { productId = "P-001", name = "Mouse", price = 500m, quantity = 1 }
            }
        };

        var created = await client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var updated = await client.PutAsJsonAsync(created.Headers.Location!, new
        {
            customerId = request.customerId,
            lines = new[]
            {
                new { productId = "P-001", name = "", price = 500m, quantity = 2 }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, updated.StatusCode);
    }

    [Fact]
    public async Task GetAll_InvalidPage_Returns400()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/orders?page=0&pageSize=200");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}