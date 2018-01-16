using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.ConfigurationEntities
{
    #region ^Configuration as per the appSettings.json file
    public class CustomConfigurations
    {
        public ConnectionStrings ConnectionStrings { get; set; } = null;
        public SmtpConfiguration Smtp { get; set; } = null;
    }
    
   
    #endregion ~Configuration as per the appSettings.json file - 1
}
