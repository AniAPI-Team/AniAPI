using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared
{
    public partial class MainLayout : LayoutBaseComponent
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();

            // controlli local storage
            if (string.IsNullOrEmpty(localStorage.GetItem<string>("token")))
            {

                navigationManager.NavigateTo("/Login");
            }

            if (localStorage.GetItem<string>("token") == "!isValid")
            {
                navigationManager.NavigateTo("/Login");
            }
        }
    }
}
