using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class LoadingSpinner : BaseComponent, IDisposable
    {
        public bool IsVisible { get; set; } = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            spinner.OnShowSpinner += ShowSpinner;
            spinner.OnHideSpinner += HideSpinner;
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

        public void Dispose()
        {
            spinner.OnShowSpinner -= ShowSpinner;
            spinner.OnHideSpinner -= HideSpinner;
        }
    }
}
