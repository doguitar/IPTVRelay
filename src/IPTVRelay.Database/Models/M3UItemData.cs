namespace IPTVRelay.Database.Models
{
    public class M3UItemData : ModelBase
    {
        public string? Key { get; set; }
        public string? Value { get; set; }

        public long M3UItemId { get; set; }
    }
}
