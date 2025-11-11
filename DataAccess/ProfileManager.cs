using LLMConfigManager.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace LLMConfigManager.DataAccess
{
    public class ProfileManager
    {
        private readonly string _filePath = "profiles.json";

        public List<ModelProfile> LoadProfiles()
        {
            if (!File.Exists(_filePath))
            {
                return new List<ModelProfile>();
            }

            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<ModelProfile>>(json) ?? new List<ModelProfile>();
        }

        public void SaveProfiles(List<ModelProfile> profiles)
        {
            var json = JsonConvert.SerializeObject(profiles, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
