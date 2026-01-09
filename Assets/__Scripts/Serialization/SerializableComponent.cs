using System;
using UnityEngine;

namespace UBUI.Serialization
{
    // This class needs to exist because for some reason you can't do 'object as T' when T doesn't have a constraint
    public abstract class SerializableData { }


    public abstract class SerializableComponent : MonoBehaviour
    {
        public abstract object GetData();


        public abstract void SetData(object data);


        public abstract Type GetDataType();
    }

    
    public abstract class SerializableComponent<T> : SerializableComponent where T : SerializableData
    {
        public T Data;


        public override object GetData()
        {
            return Data;
        }


        public override void SetData(object data)
        {
            if(data is T serializable)
            {
                Data = serializable;
            }
        }


        public override Type GetDataType()
        {
            return typeof(T);
        }
    }
}