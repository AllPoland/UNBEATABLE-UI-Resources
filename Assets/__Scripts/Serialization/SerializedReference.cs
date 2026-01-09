using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace UBUI.Serialization
{
    [Serializable]
    public class SerializedReference<T> where T : MonoBehaviour
    {
        [JsonIgnore] public T Value;

        [HideInInspector] public string name = "";


        public bool FindValue(Transform owner)
        {
            if(Value)
            {
                // No need to search for the value again
                return true;
            }

            Transform transform = owner.Find(name);
            if(!transform)
            {
                // Recurse down the hierarchy until we find the object
                foreach(Transform child in owner)
                {
                    if(FindValue(child))
                    {
                        return true;
                    }
                }
                return false;
            }

            Value = transform.GetComponent<T>();
            return true;
        }


        public void UpdateName()
        {
            if(!Value)
            {
                name = "";
            }
            else name = Value.gameObject.name;
        }


        [OnSerializing]
        internal void PreSerialize(StreamingContext context)
        {
            UpdateName();
        }
    }
}