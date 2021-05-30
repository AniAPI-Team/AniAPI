using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class CheckBox
    {
        [Parameter] public string Label { get; set; }
        [Parameter] public bool Checked { get; set; }
        [Parameter] public bool IsInline { get; set; }
        [Parameter] public bool NoLabel { get; set; }
        [Parameter] public Action<bool> OnChange { get; set; }

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

        private void ToggleChecked()
        {
            Checked = !Checked;
            OnChange?.Invoke(Checked);

            StateHasChanged();
        }
    }
}
