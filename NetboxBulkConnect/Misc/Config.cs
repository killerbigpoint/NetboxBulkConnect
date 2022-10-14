using NetboxBulkConnect.Models;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.IO;
using System;

namespace NetboxBulkConnect.Misc
{
    public class Config
    {
        [JsonIgnore]
        public static readonly string ConfigLocation = "NetboxBulkConnect.config";
        [JsonIgnore]
        private static Config ConfigInstance = null;

        public string Server = ""; //Netbox Server IP
        public string ApiToken = ""; //API Token for access
        public bool UseHttpEncryption = true; //Default to true because we have standards

        public Metrics.Type MetricsType = Metrics.Type.Meters; //EU > NA
        public bool UseTooltips = true; //Because some people don't have a brain lmao

        //Setting all of these can be tedious and takes time
        public int NumberOfPorts = 1;
        public int CableType = 0;
        public int CableLength = 20;

        //Explaind how to use these in the tooltips
        public int DeviceAPortSkips = 0;
        public int DeviceBPortSkips = 0;

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
                FileLogging.Append("Config loaded");
            }
            catch (Exception e)
            {
                string error = $"Config file couldn't be loaded: {e.Message}";

                MessageBox.Show(error, "Config");
                FileLogging.Append(error);
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
                string error = $"Config file couldn't be saved: {e.Message}";

                MessageBox.Show(error, "Config");
                FileLogging.Append(error);
            }
        }
    }
}
