using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebApp.Pages.Login
{
    public partial class Login
    {
        #region Injection
        
        [Inject] protected Generic Generic { get; set; }
        [Inject] protected HttpClient Client { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ISyncLocalStorageService LocalStorage { get; set; }
        
        #endregion

        #region Parameter
        
        private char Mode { get; set; } = 'L';
        private string User { get; set; }
        private string Password { get; set; }
        private string PwdMessage { get; set; }
        private string Email { get; set; }
        private string ConfirmationEmail { get; set; }
        
        #endregion

        #region Methods

        private Task OnPasswordChanged(ChangeEventArgs e)
        {
            string pwd = e.Value.ToString();

            if (PasswordValid(pwd))
                PwdMessage = "";
            else
                PwdMessage = "La password non rispetta i criteri di sicurezza.";

            return Task.CompletedTask;
        }

        private void SwitchMode()
        {
            if (Mode == 'L')
                Mode = 'R';
            else
                Mode = 'L';

            StateHasChanged();
        }

        private bool PasswordValid(string password)
        {
            Regex hasNumber = new Regex(@"[0-9]+");
            Regex hasUpperChar = new Regex(@"[A-Z]+");
            Regex hasMinimumChars = new Regex(@".{8,}"); // minimo di 8 caratteri

            return hasNumber.IsMatch(password) &&
                    hasUpperChar.IsMatch(password) &&
                    hasMinimumChars.IsMatch(password);
        }

        private void Log_In()
        {
            string user = this.User;
            string password = this.Password;

            LocalStorage.SetItem<string>("token", "imAToken");

            // chiamata api per controllo login

            // redirect
            NavigationManager.NavigateTo("/");
        }

        private void Register()
        {
            string user = this.User;
            string password = this.Password;

            // chiamata api di registrazione


            // redirect

        }

        #endregion

    }
}
