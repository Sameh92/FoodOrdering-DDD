using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.Repositories;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using FoodOrdering.Application.Common;
using Microsoft.AspNetCore.Mvc;
using FoodOrdering.API.DTOs.RestaurantDTOs;

namespace FoodOrdering.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RestaurantsController(IRestaurantRepository restaurantRepository, IUnitOfWork unitOfWork)
    {
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all active restaurants.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RestaurantSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRestaurants()
    {
        var restaurants = await _restaurantRepository.GetActiveRestaurantsAsync();
        var dtos = restaurants.Select(RestaurantSummaryDto.FromRestaurant).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get a restaurant by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRestaurant(Guid id)
    {
        var restaurantId = RestaurantId.From(id);
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

        if (restaurant == null)
            return NotFound(new { error = "Restaurant not found" });

        return Ok(RestaurantDto.FromRestaurant(restaurant));
    }

    /// <summary>
    /// Search restaurants by name.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<RestaurantSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchRestaurants([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Ok(new List<RestaurantSummaryDto>());

        var restaurants = await _restaurantRepository.SearchByNameAsync(name);
        var dtos = restaurants.Select(RestaurantSummaryDto.FromRestaurant).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get restaurants by city.
    /// </summary>
    [HttpGet("by-city/{city}")]
    [ProducesResponseType(typeof(List<RestaurantSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRestaurantsByCity(string city)
    {
        var restaurants = await _restaurantRepository.GetByCityAsync(city);
        var dtos = restaurants.Select(RestaurantSummaryDto.FromRestaurant).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Create a new restaurant.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantRequest request)
    {
        try
        {
            var address = new Address(
                request.Address.Street,
                request.Address.City,
                request.Address.State,
                request.Address.PostalCode,
                request.Address.Country
            );

            var restaurant = Restaurant.Create(
                request.Name,
                request.Description,
                address,
                maxConcurrentOrders: request.MaxConcurrentOrders
            );

            await _restaurantRepository.AddAsync(restaurant);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRestaurant),
                new { id = restaurant.Id.Value },
                RestaurantDto.FromRestaurant(restaurant)
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update restaurant details.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRestaurant(Guid id, [FromBody] UpdateRestaurantRequest request)
    {
        try
        {
            var restaurantId = RestaurantId.From(id);
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            if (restaurant == null)
                return NotFound(new { error = "Restaurant not found" });

            restaurant.UpdateDetails(request.Name, request.Description);

            _restaurantRepository.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();

            return Ok(RestaurantDto.FromRestaurant(restaurant));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Activate a restaurant.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateRestaurant(Guid id)
    {
        var restaurantId = RestaurantId.From(id);
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

        if (restaurant == null)
            return NotFound(new { error = "Restaurant not found" });

        restaurant.Activate();

        _restaurantRepository.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return Ok(RestaurantDto.FromRestaurant(restaurant));
    }

    /// <summary>
    /// Deactivate a restaurant.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateRestaurant(Guid id)
    {
        var restaurantId = RestaurantId.From(id);
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

        if (restaurant == null)
            return NotFound(new { error = "Restaurant not found" });

        restaurant.Deactivate();

        _restaurantRepository.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return Ok(RestaurantDto.FromRestaurant(restaurant));
    }

    /// <summary>
    /// Add a rating to a restaurant.
    /// </summary>
    [HttpPost("{id:guid}/rate")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RateRestaurant(Guid id, [FromBody] RateRestaurantRequest request)
    {
        try
        {
            var restaurantId = RestaurantId.From(id);
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            if (restaurant == null)
                return NotFound(new { error = "Restaurant not found" });

            restaurant.AddRating(request.Stars);

            _restaurantRepository.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();

            return Ok(RestaurantDto.FromRestaurant(restaurant));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #region Menu Item Endpoints

    /// <summary>
    /// Get menu items for a restaurant.
    /// </summary>
    [HttpGet("{id:guid}/menu")]
    [ProducesResponseType(typeof(List<MenuItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMenuItems(Guid id)
    {
        var restaurantId = RestaurantId.From(id);
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

        if (restaurant == null)
            return NotFound(new { error = "Restaurant not found" });

        var menuItems = restaurant.MenuItems
            .Select(MenuItemDto.FromMenuItem)
            .ToList();

        return Ok(menuItems);
    }

    /// <summary>
    /// Add a menu item to a restaurant.
    /// </summary>
    [HttpPost("{id:guid}/menu")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMenuItem(Guid id, [FromBody] AddMenuItemRequest request)
    {
        try
        {
            var restaurantId = RestaurantId.From(id);
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            if (restaurant == null)
                return NotFound(new { error = "Restaurant not found" });

            var price = new Money(request.Price, request.Currency ?? "MYR");

            var menuItem = restaurant.AddMenuItem(
                request.Name,
                request.Description,
                price,
                request.Category,
                request.PreparationTimeMinutes
            );

            _restaurantRepository.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMenuItems),
                new { id = restaurant.Id.Value },
                MenuItemDto.FromMenuItem(menuItem)
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a menu item.
    /// </summary>
    [HttpPut("{id:guid}/menu/{menuItemId:guid}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMenuItem(Guid id, Guid menuItemId, [FromBody] UpdateMenuItemRequest request)
    {
        try
        {
            var restaurantId = RestaurantId.From(id);
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            if (restaurant == null)
                return NotFound(new { error = "Restaurant not found" });

            var menuItemIdVO = MenuItemId.From(menuItemId);
            var price = new Money(request.Price, request.Currency ?? "MYR");

            restaurant.UpdateMenuItem(menuItemIdVO, request.Name, request.Description, price, request.Category);

            _restaurantRepository.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();

            var updatedItem = restaurant.GetMenuItem(menuItemIdVO);
            return Ok(MenuItemDto.FromMenuItem(updatedItem!));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Set menu item availability.
    /// </summary>
    [HttpPut("{id:guid}/menu/{menuItemId:guid}/availability")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetMenuItemAvailability(Guid id, Guid menuItemId, [FromBody] SetAvailabilityRequest request)
    {
        try
        {
            var restaurantId = RestaurantId.From(id);
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            if (restaurant == null)
                return NotFound(new { error = "Restaurant not found" });

            var menuItemIdVO = MenuItemId.From(menuItemId);
            restaurant.SetMenuItemAvailability(menuItemIdVO, request.IsAvailable);

            _restaurantRepository.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();

            var updatedItem = restaurant.GetMenuItem(menuItemIdVO);
            return Ok(MenuItemDto.FromMenuItem(updatedItem!));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a menu item.
    /// </summary>
    [HttpDelete("{id:guid}/menu/{menuItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMenuItem(Guid id, Guid menuItemId)
    {
        try
        {
            var restaurantId = RestaurantId.From(id);
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            if (restaurant == null)
                return NotFound(new { error = "Restaurant not found" });

            var menuItemIdVO = MenuItemId.From(menuItemId);
            restaurant.RemoveMenuItem(menuItemIdVO);

            _restaurantRepository.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    #endregion
}
