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


        private static void AddMissingComponents(GameObject gameObject, SerializedGameObject serialized)
        {
            foreach(SerializedComponent serializedComponent in serialized.customComponents)
            {
                Type type = Type.GetType(serializedComponent.typeName);
                Type dataType = Type.GetType(serializedComponent.dataTypeName);

                SerializableComponent newComponent = (SerializableComponent)gameObject.AddComponent(type);
                Debug.Log(newComponent.name);

                object newData = JsonConvert.DeserializeObject(serializedComponent.data, dataType);

                newComponent.SetData(newData);
            }

            Transform t = gameObject.transform;
            for(int i = 0; i < t.childCount; i++)
            {
                AddMissingComponents(t.GetChild(i).gameObject, serialized.children[i]);
            }
        }


        public static GameObject LoadPrefab(string prefabName, AssetBundle bundle)
        {
            // Disable warnings about missing components
            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName.ToLower());

            if(!SerializedPrefabs.TryGetValue(prefabName.ToLower(), out SerializedGameObject serialized))
            {
                // No custom components on this prefab
                return prefab;
            }

            AddMissingComponents(prefab, serialized);
            return prefab;
        }
    }
}