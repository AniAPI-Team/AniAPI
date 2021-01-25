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

        #endregion

        private string _connectionString = "mongodb://localhost:27017";
        private string _connectionDatabase = "aniapi_dotnet";
        private IMongoDatabase _database;

        public IMongoDatabase Database
        {
            get
            {
                if(this._database == null)
                {
                    MongoClient client = new MongoClient(this._connectionString);
#if DEBUG
                    client.Settings.ConnectTimeout = new TimeSpan(0, 0, 0, 5);
                    client.Settings.ServerSelectionTimeout = new TimeSpan(0, 0, 0, 5);
#endif
                    this._database = client.GetDatabase(this._connectionDatabase);
                }

                return this._database;
            }
        }
    }
}
