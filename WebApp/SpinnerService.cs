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
            //Console.WriteLine($"Chiamato Show: {DateTime.Now.ToString()}");
            OnShowSpinner?.Invoke();
        }
        
        public void Hide()
        {
            ((Console.WriteLine($"Chiamato Hide: {DateTime.Now.ToString()}");
            OnHideSpinner?.Invoke();
        }

    }
}
