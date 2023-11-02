using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FrameworkDesign
{
    //序列化自定义对象列表
    [System.Serializable]
    public class SerializationList<T>
    {
        [SerializeField]
        List<T> list;

        public List<T> ToList => list;

        public SerializationList(List<T> list)
        {
            this.list = list;
        }
    }
}
