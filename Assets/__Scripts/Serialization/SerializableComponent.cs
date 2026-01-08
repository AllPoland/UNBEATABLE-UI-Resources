using System;
using UnityEngine;

namespace UBUI.Serialization
{
    public abstract class SerializableComponent : MonoBehaviour
    {
        public abstract object GetData();


        public abstract void SetData(object data);


        public abstract Type GetDataType();
    }

    
    public abstract class SerializableComponent<T> : SerializableComponent
    {
        public T Data;


        public override object GetData()
        {
            return Data;
        }


        public override Type GetDataType()
        {
            return typeof(T);
        }
    }
}