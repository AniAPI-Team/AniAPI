using Commons;
using Commons.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace WebApp.Shared.Components
{
    public partial class PrequelSequel
    {
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected Generic Generic { get; set; }

        [Parameter] public Anime Prequel { get; set; }
        [Parameter] public Anime Sequel { get; set; }
        [Parameter] public string Localization { get; set; }
    }
}
