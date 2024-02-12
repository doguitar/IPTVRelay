using System.ComponentModel;

namespace IPTVRelay.Database.Enums
{
    public enum FilterType
    {
        [Description("")]
        None,
        [Description("Contains")]
        Contains,
        [Description("Regex")] 
        Regex,
        [Description("First")]
        First,
        [Description("Last")]
        Last,
        [Description("Index")]
        Index
    }
}
