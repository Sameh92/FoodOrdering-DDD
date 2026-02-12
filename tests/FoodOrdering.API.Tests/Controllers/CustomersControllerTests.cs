using FluentAssertions;
using FoodOrdering.API.Controllers;
using FoodOrdering.API.DTOs.Common;
using FoodOrdering.API.DTOs.CustomerDTOs;
using FoodOrdering.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace FoodOrdering.API.Tests.Controllers;

public class CustomersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"john.doe.{Guid.NewGuid()}@example.com", // Unique email
            PhoneNumber = "+60123456789",
            DefaultAddress = new AddressRequest
            {
                Street = "123 Main Street",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.FullName.Should().Be("John Doe");
        customer.MembershipLevel.Should().Be("Standard");
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate.{Guid.NewGuid()}@example.com";

        var request1 = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            PhoneNumber = "+60123456789"
        };

        var request2 = new CreateCustomerRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = email, // Same email
            PhoneNumber = "+60987654321"
        };

        // Act
        await _client.PostAsJsonAsync("/api/customers", request1);
        var response = await _client.PostAsJsonAsync("/api/customers", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email", // Invalid email format
            PhoneNumber = "+60123456789"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await TestDataHelper.SeedCustomerAsync(context);

        // Act
        var response = await _client.GetAsync($"/api/customers/{customer.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(customer.Id.Value);
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomerByEmail_WithValidEmail_ShouldReturnCustomer()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await TestDataHelper.SeedCustomerAsync(context);

        // Act
        var response = await _client.GetAsync($"/api/customers/by-email/{customer.Email.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        dto.Should().NotBeNull();
        dto!.Email.Should().Be(customer.Email.Value);
    }

    [Fact]
    public async Task UpdateContactInfo_WithValidData_ShouldReturnUpdatedCustomer()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await TestDataHelper.SeedCustomerAsync(context);

        var request = new UpdateContactRequest
        {
            Email = $"updated.{Guid.NewGuid()}@example.com",
            PhoneNumber = "+60999888777"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{customer.Id.Value}/contact", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        dto!.Email.Should().Be(request.Email.ToLower());
        dto.PhoneNumber.Should().Contain("999888777");
    }

    [Fact]
    public async Task UpdateName_WithValidData_ShouldReturnUpdatedCustomer()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await TestDataHelper.SeedCustomerAsync(context);

        var request = new UpdateNameRequest
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{customer.Id.Value}/name", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        dto!.FirstName.Should().Be("Updated");
        dto.LastName.Should().Be("Name");
        dto.FullName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task SetDefaultAddress_WithValidData_ShouldReturnUpdatedCustomer()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await TestDataHelper.SeedCustomerAsync(context);

        var request = new AddressRequest
        {
            Street = "999 New Street",
            City = "Petaling Jaya",
            State = "Selangor",
            PostalCode = "47800",
            Country = "Malaysia"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{customer.Id.Value}/address", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        dto!.DefaultAddress.Should().NotBeNull();
        dto.DefaultAddress!.Street.Should().Be("999 New Street");
        dto.DefaultAddress.City.Should().Be("Petaling Jaya");
    }
}