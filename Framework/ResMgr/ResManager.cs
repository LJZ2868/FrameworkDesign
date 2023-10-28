using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FrameworkDesign
{
    public class ResManager : MonoSingleton<ResManager>
    {
        //同步加载资源
        public T Load<T>(string resName, System.Func<T,T> callback) where T : Object
        {
            T resObj = Resources.Load<T>(resName);
            if (resObj is GameObject)
            {
                return callback(GameObject.Instantiate(resObj));
            }
            else
                return resObj;
        }

        public T Load<T>(string resName) where T : Object
        {
            T resObj = Resources.Load<T>(resName);
            if (resObj is GameObject)
            {
                return GameObject.Instantiate(resObj);
            }
            else
                return resObj;
        }

        //异步加载资源
        public void LoadAysnc<T>(string resName, UnityAction<T> callback) where T : Object
        {
            //开启异步加载的协程
            StartCoroutine(LoadAsyncCor(resName, callback));
        }

        //真正的协同程序函数  用于 开启异步加载对应的资源
        IEnumerator LoadAsyncCor<T>(string resName, UnityAction<T> callback) where T : Object
        {

            ResourceRequest R = Resources.LoadAsync<T>(resName);
            yield return R;

            if (R.asset is GameObject)
            {
                var obj = GameObject.Instantiate(R.asset);
                obj.name = R.asset.name;
                callback(obj as T);
            }
            else
                callback(R.asset as T);
        }
    }
}