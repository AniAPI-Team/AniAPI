using AniAPI.NET.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GUI.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        //public async void Login()
        //{
        //    try
        //    {
        //        AniAPI.NET.AniAPI.Instance.UseImplicitGrant("30f2b27f-74ff-4f7a-9ff8-1f4d5683317c", "http://localhost:5000/aniapi/oauth");
        //        await AniAPI.NET.AniAPI.Instance.Login();
        //    }
        //    catch(Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //}
    }
}
