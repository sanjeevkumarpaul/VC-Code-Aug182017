using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.ConfigurationEntities
{
    public class MongoDb
    {
        public string ConnectionString { get { return $"mongodb://{Server}:{Port}"; } }
        public string Server { get; set; } = string.Empty;
        public Int32 Port { get; set; } = 0;
        public string Database { get; set; } = string.Empty;
    }
}
