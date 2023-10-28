using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FrameworkDesign
{
    public class ABTest : MonoBehaviour
    {
        private void Awake()
        {
            UpdateABMgr.Instance.CheckUpdate(async isSucceed =>
            {
                if (isSucceed)
                {
                    Debug.Log("�������");
                    ABManager.Instance.Load<GameObject>("test", "Cube");
                    await ABManager.Instance.LoadAsync<GameObject>("test", "Sphere");
                    ABManager.Instance.Load<GameObject>("test", "Capsule");

                }
                else
                {
                    Debug.Log("����ʧ�ܣ���������");
                }
            }, isNeed =>
            {
                if (isNeed)
                    Debug.Log("��Ҫ����");
                else
                    Debug.Log("����Ҫ����");
            }, schedule =>
            {
                Debug.Log("���ؽ���:" + schedule);
            });
        }


        private void Start()
        {
            
        }


        private void Update()
        {

        }
    }
}
