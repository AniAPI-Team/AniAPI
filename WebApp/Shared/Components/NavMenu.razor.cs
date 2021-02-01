using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class NavMenu : BaseComponent
    {
        protected override bool ShouldRender() => false;

        public void Logout()
        {
            localStorage.RemoveItem("token");
            navigationManager.NavigateTo("/Login");
        }
    }
}
