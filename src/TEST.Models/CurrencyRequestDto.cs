namespace TEST.Models;

public class CurrencyRequestDto
{
    public string CurrencyName { get; set; }
    public double CurrencyRate { get; set; }
    public List<string> Countries { get; set; }
}