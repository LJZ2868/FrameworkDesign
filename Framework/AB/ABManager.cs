using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace FrameworkDesign
{
    public class ABManager : MonoSingleton<ABManager>
    {
        //加载主包
        private AssetBundle mainAB;
        //加载的主包的配置文件
        private AssetBundleManifest manifest;


        //存储已加载的ab包
        private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
        //加载路径
        private string Path
        {
            get
            {
                return Application.streamingAssetsPath;
            }
        }
        //加载的主包路径,主包名
        private string TargetPlatform
        {
            get
            {
#if UNITY_IOS
            return "/IOS/";
#elif UNITY_ANDROID
            return "/Android/";
#else
                return "/PC/";
            }
        }
#endif

        private string MainABName
        {
            get
            {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
                return "PC";
            }
#endif
        }
        //加载主包和配置文件
        private void LoadMainAssetBundle()
        {
            if (mainAB != null)
                return;
            mainAB = CheckAB(MainABName);
            //配置文件
            manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        AssetBundle CheckAB(string assetBundleName)
        {
            if (File.Exists(Path + TargetPlatform + assetBundleName))
                return AssetBundle.LoadFromFile(Path + TargetPlatform + assetBundleName);
            else
                return AssetBundle.LoadFromFile(Application.persistentDataPath + TargetPlatform + assetBundleName);
        }
        AssetBundleCreateRequest AsyncCheckaAB(string assetBundleName)
        {
            if (File.Exists(Path + TargetPlatform + assetBundleName))
                return AssetBundle.LoadFromFileAsync(Path + TargetPlatform + assetBundleName);
            else
                return AssetBundle.LoadFromFileAsync(Application.persistentDataPath + TargetPlatform + assetBundleName);
        }

        /// <summary>
        /// 加载指定包的依赖包
        /// </summary>
        /// <param name="bundleName">包名</param>
        private void LoadDependencies(string bundleName)
        {
            LoadMainAssetBundle();
            //获取依赖相关信息
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            //var ge = dependencies.GetEnumerator();
            //while (ge.MoveNext())
            //{
            //    if (abDic.ContainsKey(ge.Current.GetType().Name))
            //    { 
            //    }
            //}
            foreach (var dependency in dependencies)
            {
                //判断是否以及加载过
                if (abDic.ContainsKey(dependency))
                    continue;
                var assetBundle = CheckAB(dependency);
                abDic.Add(dependency, assetBundle);
            }
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName">包名</param>
        /// <param name="assetName">资源名</param>
        /// <returns>资源类型</returns>
        public T Load<T>(string bundleName, string assetName) where T : Object
        {
            //先加载依赖包
            LoadDependencies(bundleName);
            //加载资源包
            if (!abDic.ContainsKey(bundleName))
            {
                var assetBundle = CheckAB(bundleName);
                if (assetBundle == null)
                {
                    Debug.Log("资源加载失败");
                    return null;
                }
                abDic.Add(bundleName, assetBundle);
            }
            //加载指定的资源
            var obj = abDic[bundleName].LoadAsset<T>(assetName);
            return obj is GameObject ? Instantiate(obj) : obj;
        }

        //Type型加载
        public Object Load(string bundleName, string assetName, System.Type type)
        {
            LoadDependencies(bundleName);
            if (!abDic.ContainsKey(bundleName))
            {
                var assetBundle = CheckAB(bundleName);
                if (assetBundle == null)
                {
                    Debug.Log("资源加载失败");
                    return null;
                }
                abDic.Add(bundleName, assetBundle);
            }
            //加载指定的资源
            var obj = abDic[bundleName].LoadAsset(assetName, type);
            return obj is GameObject ? Instantiate(obj) : obj;
        }


        /// <summary>
        /// 异步加载AB包资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName">包名</param>
        /// <param name="assetName">资源名</param>
        /// <returns></returns>
        public async Task<T> LoadAsync<T>(string bundleName, string assetName) where T : Object
        {
            //先加载依赖包
            LoadDependencies(bundleName);
            if (!abDic.ContainsKey(bundleName))
            {
                var assetBundle = await LoadFromFileAsync(bundleName);
                if (assetBundle == null)
                {
                    Debug.Log("资源失败");
                    return null;
                }
                abDic.Add(bundleName, assetBundle);
            }
            await Task.Yield();
            var request = abDic[bundleName].LoadAssetAsync<T>(assetName);
            if (request.asset == null) return null;
            return request.asset is GameObject ? (T)Instantiate(request.asset) : (T)request.asset;

        }

        private async Task<AssetBundle> LoadFromFileAsync(string assetBundleName)
        {
            var request = AsyncCheckaAB(assetBundleName);
            await Task.Yield();
            return request.assetBundle;
        }

        //卸载指定包
        public void Unload(string assetBundle)
        {
            if (abDic.ContainsKey(assetBundle))
            {
                abDic[assetBundle].Unload(false);
                abDic.Remove(assetBundle);
            }
        }

        //卸载所有包
        public void ClearAB()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            abDic.Clear();
            manifest = null;
            mainAB = null;

        }
    }
}
