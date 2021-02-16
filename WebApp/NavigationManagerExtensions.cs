using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public static class NavigationManagerExtensions
    {
        public static bool TryGetQueryString<T>(this NavigationManager navigationManager, string key, out T value)
        {
            Uri uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

            if(QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var queryValue))
            {
                Type valueType = typeof(T);

                if(valueType == typeof(int) && int.TryParse(queryValue, out int intValue))
                {
                    value = (T)(object)intValue;
                    return true;
                }

                if(valueType == typeof(decimal) && decimal.TryParse(queryValue, out decimal decValue))
                {
                    value = (T)(object)decValue;
                    return true;
                }

                if(valueType == typeof(string))
                {
                    value = (T)(object)queryValue.ToString();
                    return true;
                }
            }

            value = default(T);
            return false;
        }
    }
}
