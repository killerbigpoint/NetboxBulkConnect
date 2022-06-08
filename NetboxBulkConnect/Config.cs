using NetboxBulkConnect.Models;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.IO;
using System;

namespace NetboxBulkConnect
{
    public class Config
    {
        [JsonIgnore]
        public static readonly string ConfigLocation = "NetboxBulkConnect.config";
        [JsonIgnore]
        private static Config ConfigInstance = null;

        public string Server = "";
        public string ApiToken = "";
        public bool UseHttpEncryption = true; //Default to true because we have standards
        public Metrics.Type MetricsType = Metrics.Type.Meters; //EU > NA

        public static Config GetConfig()
        {
            return ConfigInstance;
        }

        public static void LoadConfig()
        {
            if (File.Exists(ConfigLocation) == false)
            {
                ConfigInstance = new Config();
                return;
            }

            try
            {
                ConfigInstance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigLocation));
            }
            catch (Exception e)
            {
                MessageBox.Show($"Config file couldn't be loaded: {e.Message}", "Config");
            }
        }

        public static void SaveConfig()
        {
            try
            {
                File.WriteAllText(ConfigLocation, JsonConvert.SerializeObject(ConfigInstance));
            }
            catch (Exception e)
            {
                MessageBox.Show($"Config file couldn't be saved: {e.Message}", "Config");
            }
        }
    }
}
