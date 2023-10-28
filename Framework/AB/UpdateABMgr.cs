using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FrameworkDesign
{
    public class UpdateABMgr : MonoBehaviour
    {
        readonly List<ABInfo> ABInfos = new List<ABInfo>();

        private static UpdateABMgr instance;
        public static UpdateABMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject(nameof(UpdateABMgr));
                    instance = obj.AddComponent<UpdateABMgr>();
                }
                return instance;
            }
        }


        string FTPPath = "ftp://192.168.84.10/AB";
        string username = "FTP Upload";
        string password = "123456";

#if UNITY_STANDALONE_WIN
        string targetPlatform = "/PC";
#elif UNITY_UNITY_ANDROID
        string targetPlatform = "/Android";
#elif UNITY_IOS
        string targetPlatform = "/IOS";
#endif

        public void CheckUpdate(Action<bool> cIsSucceed,Action<bool> cIsNeed,Action<string> schedule)
        {
            UpdateABCompareFile(isSucceed =>
            {
                if (isSucceed)
                {
                    UpdateABFile(isSucceed =>
                    {
                        cIsSucceed(isSucceed);
                    },isNeed => 
                    {
                        cIsNeed(isNeed);
                    },Count =>
                    {
                        schedule(Count);
                    });
                }
            });
        }

        //下载AB包资源对比文件 callBack返回 是否下载成功 是否需要更新
        private void UpdateABCompareFile(Action<bool> callBack)
        {
            //没有AB包资源文件
            if (!File.Exists(Application.persistentDataPath + targetPlatform + "/ABCompare.date"))
            {
                //下载AB包对比文件到指定路径下
                callBack(DownLoadFile("ABCompare.date", Application.persistentDataPath+ targetPlatform));
            }
            else
            {
                //获取服务器AB包资源文件 和 本地AB包资源文件 进行对比 看是否需要更新

                var request = GetFTP(FTPPath + targetPlatform + "/ABCompare.date", username, password);

                //FTP下载流对象
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream DownloadStream = response.GetResponseStream();
                if (GetMD5(DownloadStream) != GetMD5(Application.persistentDataPath + targetPlatform + "/ABCompare.date"))
                {
                    callBack(DownLoadFile("ABCompare.date", Application.persistentDataPath + targetPlatform ));
                }
                else
                    callBack(true);
            }
            
        }

        //下载AB包资源对比文件中的AB包资源
        private async void UpdateABFile(Action<bool> isSucceed,Action<bool> isNeed ,Action<string> action)
        {
            //获取对比文件的信息
            var path = Application.persistentDataPath + targetPlatform + "/ABCompare.date";
            var json = File.ReadAllText(path);
            var date = JsonUtility.FromJson<Serialization<ABInfo>>(json);
            var dateList = date.ToList;

            //以下载列表缓存
            List<ABInfo> tempList = new List<ABInfo>();
            //要下载的AB包
            Dictionary<string, ABInfo> DownloadAB = new Dictionary<string, ABInfo>();
            var geDic = DownloadAB.GetEnumerator();
            var geList = dateList.GetEnumerator();

            while(geList.MoveNext())
            {
                DownloadAB.Add(geList.Current.name, geList.Current);
            }

#if UNITY_ANDROID || UNITY_IOS
            //默认AB包资源文件夹(只读)
            var localPath1 = Application.streamingAssetsPath + targetPlatform;
            if(Directory.Exists(localPath1))
            {
                DirectoryInfo directoryInfo1 = new DirectoryInfo(localPath1);
                var fileInfo1 = directoryInfo1.GetFiles();
                //检测要下载(更新)的默认AB包(只读) 要删除的AB包
                if (fileInfo1.Length != 0)
                {
                    //检测默认资源下载列表
                    foreach (var item in fileInfo1)
                    {
                        //包名相同且md5码相同
                        if (DownloadAB.ContainsKey(item.Name))
                        {
                            //不用更新的AB包
                            DownloadAB.Remove(item.Name);
                        }
                    }
                }
            }

            //更新后新的AB包存放文件夹
            var localPath2 = Application.persistentDataPath + targetPlatform;
            DirectoryInfo directoryInfo2 = new DirectoryInfo(localPath2);
            var fileInfo2 = directoryInfo2.GetFiles();

            //检测要剩下的AB包 要删除的AB包
            CheckUpdateAB(fileInfo2, DownloadAB);

            await Task.Run(() => 
            {
                UpdateAB(DownloadAB, tempList, localPath2, action);
            });
#else
            //默认AB包资源文件夹
            var localPath = Application.streamingAssetsPath + targetPlatform ;
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(localPath);
            var fileInfo = directoryInfo.GetFiles();

            //检测要下载(更新)的AB包 要删除的AB包
            CheckUpdateAB(fileInfo, DownloadAB);

            if (DownloadAB.Count == 0)
                isNeed(false);
            else
                isNeed(true);
            //Debug.Log(DownloadAB.Count);
            await Task.Run(() =>
            {
                UpdateAB(DownloadAB, tempList, localPath, action);
            });
#endif
            isSucceed(DownloadAB.Count == 0);
        }

        /// <summary>
        /// 检测要下载的新的AB包，删除不用的AB包
        /// </summary>
        /// <param name="fileInfos">AB包文件夹</param>
        /// <param name="DownloadAB">要下载的AB包存储字典</param>
        void CheckUpdateAB(FileInfo[] fileInfos,Dictionary<string,ABInfo> DownloadAB)
        {
            if (fileInfos.Length != 0)
            {
                foreach (var item in fileInfos)
                {
                    if (DownloadAB.ContainsKey(item.Name))
                    {
                        if (DownloadAB[item.Name].md5 == GetMD5(item.FullName))
                            DownloadAB.Remove(item.Name);
                        else
                            item.Delete();
                    }
                }
            }
        }
        /// <summary>
        /// 下载AB包(非默认资源文件夹)
        /// </summary>
        /// <param name="DownloadAB">下载列表字典</param>
        /// <param name="tempList">以下载的记录</param>
        /// <param name="localPath">下载到的路径</param>
        /// <param name="action">返回下载进度的委托</param>
        async void UpdateAB(Dictionary<string, ABInfo> DownloadAB, List<ABInfo> tempList, string localPath,Action<string> action)
        {
            //下载成功次数
            var succeedDownloadCount = 0;
            //重新下载次数
            var reDownloadCount = 0;
            //需要下载次数
            var maxDownloadCount = DownloadAB.Count;

            var geDic = DownloadAB.GetEnumerator();
            //下载
            while (DownloadAB.Count != 0 || reDownloadCount == 5)
            {
                while (geDic.MoveNext())
                {
                    await Task.Run(() => 
                    {
                        //一个AB包下载成功
                        if (DownLoadFile(geDic.Current.Key, localPath))
                        {
                            //下载成功记录
                            tempList.Add(geDic.Current.Value);
                        }
                    });
                    //Debug.Log("下载进度:" + succeedDownloadCount++ + "/" + maxDownloadCount);
                    //返回下载进度
                    action(++succeedDownloadCount + "/" + maxDownloadCount);
                }
                //移除下载成功的资源
                foreach (var item in tempList)
                {
                    DownloadAB.Remove(item.name);
                }
                reDownloadCount++;
            }
        }

        /// <summary>
        /// 连接FTP服务器
        /// </summary>
        /// <param name="requestUriString">资源路径(单个资源)</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        FtpWebRequest GetFTP(string requestUriString,string username,string password)
        {
            //创建一个FTP连接 用于下载
            FtpWebRequest request = WebRequest.Create(requestUriString) as FtpWebRequest;
            //设置一个通讯凭证(用户名，密码)
            NetworkCredential credential = new NetworkCredential(username,password);
            request.Credentials = credential;
            //设置代理为null
            request.Proxy = null;
            //请求结束后关闭控制连接
            request.KeepAlive = false;
            //指定传输类型为2进制
            request.UseBinary = true;
            return request;
        }
        /// <summary>
        /// 从FTP服务器下载资源
        /// </summary>
        /// <param name="fileName">下载的文件名</param>
        /// <param name="filePath">下载的路径</param>
        bool DownLoadFile(string fileName, string filePath)
        {
            try
            {
#if UNITY_STANDALONE_WIN
                var target = "/PC/";
#elif UNITY_UNITY_ANDROID
                var target = "/Android/";
#elif UNITY_IOS
                var target = "/IOS/";
#endif
                var request = GetFTP(FTPPath + target + fileName, username, password);
                //设置操作命令为下载
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                //FTP下载流对象
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream DownloadStream = response.GetResponseStream();

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                //读取文件信息，写入FTP流
                using (FileStream fileStream = File.Create(filePath + "/" + fileName))
                {
                    //每次下载2048B
                    var bytes = new byte[2048];
                    //读取字节数
                    int contentLength = DownloadStream.Read(bytes, 0, bytes.Length);
                    while (contentLength != 0)
                    {
                        //写入流的字节数
                        fileStream.Write(bytes, 0, contentLength);
                        contentLength = DownloadStream.Read(bytes, 0, bytes.Length);
                    }
                    fileStream.Close();
                    DownloadStream.Close();
                }
                Debug.Log(fileName + "下载成功");
                Debug.Log(filePath+"/"+fileName);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("下载失败" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 根据文件路径获取md5码
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetMD5(string filePath)
        {
            //将文件以流的形式打开
            FileStream fileStream = new FileStream(filePath, FileMode.Open);

            //声明一个md5对象，用于生成md5码
            MD5 md5 = new MD5CryptoServiceProvider();
            //计算md5码
            var md5Res = md5.ComputeHash(fileStream);
            fileStream.Close();

            //将16位字节转换位16进制拼接成字符串
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in md5Res)
            {
                stringBuilder.Append(item.ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 根据文件流获取md5码
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public string GetMD5(Stream fileStream)
        {
            //将文件以流的形式打开
            //FileStream fileStream = new FileStream(filePath, FileMode.Open);
            //声明一个md5对象，用于生成md5码
            MD5 md5 = new MD5CryptoServiceProvider();
            //计算md5码
            var md5Res = md5.ComputeHash(fileStream);
            fileStream.Close();

            //将16位字节转换位16进制拼接成字符串
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in md5Res)
            {
                stringBuilder.Append(item.ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        private void OnDestroy()
        {
            instance = null;
            Destroy(gameObject);
        }
    }
    //ab包资源信息
    [Serializable]
    public class ABInfo
    {
        //包名
        public string name;
        //大小
        public long size;
        //md5码
        public string md5;

        public ABInfo(string name, long size, string md5)
        {
            this.name = name;
            this.size = size;
            this.md5 = md5;
        }
    }
}
