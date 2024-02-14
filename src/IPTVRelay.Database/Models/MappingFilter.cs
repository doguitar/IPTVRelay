namespace IPTVRelay.Database.Models
{
    public class MappingFilter : Filter
    {
        public long? MappingId { get; set; }

        public virtual Mapping? Mapping { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj == null || !(obj is MappingFilter)) return false;
            var f = (MappingFilter)obj;
            return f.FilterType == FilterType && f.FilterContent == FilterContent && f.Invert == Invert;
        }
    }
}
