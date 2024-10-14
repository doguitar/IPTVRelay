using IPTVRelay.Database.Enums;
using IPTVRelay.Database.Models;

namespace IPTVRelay.Blazor.ViewModels
{
    public class FilterVM : IFilter
    {
        public long Id { get; set; } = 0;
        public bool Invert { get; set; } = true;
        public FilterType FilterType { get; set; } = FilterType.Contains;
        public string? FilterContent { get; set; } = string.Empty;


        public FilterVM()
        {
                

        }
        public FilterVM(IFilter filter)
        {
            Id = filter.Id;
            Invert = filter.Invert;
			FilterType = filter.FilterType;
			FilterContent = filter.FilterContent ?? string.Empty;
		}
    }
}
