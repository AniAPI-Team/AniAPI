using System;

namespace WebApp
{
    public class SpinnerService
    {
        public Action ShowSpinner;
        public Action HideSpinner;


        public void Show()
        {
            ShowSpinner?.Invoke();
        }
        
        public void Hide()
        {
            HideSpinner?.Invoke();
        }

    }
}
