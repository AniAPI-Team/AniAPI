using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService.Helpers
{
    internal class Connection
    {
        #region Singleton

        private static Connection _instance;
        public static Connection Instance
        {
            get
            {
                return _instance ?? (_instance = new Connection());
            }
        }
        private Connection()
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IConfiguration config = new ConfigurationBuilder().
                AddJsonFile("appsettings.json", false, false).
                AddJsonFile($"appsettings.{env}.json", false, false).
                AddEnvironmentVariables().
                Build();

            this._connectionString = config["MongoDB:Host"];
            this._connectionDatabase = config["MongoDB:Database"];
        }

        #endregion

        private string _connectionString;
        private string _connectionDatabase;
        private IMongoDatabase _database;

        public IMongoDatabase Database
        {
            get
            {
                if(this._database == null)
                {
                    MongoClient client = new MongoClient(this._connectionString);
                    this._database = client.GetDatabase(this._connectionDatabase);
                }

                return this._database;
            }
        }
    }
}
