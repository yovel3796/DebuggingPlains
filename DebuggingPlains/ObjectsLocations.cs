using DebugToolkit;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace addOns
{
    public class ObjectsLocations
    {
        private List<ObjectToSpawn> objectsList;

        public ObjectsLocations(List<ObjectToSpawn> objectsList) 
        {
            this.objectsList = objectsList;
        }

        public ObjectsLocations() { this.objectsList = new List<ObjectToSpawn>(); }

        public List<ObjectToSpawn> GetObjectsList() => objectsList;
        public void SetObjectsList(List<ObjectToSpawn> objectsList) => this.objectsList = objectsList;

        public void Add(string objName, Vector3 position, Quaternion rotation) => objectsList.Add(new ObjectToSpawn(objName, position, rotation));
        public void Add(ObjectToSpawn objectToSpawn) => objectsList.Add(objectToSpawn);

    }
    public class ObjectToSpawn
    {
        public string objName;
        public Vector3 position;
        public Quaternion rotation;
        public ObjectToSpawn(string objName, Vector3 position, Quaternion rotation)
        {
            this.objName = objName;
            this.position = position;
            this.rotation = rotation;

        }
    }
}
