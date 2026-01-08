using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UBUI.Serialization
{
    public static class PrefabInitializer
    {
        public static Dictionary<string, SerializedGameObject> SerializedPrefabs = new Dictionary<string, SerializedGameObject>();


        private static void AddMissingComponents(GameObject gameObject, SerializedGameObject serialized)
        {
            foreach(SerializedComponent serializedComponent in serialized.customComponents)
            {
                Type type = Type.GetType(serializedComponent.typeName);
                Type dataType = Type.GetType(serializedComponent.dataTypeName);

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


        public static GameObject LoadPrefab(string prefabName, AssetBundle bundle)
        {
            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if(!SerializedPrefabs.TryGetValue(prefabName, out SerializedGameObject serialized))
            {
                // No custom components on this prefab
                return prefab;
            }

            AddMissingComponents(prefab, serialized);
            return prefab;
        }
    }
}