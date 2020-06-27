using Bot.Configuration;
using Bot.Entities;
using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bot.Features.GlobalAccounts
{
    public class GlobalGuildAccounts : IGlobalAccounts
    {
        private readonly ConcurrentDictionary<ulong, GlobalGuildAccount> serverAccounts = new ConcurrentDictionary<ulong, GlobalGuildAccount>();
        private readonly JsonDataStorage _jsonDataStorage;

        public GlobalGuildAccounts(JsonDataStorage jsonDataStorage)
        {
            _jsonDataStorage = jsonDataStorage;
            var info = System.IO.Directory.CreateDirectory(Path.Combine(Constants.ResourceFolder,Constants.ServerAccountsFolder));
            var files = info.GetFiles("*.json");
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    var server = jsonDataStorage.RestoreObject<GlobalGuildAccount>(Path.Combine(file.Directory.Name, file.Name));
                    serverAccounts.TryAdd(server.Id, server);
                }
            }
            else
            {
                serverAccounts = new ConcurrentDictionary<ulong, GlobalGuildAccount>();
            }
        }

        public GlobalGuildAccount GetById(ulong id)
        {
            return serverAccounts.GetOrAdd(id, (key) =>
            {
                var newAccount = new GlobalGuildAccount(id);
                _jsonDataStorage.StoreObject(newAccount, Path.Combine(Constants.ServerAccountsFolder, $"{id}.json"), useIndentations: true);
                return newAccount;
            });
        }

        public GlobalGuildAccount GetFromDiscordGuild(IGuild guild)
        {
            return GetById(guild.Id);
        }
        public List<GlobalGuildAccount> GetAllAccounts()
        {
            return serverAccounts.Values.ToList();
        }
        /// <summary>
        /// This rewrites ALL ServerAccounts to the harddrive... Strongly recommend to use SaveAccounts(id1, id2, id3...) where possible instead
        /// </summary>
        public void SaveAccounts()
        {
            foreach (var id in serverAccounts.Keys)
            {
                SaveAccounts(id);
            }
        }

        /// <summary>
        /// Saves one or multiple Accounts by provided Ids
        /// </summary>
        public void SaveAccounts(params ulong[] ids)
        {
            foreach (var id in ids)
            {
                _jsonDataStorage.StoreObject(GetById(id), Path.Combine(Constants.ServerAccountsFolder, $"{id}.json"), useIndentations: true);
            }
        }
    }
}
