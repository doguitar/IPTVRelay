namespace IPTVRelay.Blazor.ViewModels
{
    public class MappingImportVM
    {
        public string ChannelName { get; set; } = string.Empty;
        public int ChannelNumber { get; set; } = 1;
        public string CategoryName { get; set; } = string.Empty;
        public string GuideName { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
        public string PlaylistName { get; set; } = string.Empty;
        public FilterVM[] Filters { get; set; } = [];

    }
}
