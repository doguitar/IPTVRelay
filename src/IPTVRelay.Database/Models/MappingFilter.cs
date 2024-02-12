namespace IPTVRelay.Database.Models
{
    public class MappingFilter : Filter
    {
        public long? MappingId { get; set; }

        public virtual Mapping? Mapping { get; set; }
    }
}
