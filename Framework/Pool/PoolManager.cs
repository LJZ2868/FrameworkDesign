using System.Collections.Generic;
using UnityEngine;
namespace FrameworkDesign
{
    public class PoolDate : MonoBehaviour
    {
        public GameObject fatherObj;
        public List<GameObject> poolList;

        //第一个参数表示要存入缓存池的对象，第二个参数表示缓存池
        public PoolDate(GameObject obj, GameObject poolObj)
        {
            //给我们的抽屉 创建一个父对象 并且把他作为我们pool(衣柜)对象的子物体
            fatherObj = new GameObject(obj.name);
            fatherObj.transform.parent = poolObj.transform;

            //初始化缓存池的对象
            poolList = new List<GameObject>();
            //将缓存池的对象放入缓存池
            PushObj(obj);

        }

        public void PushObj(GameObject obj)
        {
            //存起来
            poolList.Add(obj);
            //存入时使其失效
            obj.SetActive(false);
            //设置父对象
            obj.transform.parent = fatherObj.transform;

        }

        public GameObject GetObj()
        {
            //取出第一个
            GameObject obj = poolList[0];
            poolList.RemoveAt(0);

            //设置父子关系
            obj.transform.parent = null;
            //激活 让其显示
            obj.SetActive(true);


            return obj;
        }
    }

    public class PoolManager : Singleton<PoolManager>
    {

        //衣柜和抽屉
        public Dictionary<string, PoolDate> poolDic = new Dictionary<string, PoolDate>();

        //缓存池
        private GameObject poolObj;


        //获取并实例化GameObject(父对象实例化子对象时)
        public GameObject GetObj(string name,GameObject @object)
        {
            GameObject gameObj;
            //有抽屉 并且抽屉里有东西
            if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
            {
                gameObj = poolDic[name].GetObj();
                gameObj.transform.position = @object.transform.position;
                gameObj.transform.rotation = @object.transform.rotation;
                gameObj.transform.localScale = @object.transform.lossyScale;
            }
            else
            {
                gameObj = ResManager.Instance.Load<GameObject>(name,obj =>
                {
                    obj.transform.SetPositionAndRotation(@object.transform.position, @object.transform.rotation);
                    obj.transform.localScale = @object.transform.lossyScale;
                    obj.name = name;
                    obj.SetActive(true);
                    return obj;
                });
                //gameObj = ResManager.Instance.Load<GameObject>(name);
                //gameObj.transform.SetPositionAndRotation(@object.transform.position, @object.transform.rotation);
                //gameObj.transform.localScale = @object.transform.lossyScale;
                //gameObj.name = name;
                //gameObj.SetActive(true);
            }
            return gameObj;
        }



        //实例化预制体
        public GameObject GetObj(string name, Vector3 vector)
        {
            GameObject gameObj = null;
            //有抽屉 并且抽屉里有东西
            if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
            {
                gameObj = poolDic[name].GetObj();
                gameObj.transform.position = vector;
            }
            else
            {
                ResManager.Instance.LoadAysnc<GameObject>(name, obj =>
                {
                    obj.transform.position = vector;
                    obj.name = name;
                    obj.SetActive(true);
                    gameObj = obj;
                });
                //gameObj = GameObject.Instantiate(Resources.Load<GameObject>(name),@object.transform.position,@object.transform.rotation);
                //gameObj.transform.localScale = @object.transform.lossyScale;
                //gameObj.name = name;
                //gameObj.SetActive(true);
            }
            return gameObj;
        }

        //获取GameObject
        public GameObject GetObj(string name)
        {
            GameObject gameObj;
            //有抽屉 并且抽屉里有东西
            if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
            {
                gameObj = poolDic[name].GetObj();
            }
            else
            {
                gameObj = Resources.Load<GameObject>(name);
                gameObj.name = name;
                gameObj.SetActive(true);
            }
            return gameObj;
        }

        public void PushObj(string name, GameObject obj)
        {
            //是否有缓存池
            if (poolObj == null)
            {
                poolObj = new GameObject("Pool");
            }


            //里面有抽屉
            if (poolDic.ContainsKey(name))
            {
                poolDic[name].PushObj(obj);
            }
            //里面没有抽屉
            else
            {
                poolDic.Add(name, new PoolDate(obj, poolObj));
            }
        }

        // 清空缓存池的方法 
        // 主要用在 场景切换时
        public void PoolClear()
        {

            poolDic.Clear();
            poolObj = null;

        }



    }
}