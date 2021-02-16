using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class SelectBox
    {
        [Parameter] public string Label { get; set; }
        [Parameter] public Dictionary<string, string> Options { get; set; }
        [Parameter] public string SelectedValue { get; set; }
        [Parameter] public bool IsInline { get; set; }
        [Parameter] public bool NoLabel { get; set; }
        [Parameter] public Action<string> OnChange { get; set; }

        private List<string> CssClass
        {
            get
            {
                List<string> cssClass = new List<string>();

                if (IsInline)
                {
                    cssClass.Add("col");
                }

                if (NoLabel)
                {
                    cssClass.Add("no-label");
                }

                return cssClass;
            }
        }
        private bool IsDropdownActive { get; set; }
        private bool HasValue { get; set; }

        protected override void OnInitialized()
        {
            if(SelectedValue != null)
            {
                HasValue = true;
            }

            base.OnInitialized();
        }

        private void OnValueSelected(string key)
        {
            SelectedValue = key;

            HasChanged();
        }

        private void HasChanged()
        {
            HasValue = SelectedValue != null;
            OnChange?.Invoke(SelectedValue);

            StateHasChanged();
        }

        private void OnOpenDropdown()
        {
            IsDropdownActive = true;

            StateHasChanged();
        }

        private void OnCloseDropdown()
        {
            IsDropdownActive = false;

            StateHasChanged();
        }

        private void OnReset()
        {
            SelectedValue = null;

            HasChanged();
        }
    }
}
