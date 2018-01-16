using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.ConfigurationEntities
{
    public class SmtpConfiguration
    {
        public string Server { get; set; } = string.Empty; //1
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;

    }
}
