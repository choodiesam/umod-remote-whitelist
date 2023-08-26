using System.Collections.Generic;
using UnityEngine.Playables;
using Oxide.Core;
using Oxide.Core.Libraries;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("Remote Whitelist", "Choodiesam", "0.1.0")]
    [Description("Whitelist from file or http endpoint")]
    public class RemoteWhitelist : CovalencePlugin
    {
        private PluginConfig config;
        private List whitelist;
        private Timer updateTimer;

        // Hooks

        private void Init()
        {
            updateTimer = timer.Once(1, () => { });
            ReloadConfigFromFile();
            ReloadWhitelist();
        }

        private bool CanUserLogin(string name, string id, string ip)
        {
            if (whitelist == null || whitelist.members == null || !whitelist.members.Contains(id))
            {
                Puts($"Id {id} deny access with name {name} and ip {ip}");
                return false;
            }
            Puts($"Id {id} allow access with name {name} and ip {ip}");
            return true;
        }

        void OnUserConnected(IPlayer player)
        {
            NotifyMemberAction(new MemberAction
            {
                playerName = player.Name,
                playerId = player.Id,
                playerAddress = player.Address,
                action = "connected"
            });
        }

        void OnUserDisconnected(IPlayer player)
        {
            NotifyMemberAction(new MemberAction
            {
                playerName = player.Name,
                playerId = player.Id,
                playerAddress = player.Address,
                action = "disconnected"
            });
        }

        // Commands

        [Command("RemoteWhitelist.whitelist")]
        private void WhitelistCommand(IPlayer player, string command, string[] args)
        {
            if (args.Contains("reload"))
            {
                ReloadWhitelist();
            }
            else
            {
                Puts("Invalid arguments");
            }
        }

        [Command("RemoteWhitelist.config")]
        private void ConfCommand(IPlayer player, string command, string[] args)
        {
            if (args.Contains("reload"))
            {
                ReloadConfigFromFile();
            }
            else
            {
                Puts("Invalid arguments");
            }
        }

        [Command("RemoteWhitelist.remoteUpdate")]
        private void RemoteUpdateCommand(IPlayer player, string command, string[] args)
        {
            if (args.Contains("stop"))
            {
                StopRemoteUpdate();
            }
            else if (args.Contains("start"))
            {
                StartRemoteUpdate();
            }
            else
            {
                Puts("Invalid arguments");
            }
        }

        // Custom stuff

        private void NotifyMemberAction(MemberAction memberAction)
        {
            if (config.LogMemberActionEndpoint.Length != 0)
            {
                webrequest.Enqueue(
                    config.LogMemberActionEndpoint,
                    JsonConvert.SerializeObject(memberAction),
                    (code, response) =>
                    {
                        if (code != 201)
                        {
                            Puts($"Get code {code} from {config.LogMemberActionEndpoint}");
                        }
                    },
                    this, RequestMethod.POST,
                    new Dictionary<string, string>{
                    {"Content-Type", "application/json" }
                    }
                 );
            }
            Puts($"{memberAction.playerName} {memberAction.action}");
        }

        private void StartRemoteUpdate()
        {
            updateTimer.Destroy();

            if (config.UpdateInterval < 5)
            {
                if (config.UpdateInterval != 0)
                {
                    Puts("Update interval cannot be less than 5 seconds!");
                }
                Puts("Remote update is disabled.");
            }
            else
            {
                Puts($"Remote update (every {config.UpdateInterval} seconds) is enabled.");
                updateTimer = timer.Every(config.UpdateInterval, UpdateWhitelistFromRemote);
            }
        }

        private void StopRemoteUpdate()
        {
            updateTimer.Destroy();
            Puts("Remote update is disabled.");
        }

        private void SaveWhitelistToFile(List list)
        {
            Interface.Oxide.DataFileSystem.WriteObject(config.WhitelistFileName, list, true);
            whitelist = list;
        }

        private void ReloadWhitelist()
        {
            whitelist = Interface.Oxide.DataFileSystem.ReadObject<List>(config.WhitelistFileName);
        }

        private void UpdateWhitelistFromRemote()
        {
            webrequest.Enqueue(config.WhitelistEndpoint, null, (code, response) =>
            {
                if (code != 200)
                {
                    Puts($"Get code {code} from {config.WhitelistEndpoint}");
                    return;
                }
                List list = JsonConvert.DeserializeObject<List>(response);
                SaveWhitelistToFile(list);
            }, this, RequestMethod.GET, new Dictionary<string, string> { });
        }

        private void ReloadConfigFromFile()
        {
            config = Config.ReadObject<PluginConfig>();
            Puts("Config reloaded from file");
            StartRemoteUpdate();
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(GetDefaultConfig(), true);
        }

        private PluginConfig GetDefaultConfig()
        {
            return new PluginConfig
            {
                WhitelistEndpoint = "https://remote-whitelist.choodiesam.com/api/whitelist/plugin/token",
                LogMemberActionEndpoint = "https://remote-whitelist.choodiesam.com/api/whitelist/plugin/token/member-action",
                UpdateInterval = 60,
                WhitelistFileName = "WhitelistSteamIds",
            };
        }

        private class MemberAction
        {
            public string playerName;
            public string playerId;
            public string playerAddress;
            public string action;
        }

        private class List
        {
            public string[] members;
            public string createdAt;
        }

        private class PluginConfig
        {
            public string WhitelistEndpoint;
            public string LogMemberActionEndpoint;
            public float UpdateInterval;
            public string WhitelistFileName;
        }
    }
}
