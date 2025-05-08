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
            var query = "SELECT COUNT(1) FROM Country WHERE Name = @CountryName";
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
            var query = "SELECT COUNT(1) FROM Currency WHERE Name = @CurrencyName";

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
                while (reader.Read())
                    countries.Add(reader["CountryName"].ToString());
            }
        }

        return countries;
    }

    public bool UpdateCurrency(CurrencyRequestDto request)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // Check if all countries exist in the database
            foreach (var country in request.Countries)
                if (!CountryExists(country))
                    return false; // If any country doesn't exist, return false

            var updateQuery = @"UPDATE Currency SET Rate = @CurrencyRate WHERE Name = @CurrencyName";
            var updateCommand = new SqlCommand(updateQuery, connection);
            updateCommand.Parameters.AddWithValue("@CurrencyName", request.CurrencyName);
            updateCommand.Parameters.AddWithValue("@CurrencyRate", request.CurrencyRate);

            connection.Open();
            updateCommand.ExecuteNonQuery();

            // Update Currency-Country associations
            // First, remove old associations
            var deleteQuery =
                "DELETE FROM CurrencyCountry WHERE CurrencyId = (SELECT Id FROM Currency WHERE Name = @CurrencyName)";
            var deleteCommand = new SqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@CurrencyName", request.CurrencyName);
            deleteCommand.ExecuteNonQuery();


            foreach (var country in request.Countries)
            {
                var insertQuery = @"
                    INSERT INTO CurrencyCountry (CurrencyId, CountryId)
                    SELECT 
                        (SELECT Id FROM Currency WHERE Name = @CurrencyName),
                        (SELECT Id FROM Country WHERE Name = @CountryName)";
                var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@CurrencyName", request.CurrencyName);
                insertCommand.Parameters.AddWithValue("@CountryName", country);
                insertCommand.ExecuteNonQuery();
            }

            return true;
        }
    }

    public bool CreateCurrency(CurrencyRequestDto request)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            foreach (var country in request.Countries)
            {
                if(!CountryExists(country))
                    return false;
            }
            
            var insertQuery = @"
                INSERT INTO Currency (Name, Rate)
                VALUES (@CurrencyName, @CurrencyRate)";
            
            var insertCommand = new SqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@CurrencyName", request.CurrencyName);
            insertCommand.Parameters.AddWithValue("@CurrencyRate", request.CurrencyRate);
            connection.Open();
            insertCommand.ExecuteNonQuery();
            
            // Get the newly created CurrencyId
            var getCurrencyIdQuery = "SELECT Id FROM Currency WHERE Name = @CurrencyName";
            var getCurrencyIdCommand = new SqlCommand(getCurrencyIdQuery, connection);
            getCurrencyIdCommand.Parameters.AddWithValue("@CurrencyName", request.CurrencyName);
            var currencyId = (int)getCurrencyIdCommand.ExecuteScalar();

            foreach (var country in request.Countries)
            {
                var insertCountryQuery = @"INSERT INTO Currency_Country (Currency_Id, Country_Id)
                                           VALUES (@CurrencyId, SELECT Id FROM Country WHERE Name = @CountryName)";
                var insertCountryCommand = new SqlCommand(insertCountryQuery, connection);
                insertCountryCommand.Parameters.AddWithValue("@CurrencyId", currencyId);
                insertCountryCommand.Parameters.AddWithValue("@CountryName", country);
            }
            return true;
        }
    }
}