using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FrameworkDesign
{
    //���л��Զ�������б�
    [System.Serializable]
    public class Serialization<T>
    {
        [SerializeField]
        List<T> list;

        public List<T> ToList => list;

        public Serialization(List<T> list)
        {
            this.list = list;
        }
    }
}
