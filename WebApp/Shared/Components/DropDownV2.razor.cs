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
        [Parameter] public Action<List<string>> CambiaElemento { get; set; }
        [Parameter] public RenderFragment Placeholder { get; set; }
        [Parameter] public bool IsMultiple { get; set; }
        [Parameter] public List<string> SelectedValue { get; set; }
        [Parameter] public string min_width { get; set; }
        [Parameter] public string max_width { get; set; }

        private bool ShowOptions { get; set; }

        protected override void OnParametersSet()
        {
            min_width ??= "160px";
            max_width ??= "250px";

            base.OnParametersSet();
        }

        protected override void OnInitialized()
        {
            ShowOptions = false;

            if ((SelectedValue ??= new List<string>()).Count == 0)
                SelectedValue.Add(DropDownOptions.Values.ToList().FirstOrDefault());

            base.OnInitialized();
        }

        private void ElementChange(ChangeEventArgs e) => CambiaElemento?.Invoke((List<string>)e.Value);

        private void SelectedElementChange(string chiave, string valore)
        {
            if (IsMultiple && SelectedValue.Contains(valore))
                SelectedValue.Remove(valore);
            else if (IsMultiple)
                SelectedValue.Add(valore);
            else
            {
                SelectedValue.Clear();
                SelectedValue.Add(valore);
            }

            ElementChange(new ChangeEventArgs() { Value = SelectedValue });
        }

    }
}
