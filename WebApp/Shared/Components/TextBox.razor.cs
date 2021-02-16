using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class TextBox
    {
        [Parameter] public string Label { get; set; }
        [Parameter] public string Icon { get; set; }
        [Parameter] public string Value { get; set; }
        [Parameter] public bool IsInline { get; set; }
        [Parameter] public Action<string> OnChange { get; set; }

        private bool HasValue { get; set; }

        protected override void OnInitialized()
        {
            HasValue = !string.IsNullOrEmpty(Value);

            base.OnInitialized();
        }

        private void HasChanged()
        {
            HasValue = !string.IsNullOrEmpty(Value);
            OnChange?.Invoke(Value);
        }

        private void OnKeyUp(ChangeEventArgs e)
        {
            Value = e.Value.ToString();
            HasChanged();
        }

        private void OnReset()
        {
            Value = string.Empty;
            HasChanged();
        }
    }
}
