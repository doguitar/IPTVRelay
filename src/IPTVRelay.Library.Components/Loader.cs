using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library.Components
{
    public class Loader : ComponentBase
    {
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public string LoadingMessage { get; set; } = "Loading...";
        [Parameter] public RenderFragment ChildContent { get; set; } = new RenderFragment(builder => { });

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (IsLoading)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "loading-spinner");
                builder.OpenElement(2, "div");
                builder.AddAttribute(1, "class", "loading-spinner-message");
                builder.AddContent(3, LoadingMessage);
                builder.CloseElement();
                builder.CloseElement();
            }
                base.BuildRenderTree(builder);
                builder.AddContent(3, ChildContent);
        }

    }
}
