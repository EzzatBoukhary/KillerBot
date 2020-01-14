using Bot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bot.Configuration
{
    public class JsonDataStorage : IDataStorage
    {
        private readonly string resourcesFolder = Constants.ResourceFolder;

        public JsonDataStorage()
        {
            if (!Directory.Exists(resourcesFolder))
            {
                Directory.CreateDirectory(resourcesFolder);
            }
        }

        #region Help Modules

        /// <summary>
        /// Saves a list of help modules
        /// </summary>
        /// <param name="helpModules"></param>
        /// <param name="filePath"></param>
        public static void SaveHelpModules(IEnumerable<HelpModule> helpModules, string filePath)
        {
            string json = JsonConvert.SerializeObject(helpModules, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a list of all the help modules from a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IEnumerable<HelpModule> LoadHelpModules(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<HelpModule>>(json);
        }

        #endregion

        public void StoreObject(object obj, string file)
        {
            StoreObject(obj, file, true);
        }

        public void StoreObject(object obj, string file, Formatting formatting)
        {
            string json = JsonConvert.SerializeObject(obj, formatting);
            string filePath = String.Concat(resourcesFolder, "/", file);
            File.WriteAllText(filePath, json);
        }

        public void StoreObject(object obj, string file, bool useIndentations)
        {
            var formatting = (useIndentations) ? Formatting.Indented : Formatting.None;
            StoreObject(obj, file, formatting);
        }

        public T RestoreObject<T>(string file)
        {
            string json = GetOrCreateFileContents(file);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool KeyExists(string key)
        {
            return LocalFileExists(key);
        }

        public bool LocalFileExists(string file)
        {
            string filePath = String.Concat(resourcesFolder, "/", file);
            return File.Exists(filePath);
        }
        /// <summary>
		/// Checks if a saved user file exists
		/// </summary>
		/// <param name="filePath">The path to the user save file</param>
		/// <returns></returns>
		public static bool SaveExists(string filePath)
        {
            return File.Exists(filePath);
        }
        private string GetOrCreateFileContents(string file)
        {
            string filePath = String.Concat(resourcesFolder, "/", file);
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");
                return "";
            }
            return File.ReadAllText(filePath);
        }
    }
}
