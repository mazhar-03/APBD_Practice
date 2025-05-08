using Microsoft.AspNetCore.Mvc;
using TEST.Application;

namespace TEST.API;

[Route("api/")]
[ApiController]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }
    
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string searchTerm, [FromQuery] string type)
    {
        if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(type))
            return BadRequest("Both searchTerm and type are required.");

        if (type.ToLower() == "country")
        {
            var result = _currencyService.SearchByCountry(searchTerm);
            var countryExists = _currencyService.CountryExists(searchTerm);
            if(!countryExists)
                return BadRequest($"Country '{searchTerm}' does not exist.");
            
            return Ok(new
            {
                CountryName = searchTerm,
                Currencies = result.Select(c => new { c.Name, c.Rate }).ToList()
            });
        }

        else if (type.ToLower() == "currency")
        {
            var result = _currencyService.SearchByCurrency(searchTerm);
            var currencyExists = _currencyService.CurrencyExists(searchTerm);
            if(!currencyExists)
                return BadRequest($"Currency '{searchTerm}' does not exist.");
            
            return Ok(new
            {
                CurrencyName = searchTerm,
                Countries = result
            });
        }
        else
        {
            return BadRequest("Invalid search type. Use 'country' or 'currency'.");
        }
    }
}