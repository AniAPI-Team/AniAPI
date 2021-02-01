using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;

namespace WebApp
{
    public abstract class BaseComponent : ComponentBase
    {
        [Inject] protected HttpClient client { get; set; }
        [Inject] protected NavigationManager navigationManager { get; set; }
        [Inject] protected SpinnerService spinner { get; set; }
        [Inject] protected ISyncLocalStorageService localStorage { get; set; }
        [Inject] protected ISyncSessionStorageService sessionStorage { get; set; }
        [Inject] protected Generic generic { get; set; }
        [Inject] protected IJSRuntime JS { get; set; }
        [Inject] protected UIConfiguration theme { get; set; }
    }
    public abstract class LayoutBaseComponent : LayoutComponentBase
    {
        //[Inject] protected HttpClient client { get; set; }
        [Inject] protected NavigationManager navigationManager { get; set; }
        //[Inject] protected SpinnerService spinner { get; set; }
        [Inject] protected ISyncLocalStorageService localStorage { get; set; }
        //[Inject] protected ISyncSessionStorageService sessionStorage { get; set; }
        //[Inject] protected Generic generic { get; set; }
        //[Inject] protected IJSRuntime JS { get; set; }
        [Inject] protected UIConfiguration theme { get; set; }
    }

}
