using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.ConfigurationEntities
{
    public class ConnectionStrings
    {
        public string SqlConnection { get; set; } = string.Empty;
        public MongoDb MongoDb { get; set; } = null;
    }

}
