using Microsoft.AspNetCore.Mvc;
using TEST.Application;
using TEST.Models;

namespace TEST.API;

[Route("api/")]
public class CurrencyController
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }
    
    [HttpPost("currency")]
    public IResult CreateOrUpdateCurrency([FromBody] CurrencyRequestDto request)
    {
        if (string.IsNullOrEmpty(request.CurrencyName) || request.CurrencyRate <= 0 || request.Countries == null || !request.Countries.Any())
        {
            return Results.BadRequest("Invalid request data.");
        }

        var currencyExists = _currencyService.CurrencyExists(request.CurrencyName);
        if (currencyExists)
        {
            var updateResult = _currencyService.UpdateCurrency(request);
            return Results.Ok(new { message = "Currency updated successfully." });
        }
    
        var createResult = _currencyService.CreateCurrency(request);
    
        return Results.Ok(new { message = "Currency created successfully." });
    }

    [HttpGet("search")]
    public IResult Search([FromQuery] string searchTerm, [FromQuery] string type)
    {
        if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(type))
            return Results.BadRequest("Both searchTerm and type are required.");

        if (type.ToLower() == "country")
        {
            var result = _currencyService.SearchByCountry(searchTerm);
            var countryExists = _currencyService.CountryExists(searchTerm);
            if (!countryExists)
                return Results.BadRequest($"Country '{searchTerm}' does not exist.");

            return Results.Ok(new
            {
                CountryName = searchTerm,
                Currencies = result.Select(c => new { c.Name, c.Rate }).ToList()
            });
        }

        if (type.ToLower() == "currency")
        {
            var result = _currencyService.SearchByCurrency(searchTerm);
            var currencyExists = _currencyService.CurrencyExists(searchTerm);
            if (!currencyExists)
                return Results.BadRequest($"Currency '{searchTerm}' does not exist.");

            return Results.Ok(new
            {
                CurrencyName = searchTerm,
                Countries = result
            });
        }

        return Results.BadRequest("Invalid search type. Use 'country' or 'currency'.");
    }
}