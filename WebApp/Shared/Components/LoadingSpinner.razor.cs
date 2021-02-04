using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class LoadingSpinner : IDisposable
    {
        #region Injection
        [Inject] protected SpinnerService Spinner { get; set; }
        #endregion

        public bool IsVisible { get; set; } = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Spinner.OnShowSpinner += ShowSpinner;
            Spinner.OnHideSpinner += HideSpinner;
        }

        public void Dispose()
        {
            Spinner.OnShowSpinner -= ShowSpinner;
            Spinner.OnHideSpinner -= HideSpinner;
        }

        public void ShowSpinner()
        {
            //Console.WriteLine($"Chiamato ShowSpinner: {DateTime.Now.ToString()}");
            IsVisible = true;
            StateHasChanged();
        }

        public void HideSpinner()
        {
            //Console.WriteLine($"Chiamato HideSpinner: {DateTime.Now.ToString()}");
            IsVisible = false;
            StateHasChanged();
        }
    }
}
