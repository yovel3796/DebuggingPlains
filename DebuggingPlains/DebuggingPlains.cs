using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using DebugToolkit;
using System.Diagnostics;
using UnityEngine;
using EntityStates.Engi.EngiWeapon;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections.Generic;
using DebugDiff;
using System.IO;
using SimpleJSON;
using System.Reflection;

namespace MyUserName
{
    [R2APISubmoduleDependency(new string[]
    {
        "LanguageAPI",
        "PrefabAPI",
        "LoadoutAPI"
    })]

    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.harbingerofme.DebugToolkit")]
    //Change these
    [BepInPlugin("com.Yovelz.DebuggingPlains", "DebuggingPlains", "0.0.1")]
    public class DebuggingPlains : BaseUnityPlugin
    {
        public static ConfigEntry<bool> CfgEnabled { get; set; }
        public static ConfigEntry<string> CfgScenesConfigPath { get; set; }
        public static ConfigEntry<bool> CfgSkipLobbyScreen { get; set; }
        public static ConfigEntry<string> CfgInitCmds { get; set; }
        public static ConfigEntry<string> CfgCharacter { get; set; }
        public static ConfigEntry<bool> CfgSpawnWithDropPod { get; set; }
        public static ConfigEntry<string> CfgArtifactsList { get; set; }
        public static ConfigEntry<string> CfgDifficulty { get; set; }
        public static ConfigEntry<string> CfgStartingStage { get; set; }
        public static ConfigEntry<bool> CfgFreezeTimer { get; set; }
        private ScenesData scenesData;

        public void Awake()
        {
            InitConfigs();
            if (CfgEnabled.Value)
            {
                // HOOK - land directly to lobby screen at the start of the game
                On.RoR2.Networking.GameNetworkManager.CCSetScene += GameNetworkManager_CCSetScene;
                if (CfgSkipLobbyScreen.Value)
                {
                    // HOOK - start the game immidiatley after creating lobby
                    On.RoR2.PreGameRuleVoteController.ServerHandleClientVoteUpdate += PreGameRuleVoteController_ServerHandleClientVoteUpdate;
                }
                // commands list to execute at the start of the debug run
                // TODO: fix the "Return to menu" restart run (submitting no_enemies again which makes this false)
                Run.onRunStartGlobal += (obj) => {
                    // add error handling (when Network user list is empty/null?)
                    RoR2.Console.instance.SubmitCmd(NetworkUser.readOnlyLocalPlayersList[0], CfgInitCmds.Value, false);
                };
                // HOOK - add all run configs from config file to the debug run
                On.RoR2.PreGameRuleVoteController.UpdateGameVotes += PreGameRuleVoteController_UpdateGameVotes;
                // HOOK - always spawn on chosen map
                On.RoR2.Run.PickNextStageScene += Run_PickNextStageScene;
                // HOOK - respawn always at a fixed location
                On.RoR2.Stage.RespawnCharacter += Stage_RespawnCharacter;
                // HOOK - populate the scene with our additional stuff
                On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
                // HOOK - stop the run timer
                if (CfgFreezeTimer.Value)
                {
                    On.RoR2.Run.ShouldUpdateRunStopwatch += (orig, self) => { return false; };
                } 
                // HOOK - allows you to connect a few clients to a single localhost server
                On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
                // TODO: add xyz character position text every frame for debugging (maybe with ObjectivePanelController.GetObjectiveSources?)
            }
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            ScenesData.SceneData scene = this.scenesData.GetSceneDataBySceneName(SceneInfo.instance.sceneDef.baseSceneName);
            if (scene != null)
            {
                foreach (ObjectsLocations.ObjectToSpawn obj in scene.objectsList.GetObjectsList())
                {
                    SpawnCard spawnCard = Resources.Load<SpawnCard>(obj.objName);
                    if (spawnCard)
                    {
                        GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Direct,
                            position = obj.position
                        }, new Xoroshiro128Plus(2)));
                        gameObject.transform.rotation = obj.rotation;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"No spawn card with name {obj.objName} found! ignoring.");
                        continue;
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"no scene data for scene {SceneInfo.instance.sceneDef.baseSceneName} found!");
            }
        }

        private void Stage_RespawnCharacter(On.RoR2.Stage.orig_RespawnCharacter orig, Stage self, CharacterMaster characterMaster)
        {
            if (!characterMaster)
            {
                return;
            }
            //UnityEngine.Debug.Log($"x: {vector.x} y:{vector.y} z:{vector.z}");
            //UnityEngine.Debug.Log($"rotation: x: {quaternion.x} y:{quaternion.y} z:{quaternion.z} w:{quaternion.w}");
            Vector3 vector = Vector3.zero;
            Quaternion quaternion = Quaternion.identity;

            ScenesData.SceneData scene = this.scenesData.GetSceneDataBySceneName(self.sceneDef.baseSceneName);
            // TODO: make the character spawn facing the objects and not to the side
            if (scene != null)
            {
                vector = scene.playerSpawnPosition;
                quaternion = scene.playerSpawnRotation;
            }
            else
            {
                Transform playerSpawnTransform = self.GetPlayerSpawnTransform();
                if (playerSpawnTransform)
                {
                    vector = playerSpawnTransform.position;
                    quaternion = playerSpawnTransform.rotation;
                }
            }

            SurvivorIndex survivorIndex = SurvivorCatalog.FindSurvivorIndex(CfgCharacter.Value);
            if (survivorIndex == SurvivorIndex.None)
            {
                UnityEngine.Debug.LogWarning($"Survivor {CfgCharacter.Value} could not be found!");
                
                survivorIndex = SurvivorCatalog.FindSurvivorIndex((string)CfgCharacter.DefaultValue);
            }
            characterMaster.bodyPrefab = SurvivorCatalog.GetSurvivorDef(survivorIndex).bodyPrefab;
            CharacterBody a = characterMaster.Respawn(vector, quaternion, true);
            if (CfgSpawnWithDropPod.Value)
            {
                Run.instance.HandlePlayerFirstEntryAnimation(a, vector, quaternion);
            }
        }

        private void Run_PickNextStageScene(On.RoR2.Run.orig_PickNextStageScene orig, Run self, SceneDef[] choices)
        {
            SceneDef debuggingPlains = SceneCatalog.GetSceneDefFromSceneName(CfgStartingStage.Value);
            if (!debuggingPlains)
            {
                UnityEngine.Debug.LogWarning($"Scene {CfgStartingStage.Value} not found! going to debugging plains");
                debuggingPlains = SceneCatalog.GetSceneDefFromSceneName("golemplains");
            }
            List<string> newMapOverrides = new List<string>();
            foreach (var mapOverride in debuggingPlains.sceneNameOverrides)
            {
                if (mapOverride == CfgStartingStage.Value)
                {
                    newMapOverrides.Add(mapOverride);
                }
            }
            
            debuggingPlains.nameToken = "Debugging Plains";
            debuggingPlains.subtitleToken = "Ground 0011010010";
            debuggingPlains.sceneNameOverrides = newMapOverrides;
            self.nextStageScene = debuggingPlains;
        }

        private void PreGameRuleVoteController_UpdateGameVotes(On.RoR2.PreGameRuleVoteController.orig_UpdateGameVotes orig)
        {
            orig();
            UnityEngine.Debug.Log(CfgArtifactsList.Value);
            foreach (string artifact in CfgArtifactsList.Value.Split(','))
            {
                if (artifact == "")
                {
                    continue;
                }
                RuleChoiceDef ruleChoiceDef = RuleCatalog.FindChoiceDef($"Artifacts.{artifact}.On");
                if (ruleChoiceDef != null)
                {
                    PreGameController.instance.ApplyChoice(ruleChoiceDef.globalIndex);
                }
                else
                {
                    UnityEngine.Debug.Log($"[DebugDiff] Could not find Artifact {artifact}! ignoring");
                }
            }

            RuleChoiceDef difficultyRuleDef = RuleCatalog.FindChoiceDef($"Difficulty.{CfgDifficulty.Value}");
            if (difficultyRuleDef != null)
            {
                PreGameController.instance.ApplyChoice(difficultyRuleDef.globalIndex);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[DebuggingPlains] could not find Difficulty {CfgDifficulty.Value}! ignoring");
                difficultyRuleDef = RuleCatalog.FindChoiceDef($"Difficulty.{CfgDifficulty.DefaultValue}");
                PreGameController.instance.ApplyChoice(difficultyRuleDef.globalIndex);
            }
        }

        private void InitConfigs()
        {
            CfgEnabled = Config.Bind(
            "mod_config",
            "enabled",
            true,
            "change to false if you want to disable the mod hooks and run the classic game"
            );
            CfgScenesConfigPath = Config.Bind(
            "mod_config",
            "scenes_config_path",
            "scenesConfig.json",
            "path to the scenes config file containing a json with all the scenes data (if it does not exists, it is created automatically)"
            );
            CfgSkipLobbyScreen = Config.Bind(
            "mod_config",
            "skip_lobby",
            true,
            "skip the character selection screen and head straight to the run"
            );
            CfgInitCmds = Config.Bind(
            "run_config",
            "initial_cmds",
            "no_enemies; kill_all 2;",
            "Commands to run at the beginning of the run (must be ConCommands concatenated with ';' as a seperator)"
            );
            CfgCharacter = Config.Bind(
            "run_config",
            "character",
            "Commando",
            "selected character to spawn; doesn't have to be a survivor :) (Commando,Huntress,Toolbot,Engi,Mage,Merc,Treebot,Loader,Croco,Captain or any character mod you'd like!)"
            );
            CfgSpawnWithDropPod = Config.Bind(
            "run_config",
            "drop_pod",
            false,
            "spawn from a falling drop pod or simply appearing on the scene"
            );
            CfgArtifactsList = Config.Bind(
            "run_config",
            "artifacts",
            "",
            "Artifacts to activate (must be the artifact asset names concatenated with ',' as a seperator) "
            );
            CfgDifficulty = Config.Bind(
            "run_config",
            "difficulty",
            "Normal",
            "Desired difficulty (Easy, Normal, Hard or any other difficulty you got)"
            );
            CfgStartingStage = Config.Bind(
            "run_config",
            "starting_stage",
            "golemplains",
            "Starting stage"
            );
            CfgFreezeTimer = Config.Bind(
            "run_config",
            "freeze_timer",
            true,
            "freeze the timer"
            );

            string jsonConfigPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CfgEnabled.ConfigFile.ConfigFilePath), CfgScenesConfigPath.Value);
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
                    UnityEngine.Debug.LogWarning("scene config file malformed! using defaults");
                    validJsonFile = false;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("scene config file not found! using defaults");
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

        private void PreGameRuleVoteController_ServerHandleClientVoteUpdate(On.RoR2.PreGameRuleVoteController.orig_ServerHandleClientVoteUpdate orig, UnityEngine.Networking.NetworkMessage netMsg)
        {
            orig(netMsg);
            foreach (NetworkUser networkUser in NetworkUser.readOnlyLocalPlayersList)
            {
                if (networkUser)
                {
                    networkUser.CallCmdSubmitVote(PreGameController.instance.gameObject, 0);
                }
                else
                {
                    UnityEngine.Debug.Log("Null network user in readonly local player list!");
                }
            }

        }

        private void GameNetworkManager_CCSetScene(On.RoR2.Networking.GameNetworkManager.orig_CCSetScene orig, ConCommandArgs args)
        {
            if (args[0].ToString() == "title")
            {
                RoR2.Console.instance.SubmitCmd(LocalUserManager.GetFirstLocalUser(), "transition_command gamemode ClassicRun; host 0;", false);
                return;
            }
            orig(args);
        }

        //private void PreGameController_Awake(On.RoR2.PreGameController.orig_Awake orig, PreGameController self)
        //{
        //
        //    orig(self);
        //    StackTrace st = new StackTrace();
        //    for (int i = 0; i < st.FrameCount; i++)
        //    {
        //        UnityEngine.Debug.Log(st.GetFrame(i).GetMethod().Name);
        //        UnityEngine.Debug.Log(st.GetFrame(i).GetMethod().ReflectedType.Name);
        //        UnityEngine.Debug.Log("+++++++++++++++++");
        //
        //
        //    }
        //}
    }

}