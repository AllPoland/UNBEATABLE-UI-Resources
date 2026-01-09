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

            List<SerializedComponent> components = new List<SerializedComponent>();
            foreach(Component component in prefab.GetComponents<Component>())
            {
                SerializableComponent serializable = component as SerializableComponent;
                if(!serializable)
                {
                    continue;
                }

                SerializedComponent newComponent = new SerializedComponent();
                newComponent.type = serializable.GetType().FullName;
                newComponent.dataType = serializable.GetDataType().FullName;
                newComponent.data = JsonConvert.SerializeObject(serializable.GetData());

                components.Add(newComponent);

                if(destroy)
                {
                    Object.DestroyImmediate(component);
                }
                break;
            }
            newObject.components = components.ToArray();
            
            List<SerializedGameObject> children = new List<SerializedGameObject>();
            foreach(Transform t in prefab.transform)
            {
                children.Add(SerializePrefab(t.gameObject, destroy));
            }
            newObject.children = children.ToArray();

            return newObject;
        }
    }
}