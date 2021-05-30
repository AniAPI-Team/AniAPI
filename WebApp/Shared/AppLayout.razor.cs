using Blazored.LocalStorage;
using Commons;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared
{
    public partial class AppLayout
    {
        #region Injection

        [Inject] protected UIConfiguration Theme { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ISyncLocalStorageService LocalStorage { get; set; }

        #endregion

        protected override void OnInitialized()
        {
            base.OnInitialized();

            User user = LocalStorage.GetItem<User>("user");

            if (user == null)
            {
                NavigationManager.NavigateTo("/Login");
            }
        }

        public void Logout()
        {
            LocalStorage.RemoveItem("user");
            NavigationManager.NavigateTo("/Login");
        }

        public void ChangeTheme()
        {
            Theme.UseDarkTheme = !Theme.UseDarkTheme;
            //StateHasChanged();
        }
    }
}
