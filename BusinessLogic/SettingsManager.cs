using ClaudeCodeLLMConfigManager.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClaudeCodeLLMConfigManager.BusinessLogic
{
    public class SettingsManager
    {
        public Dictionary<string, string> ParseStatementBlock(string statementBlock)
        {
            var envVars = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(statementBlock))
            {
                return envVars;
            }

                        var regex = new Regex("\\$Env:(\\S+)\\s*=\\s*\"?([^\n\r\"]*)\"?", RegexOptions.Multiline);            var matches = regex.Matches(statementBlock);

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();
                    envVars[key] = value;
                }
            }

            return envVars;
        }

        public bool ApplyToSettings(string settingsJsonPath, Dictionary<string, string> envVars)
        {
            try
            {
                var json = File.ReadAllText(settingsJsonPath);
                var jsonObj = JObject.Parse(json);

                if (!jsonObj.TryGetValue("env", out var envToken))
                {
                    envToken = new JObject();
                    jsonObj["env"] = envToken;
                }

                var envObj = (JObject)envToken;

                foreach (var kvp in envVars)
                {
                    envObj[kvp.Key] = kvp.Value;
                }

                File.WriteAllText(settingsJsonPath, jsonObj.ToString(Formatting.Indented));
                return true;
            }
            catch { return false; }
        }

        public bool ResetSettings(string settingsJsonPath)
        {
            try
            {
                var json = File.ReadAllText(settingsJsonPath);
                var jsonObj = JObject.Parse(json);

                // If 'env' property exists, replace it with an empty object.
                if (jsonObj.ContainsKey("env"))
                {
                    jsonObj["env"] = new JObject();
                }
                // If it doesn't exist, there's nothing to reset, so it's a success.

                File.WriteAllText(settingsJsonPath, jsonObj.ToString(Formatting.Indented));
                return true;
            }
            catch { return false; }
        }

        public void LaunchClaudeWithProfile(ModelProfile profile, string workingPath = null)
        {
            if (profile == null) return;
            var commandParts = new List<string>();

            // 如果指定了工作路径，先添加 cd 命令
            if (!string.IsNullOrEmpty(workingPath))
            {
                commandParts.Add($"cd '{workingPath}'");
            }

            var envVars = ParseStatementBlock(profile.StatementBlock);
            foreach (var kvp in envVars)
            {
                commandParts.Add($"$Env:{kvp.Key}='{kvp.Value}'");
            }
            commandParts.Add("claude");
            var fullCommand = string.Join("; ", commandParts);
            Process.Start("powershell.exe", $"-NoExit -Command \"{fullCommand}\" ");
        }

        public void LaunchClaudeDirectly(string workingPath = null)
        {
            var commandParts = new List<string>();

            // 如果指定了工作路径，先添加 cd 命令
            if (!string.IsNullOrEmpty(workingPath))
            {
                commandParts.Add($"cd '{workingPath}'");
            }

            commandParts.Add("claude");
            var fullCommand = string.Join("; ", commandParts);
            Process.Start("powershell.exe", $"-NoExit -Command \"{fullCommand}\" ");
        }
    }
}