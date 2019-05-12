using Bot.Configuration;
using System;
using Bot.Entities;
using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bot.Features.GlobalAccounts
{
    public class GlobalUserAccounts : IGlobalAccounts, IGlobalUserAccounts
    {
        private readonly ConcurrentDictionary<ulong, GlobalUserAccount> userAccounts = new ConcurrentDictionary<ulong, GlobalUserAccount>();
        private readonly DirectoryInfo _directoryInfo;
        private readonly JsonDataStorage _jsonDataStorage;

        private readonly string _directoryPath =
            Path.Combine(Constants.ResourceFolder, Constants.UserAccountsFolder);

        public GlobalUserAccounts(JsonDataStorage jsonDataStorage)
        {
            _jsonDataStorage = jsonDataStorage;
            _directoryInfo = Directory.CreateDirectory(_directoryPath);
            var files = _directoryInfo.GetFiles("*.json");
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    var user = jsonDataStorage.RestoreObject<GlobalUserAccount>(Path.Combine(file.Directory.Name, file.Name));
                    userAccounts.TryAdd(user.Id, user);
                }
            }
            else
            {
                userAccounts = new ConcurrentDictionary<ulong, GlobalUserAccount>();
            }
        }

        
        public string GetAccountFilePath(ulong id)
        {
            var filePath = Path.Combine(Path.Combine(_directoryPath, $"{id}.json"));
            return File.Exists(filePath) ? filePath : String.Empty;
        }

        public bool DeleteAccountFile(ulong accountId)
        {
            if (!userAccounts.TryRemove(accountId, out var account)) return false;
            var file = GetAccountFilePath(accountId);
            if (String.IsNullOrEmpty(file)) return false;
            File.Delete(file);
            return true;
        }
        
        public GlobalUserAccount GetById(ulong id)
        {
            return userAccounts.GetOrAdd(id, (key) =>
            {
                var newAccount = new GlobalUserAccount(id);
                _jsonDataStorage.StoreObject(newAccount, Path.Combine(Constants.UserAccountsFolder, $"{id}.json"), useIndentations: true);
                return newAccount;
            });
        }

        public GlobalUserAccount GetFromDiscordUser(IUser user)
        {
            return GetById(user.Id);
        }

        public List<GlobalUserAccount> GetAllAccounts()
        {
            return userAccounts.Values.ToList();
        }

        public List<GlobalUserAccount> GetFilteredAccounts(Func<GlobalUserAccount, bool> filter)
        {
            return userAccounts.Values.Where(filter).ToList();
        }

        /// <summary>
        /// This rewrites ALL UserAccounts to the harddrive... Strongly recommend to use SaveAccounts(id1, id2, id3...) where possible instead
        /// </summary>
        public void SaveAccounts()
        {
            foreach (var id in userAccounts.Keys)
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
                _jsonDataStorage.StoreObject(GetById(id), Path.Combine(Constants.UserAccountsFolder, $"{id}.json"), useIndentations: true);
            }
        }
    }
}
