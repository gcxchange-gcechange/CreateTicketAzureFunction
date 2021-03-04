using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;

namespace CreateTicketHttp.Tests
{
    class LocalSettings
    {
        public bool IsEncrypted { get; set; }
        public Dictionary<string, string> Values { get; set; }


        public static async Task SetupEnvironment()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string basePath = Path.GetFullPath(@"..\..\..\CreateTicketHttp");
            try
            {
                var settings = JsonConvert.DeserializeObject<LocalSettings>(
                File.ReadAllText(basePath + "\\local.settings.json"));

                foreach (var setting in settings.Values)
                {
                    config.AppSettings.Settings.Remove(setting.Key);
                    config.AppSettings.Settings.Add(setting.Key, setting.Value);
                }
                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception e)
            {
                // no local.settings.json file
            }
                
        }
    }
}
