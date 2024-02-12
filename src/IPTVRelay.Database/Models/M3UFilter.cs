namespace IPTVRelay.Database.Models
{
    public class M3UFilter : Filter
    {
        public long? M3UId { get; set; }

        public virtual M3U? M3U { get; set; }
    }
}
