using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace IPTVRelay.Blazor.Components.Shared
{
    public class IconButtonBase : ComponentBase
    {
        [Parameter] public bool Disabled { get; set; } = false;
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    }
}
