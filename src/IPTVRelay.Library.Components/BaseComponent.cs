using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library.Components
{
    public class BaseComponent : ComponentBase
    {
        protected bool IsLoading { get; private set; }
        protected async Task SetLoading()
        {
            if (!IsLoading)
            {
                IsLoading = true;
                await InvokeAsync(StateHasChanged);
            }
        }
        protected async Task SetLoaded()
        {
            if (IsLoading)
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
