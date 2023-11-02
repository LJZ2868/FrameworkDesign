using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FrameworkDesign
{
    //���л��Զ�������б�
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
