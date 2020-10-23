using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx.Configuration;
using SimpleJSON;
using UnityEngine;


namespace addOns
{
    class DPConfig
    {
        public DPConfig(ConfigFile config)
        {
            this.config = config;
            InitConfig();
            LoadScenesDataFromJson();
        }

        private void InitConfig()
        {
            CfgEnabled = config.Bind(
            "mod_config",
            "enabled",
            true,
            "change to false if you want to disable the mod hooks and run the classic game"
            );
            CfgScenesConfigPath = config.Bind(
            "mod_config",
            "scenes_config_path",
            "scenesConfig.json",
            "path to the scenes config file containing a json with all the scenes data (if it does not exists, it is created automatically)"
            );
            CfgSkipLobbyScreen = config.Bind(
            "mod_config",
            "skip_lobby",
            true,
            "skip the character selection screen and head straight to the run"
            );
            CfgInitCmds = config.Bind(
            "run_config",
            "initial_cmds",
            "no_enemies; kill_all 2;",
            "Commands to run at the beginning of the run (must be ConCommands concatenated with ';' as a seperator)"
            );
            CfgCharacter = config.Bind(
            "run_config",
            "character",
            "Commando",
            "selected character to spawn (Commando,Huntress,Toolbot,Engi,Mage,Merc,Treebot,Loader,Croco,Captain or any character mod you'd like!)"
            );
            CfgSpawnWithDropPod = config.Bind(
            "run_config",
            "drop_pod",
            false,
            "spawn from a falling drop pod or simply appearing on the map"
            );
            CfgArtifactsList = config.Bind(
            "run_config",
            "artifacts",
            "",
            "Artifacts to activate (must be the artifact asset names concatenated with ',' as a seperator) "
            );
            CfgDifficulty = config.Bind(
            "run_config",
            "difficulty",
            "Normal",
            "Desired difficulty (Easy, Normal, Hard or any other difficulty you got)"
            );
            CfgStartingStage = config.Bind(
            "run_config",
            "starting_stage",
            "golemplains",
            "Starting stage"
            );
            CfgFreezeTimer = config.Bind(
            "run_config",
            "freeze_timer",
            true,
            "freeze the timer"
            );
        }

        private void LoadScenesDataFromJson()
        {
            string jsonConfigPath = Path.Combine(Path.GetDirectoryName(config.ConfigFilePath), CfgScenesConfigPath.Value);
            string jsonBuffer = "";
            bool validJsonFile = true;
            bool fileExists = true;

            if (File.Exists(jsonConfigPath))
            {
                jsonBuffer = File.ReadAllText(jsonConfigPath);
                try
                {
                    JSONNode.Parse(jsonBuffer);
                }
                catch
                {
                    Debug.LogWarning("scene config file malformed! using defaults");
                    validJsonFile = false;
                }
            }
            else
            {
                Debug.LogWarning("scene config file not found! using defaults");
                validJsonFile = false;
                fileExists = false;
            }

            if (!validJsonFile)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DebuggingPlains.sampleJson.json"))
                {
                    jsonBuffer = new StreamReader(stream).ReadToEnd();
                }
                if (!fileExists)
                {
                    File.WriteAllText(jsonConfigPath, jsonBuffer);
                }
            }

            this.scenesData = new ScenesData(JSONNode.Parse(jsonBuffer));
        }

        public ConfigEntry<bool> CfgEnabled { get; set; }
        public ConfigEntry<string> CfgScenesConfigPath { get; set; }
        public ConfigEntry<bool> CfgSkipLobbyScreen { get; set; }
        public ConfigEntry<string> CfgInitCmds { get; set; }
        public ConfigEntry<string> CfgCharacter { get; set; }
        public ConfigEntry<bool> CfgSpawnWithDropPod { get; set; }
        public ConfigEntry<string> CfgArtifactsList { get; set; }
        public ConfigEntry<string> CfgDifficulty { get; set; }
        public ConfigEntry<string> CfgStartingStage { get; set; }
        public ConfigEntry<bool> CfgFreezeTimer { get; set; }
        public ScenesData scenesData { get; set; }
        private readonly ConfigFile config;
    }
}
