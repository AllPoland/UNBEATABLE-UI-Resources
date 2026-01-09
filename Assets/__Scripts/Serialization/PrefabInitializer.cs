using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace UBUI.Serialization
{
    public static class PrefabInitializer
    {
        private static Dictionary<string, SerializedGameObject> SerializedPrefabs = new Dictionary<string, SerializedGameObject>();


        public static void AddComponentManifest(string bundlePath)
        {
            string manifestPath = bundlePath + "-components.json";
            string json = File.ReadAllText(manifestPath);

            Dictionary<string, SerializedGameObject> newPrefabs = JsonConvert.DeserializeObject<Dictionary<string, SerializedGameObject>>(json);
            foreach(KeyValuePair<string, SerializedGameObject> pair in newPrefabs)
            {
                SerializedPrefabs[pair.Key] = pair.Value;
            }
        }


        public static void ClearManifest()
        {
            SerializedPrefabs.Clear();
        }


        public static void AddMissingComponents(GameObject gameObject, SerializedGameObject serialized)
        {
            foreach(SerializedComponent serializedComponent in serialized.components)
            {
                Type type = Type.GetType(serializedComponent.type);
                Type dataType = Type.GetType(serializedComponent.dataType);

                SerializableComponent newComponent = (SerializableComponent)gameObject.AddComponent(type);

                object newData = JsonConvert.DeserializeObject(serializedComponent.data, dataType);
                newComponent.SetData(newData);
            }

            Transform t = gameObject.transform;
            for(int i = 0; i < t.childCount; i++)
            {
                AddMissingComponents(t.GetChild(i).gameObject, serialized.children[i]);
            }
        }


        public static GameObject LoadAndInstantiatePrefab(string prefabName, AssetBundle bundle, Transform parent)
        {
            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName.ToLower());

            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent, false);
            if(!SerializedPrefabs.TryGetValue(prefabName.ToLower(), out SerializedGameObject serialized))
            {
                // No custom components on this prefab
                return instance;
            }

            AddMissingComponents(instance, serialized);
            return instance;
        }
    }


    [Serializable]
    public class SerializedGameObject
    {
        public SerializedComponent[] components;
        public SerializedGameObject[] children;
    }


    [Serializable]
    public class SerializedComponent
    {
        public string type;
        public string dataType;
        public string data;
    }
}