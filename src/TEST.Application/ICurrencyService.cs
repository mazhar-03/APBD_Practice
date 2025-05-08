using TEST.Models;

namespace TEST.Application;

public interface ICurrencyService
{
    public IEnumerable<Currency> SearchByCountry(string countryName);
    public IEnumerable<string> SearchByCurrency(string currencyName);
    bool CountryExists(string countryName);
    bool CurrencyExists(string currencyName);
    bool CreateCurrency(CurrencyRequestDto request);
    bool UpdateCurrency(CurrencyRequestDto request);
}