using SimpleJSON;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;


namespace DebugDiff
{
    class ScenesData
    {
        public class SceneData
        {
            public string sceneName;
            public Vector3 playerSpawnPosition;
            public Quaternion playerSpawnRotation;
            public ObjectsLocations objectsList;
            public SceneData(string sceneName, Vector3 playerSpawnPosition, Quaternion playerSpawnRotation, ObjectsLocations objectsList) 
            {
                this.sceneName = sceneName;
                this.playerSpawnPosition = playerSpawnPosition;
                this.playerSpawnRotation = playerSpawnRotation;
                this.objectsList = objectsList;
            }

            public SceneData() { }
        }
        private List<SceneData> scenesDataList = new List<SceneData>();
        public static List<SceneData> scenesDataListOld = new List<SceneData>()
        {
            new SceneData(
                "golemplains",
                new Vector3(322.5316f, -51.5807f, -184.9648f),
                new Quaternion(0, 0.6152443f, 0, 0.4883366f),
                new ObjectsLocations(new List<ObjectsLocations.ObjectToSpawn> {
                    new ObjectsLocations.ObjectToSpawn(
                        "SpawnCards/InteractableSpawnCard/iscChest1",
                        new Vector3(323.2392f, -52.29929f, -164.9908f),
                        Quaternion.Euler(0, 230, 0)
                        ),
                    new ObjectsLocations.ObjectToSpawn(
                        "SpawnCards/InteractableSpawnCard/iscChest2",
                        new Vector3(325.264f, -52.47514f, -167.6548f),
                        Quaternion.Euler(0, 230, 0)
                        ),
                    new ObjectsLocations.ObjectToSpawn(
                        "SpawnCards/InteractableSpawnCard/iscGoldChest",
                        new Vector3(328.2358f, -51.72319f, -170.4923f),
                        Quaternion.Euler(0, 230, 0)
                        ),
                    new ObjectsLocations.ObjectToSpawn(
                        "SpawnCards/InteractableSpawnCard/iscShrineBlood",
                        new Vector3(331.5801f, -52.88429f, -173.4541f),
                        Quaternion.Euler(0, 235, 0)
                        ),
                    new ObjectsLocations.ObjectToSpawn(
                        "SpawnCards/InteractableSpawnCard/iscShrineChance",
                        new Vector3(334.6402f, -52.33547f, -177.1784f),
                        Quaternion.Euler(0, 240, 0)
                        ),
                    new ObjectsLocations.ObjectToSpawn(
                        "SpawnCards/InteractableSpawnCard/iscShrineCombat",
                        new Vector3(337.9944f, -52.34299f, -182.7293f),
                        Quaternion.Euler(0, 250, 0)
                        ),
                    new ObjectsLocations.ObjectToSpawn(
                        "SpawnCards/InteractableSpawnCard/iscShrineBoss",
                        new Vector3(339.7971f, -52.31176f, -187.5125f),
                        Quaternion.Euler(0, 116, 0)
                        ),
                })
            )
        };

        public SceneData GetSceneDataBySceneName(string sceneName)
        {
            foreach (SceneData scene in this.scenesDataList)
            {
                if (scene.sceneName == sceneName)
                {
                    return scene;
                }
            }
            return null;
        }

        public void AddSceneData(SceneData scene) => this.scenesDataList.Add(scene);

        public ScenesData() { }

        public ScenesData(JSONNode jsonBuffer) 
        {
            foreach (JSONNode sceneNode in jsonBuffer["scenesData"].AsArray)
            {
                SceneData sceneData = new SceneData();

                sceneData.sceneName = sceneNode["sceneName"];
                sceneData.playerSpawnPosition = new Vector3(sceneNode["playerSpawnPosition"]["x"].AsFloat, sceneNode["playerSpawnPosition"]["y"].AsFloat, sceneNode["playerSpawnPosition"]["z"].AsFloat);
                sceneData.playerSpawnRotation = new Quaternion(sceneNode["playerSpawnPosition"]["x"].AsFloat, sceneNode["playerSpawnPosition"]["y"].AsFloat, sceneNode["playerSpawnPosition"]["z"].AsFloat, sceneNode["playerSpawnPosition"]["w"].AsFloat);
                sceneData.objectsList = new ObjectsLocations();
                foreach (JSONNode objectNode in sceneNode["objectsLocations"].AsArray)
                {
                    sceneData.objectsList.Add(
                        objectNode["objName"],
                        new Vector3(objectNode["position"]["x"].AsFloat, objectNode["position"]["y"].AsFloat, objectNode["position"]["z"].AsFloat), 
                        Quaternion.Euler(objectNode["rotation"]["x"].AsFloat, objectNode["rotation"]["y"].AsFloat, objectNode["rotation"]["z"].AsFloat)
                        );
                }
                scenesDataList.Add(sceneData);
            }
        }
    }
}
