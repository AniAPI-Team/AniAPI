using Blazored.LocalStorage;
using Commons;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared
{
    public partial class MainLayout
    {
        //[Inject] protected UIConfiguration Theme { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ISyncLocalStorageService LocalStorage { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            User user = LocalStorage.GetItem<User>("user");

            if (user != null)
            {
                NavigationManager.NavigateTo("/App");
            }
            // TODO: Rimuovere else
            else
            {
                user = new User()
                {
                    Username = "Pippo",
                    Email = "pippoAnimeLover@gmail.com",
                    EmailVerified = false,
                    Gender = Commons.Enums.UserGenderEnum.MALE,
                    Id = 1,
                    LastLoginDate = DateTime.Now,
                    CreationDate = DateTime.Now.AddDays(-7),
                    UpdateDate = DateTime.Now,
                    Localization = "it",
                    Role = Commons.Enums.UserRoleEnum.BASIC
                };

                LocalStorage.SetItem<User>("user", user);

                NavigationManager.NavigateTo("/App");
            }



            // controlli local storage
            //if (string.IsNullOrEmpty(LocalStorage.GetItem<string>("token")))
            //{
            //
            //    NavigationManager.NavigateTo("/Login");
            //}
            //
            //if (LocalStorage.GetItem<string>("token") == "!isValid")
            //{
            //    NavigationManager.NavigateTo("/Login");
            //}
        }
    }
}
