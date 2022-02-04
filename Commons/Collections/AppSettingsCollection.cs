using MongoDB.Driver;
using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commons.Collections
{
    public class AppSettingsCollection : MongoService.ICollection<AppSettings>
    {
        protected override string CollectionName => "app_settings";

        public override void Add(ref AppSettings document)
        {
            throw new NotSupportedException();
        }

        public override long Count()
        {
            return 1;
        }

        public override void Delete(long id)
        {
            throw new NotSupportedException();
        }

        public override void Edit(ref AppSettings document)
        {
            throw new NotSupportedException();
        }

        public override bool Exists(ref AppSettings document, bool updateValues = true)
        {
            return true;
        }

        public override AppSettings Get(long id)
        {
            return this.Collection.Find(Builders<AppSettings>.Filter.Empty).FirstOrDefault();
        }

        public override Paging<AppSettings> GetList<TFilter>(IFilter<TFilter> filter)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<KeyValuePair<string, string>> GetConfiguration()
        {
            AppSettings settings = this.Get(0);

            return new[]
            {
                new KeyValuePair<string, string>("jwt_secret", settings.JWTSecret),
                new KeyValuePair<string, string>("recaptcha_secret", settings.RecaptchaSecret),
                new KeyValuePair<string, string>("resources_version", settings.ResourcesVersion),
                new KeyValuePair<string, string>("smtp_host", settings.Smtp.Host),
                new KeyValuePair<string, string>("smtp_port", settings.Smtp.Port.ToString()),
                new KeyValuePair<string, string>("smtp_username", settings.Smtp.Username),
                new KeyValuePair<string, string>("smtp_password", settings.Smtp.Password),
                new KeyValuePair<string, string>("smtp_address", settings.Smtp.Address),
                new KeyValuePair<string, string>("apilytics_key", settings.ApilyticsKey)
            };
        }
    }
}
