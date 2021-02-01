using System;
using System.Threading.Tasks;

namespace WebApp
{
    public class SpinnerService
    {
        public Action OnShowSpinner;
        public Action OnHideSpinner;


        public void Show()
        {
            OnShowSpinner?.Invoke();
        }
        
        public void Hide()
        {
            OnHideSpinner?.Invoke();
        }

    }
}
