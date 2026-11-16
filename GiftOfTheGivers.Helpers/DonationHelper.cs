namespace GiftOfTheGivers.Helpers
{
    public class CurrencyTotalResult
    {
        public string Currency { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Count { get; set; }
    }

    public static class DonationHelper
    {
        public static List<CurrencyTotalResult> CalculateTotalsByCurrency(
            IEnumerable<(string Currency, decimal Amount)> donations)
        {
            return donations
                .GroupBy(d => d.Currency)
                .Select(g => new CurrencyTotalResult
                {
                    Currency = g.Key,
                    Total = g.Sum(d => d.Amount),
                    Count = g.Count()
                })
                .OrderBy(t => t.Currency)
                .ToList();
        }
    }
}