using Microsoft.Data.SqlClient;
using TEST.Models;

namespace TEST.Application;

public class CurrencyService : ICurrencyService
{
    private readonly string _connectionString;

    public CurrencyService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool CountryExists(string countryName)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT COUNT(1) FROM Country WHERE Name = @CountryName";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CountryName", countryName);
            
            connection.Open();
            var result = (int)command.ExecuteScalar();
            return result > 0;
        }
    }
    
    public bool CurrencyExists(string currencyName)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(1) FROM Currency WHERE Name = @CurrencyName";

            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@CurrencyName", currencyName);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }

    public IEnumerable<Currency> SearchByCountry(string countryName)
    {
        var currencies = new List<Currency>();
        var sql = @"SELECT c.Name AS CurrencyName, c.Rate AS CurrencyRate FROM Currency c
                    JOIN Currency_Country cc ON c.Id = cc.Currency_Id
                    JOIN Country co ON cc.Country_Id = co.Id
                    WHERE co.Name = @CountryName";

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CountryName", countryName);
            var reader = cmd.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                        currencies.Add(new Currency
                        {
                            Name = reader["CurrencyName"].ToString(),
                            Rate = (float)Convert.ToDouble(reader["CurrencyRate"])
                        });

            }
            finally
            {
                reader.Close();
            }
        }

        return currencies;
    }

    public IEnumerable<string> SearchByCurrency(string currencyName)
    {
        var countries = new List<string>();

        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @"
                    SELECT co.Name AS CountryName
                    FROM Country co
                    JOIN Currency_Country cc ON co.Id = cc.Country_Id
                    JOIN Currency c ON cc.Currency_Id = c.Id
                    WHERE c.Name = @CurrencyName";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CurrencyName", currencyName);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) countries.Add(reader["CountryName"].ToString());
            }
        }

        return countries;
    }
}