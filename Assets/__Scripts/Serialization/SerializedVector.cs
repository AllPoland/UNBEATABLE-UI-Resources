using System;
using UnityEngine;

namespace UBUI.Serialization
{
    [Serializable]
    public class SerializedVector2
    {
        public float x;
        public float y;


        public static SerializedVector2 zero => new SerializedVector2(0f, 0f);


        public SerializedVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }


        public static implicit operator Vector2(SerializedVector2 s) => new Vector2(s.x, s.y);
    }
}