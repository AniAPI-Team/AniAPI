using System;
using System.Collections.Generic;
using System.Text;

namespace Commons.Enums
{
    public class LocalizationEnum
    {
        private enum Locale
        {
            EN,
            IT,
            FR,
            JP
        }

        private Locale _locale;

        private LocalizationEnum(Locale locale)
        {
            this._locale = locale;
        }

        public override string ToString()
        {
            switch (this._locale)
            {
                case Locale.IT: return "it";
                case Locale.EN: return "en";
                case Locale.FR: return "fr";
                case Locale.JP: return "jp";
            }

            return "en";
        }

        public static string Italian => new LocalizationEnum(Locale.IT).ToString();
        public static string English => new LocalizationEnum(Locale.EN).ToString();
        public static string French => new LocalizationEnum(Locale.FR).ToString();
        public static string Japanese => new LocalizationEnum(Locale.JP).ToString();
    }
}
