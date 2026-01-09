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


        public void FindValue(Transform owner)
        {
            Transform transform = owner.Find(name);
            if(!transform)
            {
                Value = null;
                return;
            }
            Value = transform.GetComponent<T>();
        }


        [OnSerializing]
        internal void PreSerialize(StreamingContext context)
        {
            if(!Value)
            {
                name = "";
            }
            else name = Value.gameObject.name;
        }
    }
}