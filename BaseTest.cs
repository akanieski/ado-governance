using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ADO.Governance
{
    public class Configuration
    {
        #region Read Only Properties
        [JsonIgnore]
        public bool IsLegacyServicesUrl => OrganizationUrl.ToLower().Contains(".visualstudio.com");
        [JsonIgnore]
        public bool IsServices =>
            OrganizationUrl.ToLower().Contains(".visualstudio.com") ||
            OrganizationUrl.ToLower().Contains("dev.azure.com");
        #endregion

        [JsonPropertyName("OrganizationUrl")]
        public string OrganizationUrl { get; set; }
        [JsonPropertyName("PersonalAccessToken")]
        public string PersonalAccessToken { get; set; }

        [JsonPropertyNameAttribute("CheckGroupForExplicitAccess")]
        public string[] CheckGroupForExplicitAccess { get; set; } = new string[] { };
        [JsonPropertyNameAttribute("CheckProjectGroupsForExplicitAccess")]
        public string[] CheckProjectGroupsForExplicitAccess { get; set; } = new string[] { };
    }
    public class BaseTest
    {
        public static string ConfigFilePath { get; set; } = "../../../config.json";
        private static Configuration _config { get; set; } = null;
        public Configuration Config { get; private set; } = null;

        public BaseTest()
        {
            ConfigFilePath = System.IO.Path.GetFullPath(ConfigFilePath);
            if (!System.IO.File.Exists(ConfigFilePath))
            {
                throw new System.IO.FileNotFoundException($"ERROR: Could not find {ConfigFilePath} in root of test project.");
            }
            Config = GetConfig();
        }

        public static Configuration GetConfig() => _config = _config ?? JsonSerializer.Deserialize<Configuration>(System.IO.File.ReadAllText(ConfigFilePath));
    }
}