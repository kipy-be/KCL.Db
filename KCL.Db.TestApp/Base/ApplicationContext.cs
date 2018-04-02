using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TestApp
{
    public static class ApplicationContext
    {
        public static readonly string Name = Assembly.GetEntryAssembly().GetName().Name;
        public static readonly string Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
        private static readonly string CONF_FILENAME = "kcl_db.conf";

        public static Config Config { get; private set; }

        public static DbInterface DbService { get; private set; }

        public static void Init()
        {
            LoadConfig(GetConfigFile(CONF_FILENAME));
            DbService = new DbInterface(Config.DbHost, Config.DbPort, Config.DbName, Config.DbUser, Config.DbPassword);
            DbService.Connect();
        }

        private static string ConfigPath => Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), Name);

        public static string GetConfigFile(string fileName) => Path.Combine(ConfigPath, fileName);

        private static void LoadConfig(string fileUrl)
        {
            if (!File.Exists(fileUrl))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Config = new Config(){};
                    Directory.CreateDirectory(ConfigPath);
                    SaveConfig(fileUrl);
                    return;
                }
                else
                    throw new Exception($"Error : no config file found ({fileUrl})");
            }

            JObject jo;
            using (StreamReader reader = new StreamReader(new FileStream(fileUrl, FileMode.Open, FileAccess.Read)))
            {
                string json = reader.ReadToEnd();
                jo = JObject.Parse(json);
                Config = jo.ToObject<Config>();

                if (string.IsNullOrWhiteSpace(Config.DbHost)
                    || Config.DbPort == 0
                    || string.IsNullOrWhiteSpace(Config.DbName)
                    || string.IsNullOrWhiteSpace(Config.DbUser)
                    || string.IsNullOrWhiteSpace(Config.DbPassword))
                    throw new Exception("Error : missing data in config file");
            }
        }

        private static void SaveConfig(string fileUrl)
        {
            string json;
            JObject jo;

            using (var writer = new StreamWriter(new FileStream(fileUrl, FileMode.Create, FileAccess.Write)))
            {
                jo = (JObject)JToken.FromObject(Config);
                json = jo.ToString();
                writer.Write(json);
            }
        }
    }
}
