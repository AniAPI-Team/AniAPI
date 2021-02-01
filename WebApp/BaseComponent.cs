using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace WebApp.Pages.AnimePages
{
    public abstract class BaseComponent : ComponentBase
    {
        [Inject] protected HttpClient client { get; set; }
        [Inject] protected NavigationManager navigationManager { get; set; }
        [Inject] protected SpinnerService spinner { get; set; }
    }

}
