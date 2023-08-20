using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Libraries;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Remote Whitelist", "Choodiesam", "0.1.0")]
    [Description("Whitelist from file or http endpoint")]
    public class RemoteWhitelist : CovalencePlugin
    {
        private PluginConfig config;

        private void Init()
        {
            config = Config.ReadObject<PluginConfig>();

            if (config.UpdateInterval < 10)
            {
                if (config.UpdateInterval != 0)
                {
                    Puts("Update interval cannot be less than 10 seconds!");
                }
                Puts("Remote updating is disabled.");
            }
            else
            {
                Puts($"Remote updating (every {config.UpdateInterval} seconds) is allowed.");
                timer.Every(config.UpdateInterval, UpdateWhitelist);
            }
        }

        private bool CanUserLogin(string name, string id, string ip)
        {
            List list = Interface.Oxide.DataFileSystem.ReadObject<List>(config.WhitelistFileName);

            if (list == null || list.members == null || !list.members.Contains(id))
            {
                Puts($"Id {id} deny access with name {name} and ip {ip}");
                return false;
            }
            Puts($"Id {id} allow access with name {name} and ip {ip}");
            return true;
        }

        private void UpdateWhitelist()
        {
            webrequest.Enqueue(config.ApiUrl + config.ApiToken, null, (code, response) =>
            {
                if (code != 200)
                {
                    Puts($"Get code {code} from {config.ApiUrl + config.ApiToken}");
                    return;
                }
                List list = JsonConvert.DeserializeObject<List>(response);
                Interface.Oxide.DataFileSystem.WriteObject(config.WhitelistFileName, list, true);
            }, this, RequestMethod.GET, new Dictionary<string, string> { }, config.ApiTimeout);
        }

        private class List
        {
            public string[] members;
            public string createdAt;
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(GetDefaultConfig(), true);
        }

        private PluginConfig GetDefaultConfig()
        {
            return new PluginConfig
            {
                ApiUrl = "https://rust.choodiesam.com/api/rust-plugin/whitelist/",
                ApiTimeout = 2,
                ApiToken = "token",
                UpdateInterval = 60,
                WhitelistFileName = "WhitelistSteamIds",
            };
        }

        private class PluginConfig
        {
            public string ApiUrl;
            public float ApiTimeout;
            public string ApiToken;
            public float UpdateInterval;
            public string WhitelistFileName;
        }
    }
}
