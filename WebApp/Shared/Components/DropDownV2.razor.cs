using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class DropDownV2
    {
        [Parameter] public Dictionary<string, string> DropDownOptions { get; set; }
        [Parameter] public Action<string> CambiaElemento { get; set; }
        [Parameter] public RenderFragment Placeholder { get; set; }
        [Parameter] public bool IsMultiple { get; set; }
        [Parameter] public string SelectedValue { get; set; }
        [Parameter] public string Width { get; set; }

        private bool ShowOptions { get; set; }

        protected override void OnParametersSet()
        {
            Width ??= "125px";

            base.OnParametersSet();
        }

        protected override void OnInitialized()
        {
            ShowOptions = false;

            if (string.IsNullOrEmpty(SelectedValue))
                SelectedValue = DropDownOptions.Values.ToList().FirstOrDefault();

            base.OnInitialized();
        }

        private void ElementChange(ChangeEventArgs e) => CambiaElemento?.Invoke((string)e.Value);

        private void SelectedElementChange(string chiave, string valore)
        {
            SelectedValue = valore;
            ElementChange(new ChangeEventArgs() { Value = valore });
        }

    }
}
