using System.ComponentModel;

namespace IPTVRelay.Blazor.Components.Shared
{
    public enum ButtonColor
    {
        [Description("btn-primary")]
        Primary,
        [Description("btn-secondary")]
        Secondary,
        [Description("btn-success")]
        Success,
        [Description("btn-danger")]
        Danger,
        [Description("btn-warning")]
        Warning,
        [Description("btn-info")]
        Info,
        [Description("btn-light")]
        Light,
        [Description("btn-dark")]
        Dark,
    }
    public enum ButtonOutlineColor
    {
        [Description("btn-outline-primary")]
        Primary,
        [Description("btn-outline-secondary")]
        Secondary,
        [Description("btn-outline-success")]
        Success,
        [Description("btn-outline-danger")]
        Danger,
        [Description("btn-outline-warning")]
        Warning,
        [Description("btn-outline-info")]
        Info,
        [Description("btn-outline-light")]
        Light,
        [Description("btn-outline-dark")]
        Dark,
    }
}
