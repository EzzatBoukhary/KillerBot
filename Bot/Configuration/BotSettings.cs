using Bot.Entities;
using System;
using Bot.Helpers;

namespace Bot.Configuration
{
    public class BotSettings
    {
        internal BotConfig config;

        private readonly string configFile = "config.json";
        private readonly JsonDataStorage jsonDataStorage;

        public BotSettings(JsonDataStorage jsonDataStorage)
        {
            this.jsonDataStorage = jsonDataStorage;
            LoadConfig();
        }

        internal void LoadConfig()
        {
            if (jsonDataStorage.LocalFileExists(configFile))
            {
                config = jsonDataStorage.RestoreObject<BotConfig>(configFile);
            }
            else
            {
                // Setting up defaults
                config = new BotConfig()
                {
                    Token = "MzEwMTI2ODY4MDE3OTcxMjAy.XNnLww.6KF8lUgiLMA390fKk7lKJZ3lxsM"
                };
                jsonDataStorage.StoreObject(config, configFile, useIndentations: true);
            }
        }

    }
}
