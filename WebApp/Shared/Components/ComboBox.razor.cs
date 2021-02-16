using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class ComboBox
    {
        [Parameter] public string Label { get; set; }
        [Parameter] public string Search { get; set; }
        [Parameter] public Dictionary<string, string> Options { get; set; }
        [Parameter] public List<string> SelectedValues { get; set; }
        [Parameter] public bool IsInline { get; set; }
        [Parameter] public bool NoLabel { get; set; }
        [Parameter] public Action<List<string>> OnChange { get; set; }

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
        private bool IsInputActive { get; set; }
        private bool HasValue { get; set; }

        protected override void OnInitialized()
        {
            if(SelectedValues == null)
            {
                SelectedValues = new List<string>();
            }
            else
            {
                HasValue = SelectedValues.Count > 0;
            }

            base.OnInitialized();
        }

        private void OnSearchKeyUp(ChangeEventArgs e)
        {
            Search = e.Value.ToString();
            
            StateHasChanged();
        }

        private void OnValueSelected(string key)
        {
            if (SelectedValues.Contains(key))
            {
                SelectedValues.Remove(key);
            }
            else
            {
                SelectedValues.Add(key);
            }

            IsInputActive = false;
            Search = string.Empty;

            HasChanged();
        }

        private void HasChanged()
        {
            HasValue = SelectedValues.Count > 0;
            OnChange?.Invoke(SelectedValues);

            StateHasChanged();
        }

        private void OnFocusInput()
        {
            IsInputActive = true;

            OnOpenDropdown();

            StateHasChanged();
        }

        private void OnOpenDropdown()
        {   
            IsDropdownActive = true;

            StateHasChanged();
        }

        private void OnCloseDropdown()
        {
            if (IsInputActive && !string.IsNullOrEmpty(Search))
            {
                return;
            }

            IsDropdownActive = false;
            IsInputActive = false;
            Search = string.Empty;

            StateHasChanged();
        }

        private void OnReset()
        {
            SelectedValues = new List<string>();

            HasChanged();
        }
    }
}
