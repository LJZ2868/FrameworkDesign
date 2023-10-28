using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace FrameworkDesign
{
    public class ABManager : MonoSingleton<ABManager>
    {
        //��������
        private AssetBundle mainAB;
        //���ص������������ļ�
        private AssetBundleManifest manifest;


        //�洢�Ѽ��ص�ab��
        private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
        //����·��
        private string Path
        {
            get
            {
                return Application.streamingAssetsPath;
            }
        }
        //���ص�����·��,������
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
        //���������������ļ�
        private void LoadMainAssetBundle()
        {
            if (mainAB != null)
                return;
            mainAB = CheckAB(MainABName);
            //�����ļ�
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
        /// ����ָ������������
        /// </summary>
        /// <param name="bundleName">����</param>
        private void LoadDependencies(string bundleName)
        {
            LoadMainAssetBundle();
            //��ȡ���������Ϣ
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
                //�ж��Ƿ��Լ����ع�
                if (abDic.ContainsKey(dependency))
                    continue;
                var assetBundle = CheckAB(dependency);
                abDic.Add(dependency, assetBundle);
            }
        }

        /// <summary>
        /// ͬ��������Դ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName">����</param>
        /// <param name="assetName">��Դ��</param>
        /// <returns>��Դ����</returns>
        public T Load<T>(string bundleName, string assetName) where T : Object
        {
            //�ȼ���������
            LoadDependencies(bundleName);
            //������Դ��
            if (!abDic.ContainsKey(bundleName))
            {
                var assetBundle = CheckAB(bundleName);
                if (assetBundle == null)
                {
                    Debug.Log("��Դ����ʧ��");
                    return null;
                }
                abDic.Add(bundleName, assetBundle);
            }
            //����ָ������Դ
            var obj = abDic[bundleName].LoadAsset<T>(assetName);
            return obj is GameObject ? Instantiate(obj) : obj;
        }

        //Type�ͼ���
        public Object Load(string bundleName, string assetName, System.Type type)
        {
            LoadDependencies(bundleName);
            if (!abDic.ContainsKey(bundleName))
            {
                var assetBundle = CheckAB(bundleName);
                if (assetBundle == null)
                {
                    Debug.Log("��Դ����ʧ��");
                    return null;
                }
                abDic.Add(bundleName, assetBundle);
            }
            //����ָ������Դ
            var obj = abDic[bundleName].LoadAsset(assetName, type);
            return obj is GameObject ? Instantiate(obj) : obj;
        }


        /// <summary>
        /// �첽����AB����Դ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName">����</param>
        /// <param name="assetName">��Դ��</param>
        /// <returns></returns>
        public async Task<T> LoadAsync<T>(string bundleName, string assetName) where T : Object
        {
            //�ȼ���������
            LoadDependencies(bundleName);
            if (!abDic.ContainsKey(bundleName))
            {
                var assetBundle = await LoadFromFileAsync(bundleName);
                if (assetBundle == null)
                {
                    Debug.Log("��Դʧ��");
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

        //ж��ָ����
        public void Unload(string assetBundle)
        {
            if (abDic.ContainsKey(assetBundle))
            {
                abDic[assetBundle].Unload(false);
                abDic.Remove(assetBundle);
            }
        }

        //ж�����а�
        public void ClearAB()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            abDic.Clear();
            manifest = null;
            mainAB = null;

        }
    }
}
