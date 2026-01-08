using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UBUI.Serialization
{
    public static class PrefabSerializer
    {
        public static SerializedGameObject SerializePrefab(GameObject prefab, bool destroy)
        {
            SerializedGameObject newObject = new SerializedGameObject();

            List<SerializedComponent> customComponents = new List<SerializedComponent>();
            foreach(Component component in prefab.GetComponents<Component>())
            {
                SerializableComponent serializable = component as SerializableComponent;
                if(serializable == null)
                {
                    continue;
                }

                SerializedComponent newComponent = new SerializedComponent();
                newComponent.typeName = serializable.GetType().FullName;
                newComponent.dataTypeName = serializable.GetDataType().FullName;
                newComponent.data = JsonConvert.SerializeObject(serializable.GetData());

                customComponents.Add(newComponent);

                if(destroy)
                {
                    UnityEngine.Object.DestroyImmediate(component);
                }
                break;
            }
            newObject.customComponents = customComponents.ToArray();
            
            List<SerializedGameObject> children = new List<SerializedGameObject>();
            foreach(Transform t in prefab.transform)
            {
                children.Add(SerializePrefab(t.gameObject, destroy));
            }
            newObject.children = children.ToArray();

            return newObject;
        }
    }


    [Serializable]
    public class SerializedGameObject
    {
        public SerializedComponent[] customComponents;
        public SerializedGameObject[] children;
    }


    [Serializable]
    public class SerializedComponent
    {
        public string typeName;
        public string dataTypeName;
        public string data;
    }
}