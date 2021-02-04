using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class DropDown
    {
        [Parameter] public Dictionary<string, string> DropDownOptions { get; set; }
        [Parameter] public Action<string> CambiaElemento { get; set; }
        [Parameter] public RenderFragment Placeholder { get; set; }
        [Parameter] public bool IsMultiple { get; set; }

        private void ElementChange(ChangeEventArgs e) => CambiaElemento?.Invoke((string)e.Value);

    }
}
