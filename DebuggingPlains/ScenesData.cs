using SimpleJSON;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;


namespace addOns
{
    public class ScenesData
    {
        private List<SceneData> scenesDataList = new List<SceneData>();
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
}
