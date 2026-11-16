namespace GiftOfTheGivers.Helpers
{
    public static class TaxCertificateHelper
    {
        public static string FormatCertificateNumber(DateTime? issueDate = null)
        {
            var date = issueDate ?? DateTime.UtcNow;
            return $"TAX-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }
    }
}
