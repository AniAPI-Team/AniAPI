using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared
{
    public partial class MainLayout
    {
        [Inject] protected UIConfiguration Theme { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ISyncLocalStorageService LocalStorage { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // controlli local storage
            if (string.IsNullOrEmpty(LocalStorage.GetItem<string>("token")))
            {

                NavigationManager.NavigateTo("/Login");
            }

            if (LocalStorage.GetItem<string>("token") == "!isValid")
            {
                NavigationManager.NavigateTo("/Login");
            }
        }
    }
}
