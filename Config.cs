using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownloader
{
    public class Config
    {
        private static string ConfigFileName = "config.json";

        public string SavePath { get; set; }
        public Dictionary<string, string> UrlsAndSubstrings { get; set; }
        public int DelayBetweenScrollsMs { get; set; }
        public int ScrollSpeedDivisor { get; set; }
        public Dictionary<string, bool> AutoLoadSitePreferences { get; set; }



        public Config()
        {
            UrlsAndSubstrings = new Dictionary<string, string>();
            DelayBetweenScrollsMs = 10000; // Default value
            ScrollSpeedDivisor = 4;
            AutoLoadSitePreferences = new Dictionary<string, bool>();
        }

        public bool ShouldAutoLoad(string siteUrl)
        {
            return AutoLoadSitePreferences.TryGetValue(siteUrl, out var autoLoad) && autoLoad;
        }

        public static Config LoadConfig()
        {
            if (File.Exists(ConfigFileName))
            {
                string configJson = File.ReadAllText(ConfigFileName);
                return JsonConvert.DeserializeObject<Config>(configJson);
            }
            return new Config();
        }

        public static void SaveConfig(Config config)
        {
            string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFileName, configJson);
        }

        public void PrintSavedSites()
        {
            Console.WriteLine("Saved Sites:");
            foreach (var kvp in UrlsAndSubstrings)
            {
                Console.WriteLine($"URL: {kvp.Key}, Target Substring: {kvp.Value}");
            }
        }
    }
}
