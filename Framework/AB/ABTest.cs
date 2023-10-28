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
                    Debug.Log("更新完成");
                    ABManager.Instance.Load<GameObject>("test", "Cube");
                    await ABManager.Instance.LoadAsync<GameObject>("test", "Sphere");
                    ABManager.Instance.Load<GameObject>("test", "Capsule");

                }
                else
                {
                    Debug.Log("更新失败，请检测网络");
                }
            }, isNeed =>
            {
                if (isNeed)
                    Debug.Log("需要更新");
                else
                    Debug.Log("不需要更新");
            }, schedule =>
            {
                Debug.Log("下载进度:" + schedule);
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
