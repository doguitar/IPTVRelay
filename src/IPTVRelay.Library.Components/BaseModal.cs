using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library.Components
{
    public abstract class BaseModal<T, E> : BaseComponent where E : EventArgs
    {
        private long modelId;

        [Parameter] public EventCallback OnCancel { get; set; }
        [Parameter] public EventCallback<E> OnSave { get; set; }
        [Parameter] public long ModelId { get => modelId; set { modelId = value; Reload = true; } }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string LoadingMessage { get; set; } = "Loading...";

        private bool Reload { get; set; } = true;
        protected bool CanSave { get; set; }

        private bool Initialized { get; set; }

        protected T? Model { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (IsLoading)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "loading-spinner");
                builder.OpenElement(2, "span");
                builder.AddContent(3, LoadingMessage);
                builder.CloseElement();
                builder.CloseElement();
            }
            if (Initialized)
            {
                base.BuildRenderTree(builder);
                builder.AddContent(3, ChildContent);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (Reload)
            {
                await SetLoading();
                await InvokeAsync(StateHasChanged);
                await Load();
                await SetLoaded();
                Initialized = true;
                await InvokeAsync(StateHasChanged);
            }
        }

        public abstract Task Load();
        public abstract Task Save();
        public virtual async Task Cancel()
        {
            await OnCancel.InvokeAsync();
        }

    }
    public abstract class BaseModal<T> : BaseModal<T, ModalEventArgs<T>>
    {
        public override async Task Save()
        {
            await OnSave.InvokeAsync(new ModalEventArgs<T>(Model));
        }
    }
    public class ModalEventArgs<T>(T model) : EventArgs
    {
        public T Model { get; set; } = model;
    }
}
