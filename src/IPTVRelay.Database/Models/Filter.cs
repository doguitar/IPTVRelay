using System.ComponentModel.DataAnnotations.Schema;
using IPTVRelay.Database.Enums;

namespace IPTVRelay.Database.Models
{
	public class Filter : ModelBase, IFilter
	{
		[Column(TypeName = "TEXT")]
		public FilterType FilterType { get; set; } = FilterType.None;
		public string? FilterContent { get; set; }
		public bool Invert { get; set; }
	}
	public interface IFilter 
	{
		public long Id { get; set; }
		public FilterType FilterType { get; set; }
		public string? FilterContent { get; set; }
		public bool Invert { get; set; }
	}
}
