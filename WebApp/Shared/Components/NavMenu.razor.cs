using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class NavMenu
    {
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ISyncLocalStorageService LocalStorage { get; set; }
        [Inject] protected Generic Generic { get; set; }

        private Dictionary<string, string> DropDownOptions;

        protected override bool ShouldRender() => false;

        protected override void OnInitialized()
        {
            DropDownOptions = new Dictionary<string, string>()
            {
                { "it", "IT" },
                { "en", "EN" },
                { "es", "ES" },
            };
            base.OnInitialized();
        }

        public void Logout()
        {
            LocalStorage.RemoveItem("token");
            NavigationManager.NavigateTo("/Login");
        }

        private void ChangeLocalization(string element) //Seguira la dropdown che cambia la localization
        {
            // Localization save in LocalStorage
            LocalStorage.SetItem<string>("Localization", element);
            //LocalizationChanged?.Invoke();
        }
    }
}
