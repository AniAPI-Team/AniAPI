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
        [Inject] protected HttpClient Http { get; set; }
        [Inject] protected ISyncLocalStorageService localStorage { get; set; }
        [Inject] protected Generic generic { get; set; }
        [Inject] protected SpinnerService spinnerService { get; set; }

        [Parameter] public string Class { get; set; }
        [Parameter] public Dictionary<string, string> DropDownOptions { get; set; }
        [Parameter] public Action<string> CambiaElemento { get; set; }


        // chiamata solo la prima volta che carica la pagina
        protected override async Task OnInitializedAsync()
        {
            // caricamento globalization da DB
            //DropDownOptions = await generic.GetSingleRequest<Dictionary<string, string>>("/api/v1/Settings/GetGlobalization");
            if (DropDownOptions == null)
                DropDownOptions = new Dictionary<string, string>()
            {
                { "it", "IT" },
                { "en", "EN" },
                { "es", "ES" },
            };
        }

        private void ElementChange(ChangeEventArgs e) => CambiaElemento?.Invoke((string)e.Value);

    }
}
