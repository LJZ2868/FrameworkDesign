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

        //����AB����Դ�Ա��ļ� callBack���� �Ƿ����سɹ� �Ƿ���Ҫ����
        private void UpdateABCompareFile(Action<bool> callBack)
        {
            //û��AB����Դ�ļ�
            if (!File.Exists(Application.persistentDataPath + targetPlatform + "/ABCompare.date"))
            {
                //����AB���Ա��ļ���ָ��·����
                callBack(DownLoadFile("ABCompare.date", Application.persistentDataPath+ targetPlatform));
            }
            else
            {
                //��ȡ������AB����Դ�ļ� �� ����AB����Դ�ļ� ���жԱ� ���Ƿ���Ҫ����

                var request = GetFTP(FTPPath + targetPlatform + "/ABCompare.date", username, password);

                //FTP����������
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

        //����AB����Դ�Ա��ļ��е�AB����Դ
        private async void UpdateABFile(Action<bool> isSucceed,Action<bool> isNeed ,Action<string> action)
        {
            //��ȡ�Ա��ļ�����Ϣ
            var path = Application.persistentDataPath + targetPlatform + "/ABCompare.date";
            var json = File.ReadAllText(path);
            var date = JsonUtility.FromJson<Serialization<ABInfo>>(json);
            var dateList = date.ToList;

            //�������б���
            List<ABInfo> tempList = new List<ABInfo>();
            //Ҫ���ص�AB��
            Dictionary<string, ABInfo> DownloadAB = new Dictionary<string, ABInfo>();
            var geDic = DownloadAB.GetEnumerator();
            var geList = dateList.GetEnumerator();

            while(geList.MoveNext())
            {
                DownloadAB.Add(geList.Current.name, geList.Current);
            }

#if UNITY_ANDROID || UNITY_IOS
            //Ĭ��AB����Դ�ļ���(ֻ��)
            var localPath1 = Application.streamingAssetsPath + targetPlatform;
            if(Directory.Exists(localPath1))
            {
                DirectoryInfo directoryInfo1 = new DirectoryInfo(localPath1);
                var fileInfo1 = directoryInfo1.GetFiles();
                //���Ҫ����(����)��Ĭ��AB��(ֻ��) Ҫɾ����AB��
                if (fileInfo1.Length != 0)
                {
                    //���Ĭ����Դ�����б�
                    foreach (var item in fileInfo1)
                    {
                        //������ͬ��md5����ͬ
                        if (DownloadAB.ContainsKey(item.Name))
                        {
                            //���ø��µ�AB��
                            DownloadAB.Remove(item.Name);
                        }
                    }
                }
            }

            //���º��µ�AB������ļ���
            var localPath2 = Application.persistentDataPath + targetPlatform;
            DirectoryInfo directoryInfo2 = new DirectoryInfo(localPath2);
            var fileInfo2 = directoryInfo2.GetFiles();

            //���Ҫʣ�µ�AB�� Ҫɾ����AB��
            CheckUpdateAB(fileInfo2, DownloadAB);

            await Task.Run(() => 
            {
                UpdateAB(DownloadAB, tempList, localPath2, action);
            });
#else
            //Ĭ��AB����Դ�ļ���
            var localPath = Application.streamingAssetsPath + targetPlatform ;
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(localPath);
            var fileInfo = directoryInfo.GetFiles();

            //���Ҫ����(����)��AB�� Ҫɾ����AB��
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
        /// ���Ҫ���ص��µ�AB����ɾ�����õ�AB��
        /// </summary>
        /// <param name="fileInfos">AB���ļ���</param>
        /// <param name="DownloadAB">Ҫ���ص�AB���洢�ֵ�</param>
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
        /// ����AB��(��Ĭ����Դ�ļ���)
        /// </summary>
        /// <param name="DownloadAB">�����б��ֵ�</param>
        /// <param name="tempList">�����صļ�¼</param>
        /// <param name="localPath">���ص���·��</param>
        /// <param name="action">�������ؽ��ȵ�ί��</param>
        async void UpdateAB(Dictionary<string, ABInfo> DownloadAB, List<ABInfo> tempList, string localPath,Action<string> action)
        {
            //���سɹ�����
            var succeedDownloadCount = 0;
            //�������ش���
            var reDownloadCount = 0;
            //��Ҫ���ش���
            var maxDownloadCount = DownloadAB.Count;

            var geDic = DownloadAB.GetEnumerator();
            //����
            while (DownloadAB.Count != 0 || reDownloadCount == 5)
            {
                while (geDic.MoveNext())
                {
                    await Task.Run(() => 
                    {
                        //һ��AB�����سɹ�
                        if (DownLoadFile(geDic.Current.Key, localPath))
                        {
                            //���سɹ���¼
                            tempList.Add(geDic.Current.Value);
                        }
                    });
                    //Debug.Log("���ؽ���:" + succeedDownloadCount++ + "/" + maxDownloadCount);
                    //�������ؽ���
                    action(++succeedDownloadCount + "/" + maxDownloadCount);
                }
                //�Ƴ����سɹ�����Դ
                foreach (var item in tempList)
                {
                    DownloadAB.Remove(item.name);
                }
                reDownloadCount++;
            }
        }

        /// <summary>
        /// ����FTP������
        /// </summary>
        /// <param name="requestUriString">��Դ·��(������Դ)</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        FtpWebRequest GetFTP(string requestUriString,string username,string password)
        {
            //����һ��FTP���� ��������
            FtpWebRequest request = WebRequest.Create(requestUriString) as FtpWebRequest;
            //����һ��ͨѶƾ֤(�û���������)
            NetworkCredential credential = new NetworkCredential(username,password);
            request.Credentials = credential;
            //���ô���Ϊnull
            request.Proxy = null;
            //���������رտ�������
            request.KeepAlive = false;
            //ָ����������Ϊ2����
            request.UseBinary = true;
            return request;
        }
        /// <summary>
        /// ��FTP������������Դ
        /// </summary>
        /// <param name="fileName">���ص��ļ���</param>
        /// <param name="filePath">���ص�·��</param>
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
                //���ò�������Ϊ����
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                //FTP����������
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream DownloadStream = response.GetResponseStream();

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                //��ȡ�ļ���Ϣ��д��FTP��
                using (FileStream fileStream = File.Create(filePath + "/" + fileName))
                {
                    //ÿ������2048B
                    var bytes = new byte[2048];
                    //��ȡ�ֽ���
                    int contentLength = DownloadStream.Read(bytes, 0, bytes.Length);
                    while (contentLength != 0)
                    {
                        //д�������ֽ���
                        fileStream.Write(bytes, 0, contentLength);
                        contentLength = DownloadStream.Read(bytes, 0, bytes.Length);
                    }
                    fileStream.Close();
                    DownloadStream.Close();
                }
                Debug.Log(fileName + "���سɹ�");
                Debug.Log(filePath+"/"+fileName);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("����ʧ��" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// �����ļ�·����ȡmd5��
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetMD5(string filePath)
        {
            //���ļ���������ʽ��
            FileStream fileStream = new FileStream(filePath, FileMode.Open);

            //����һ��md5������������md5��
            MD5 md5 = new MD5CryptoServiceProvider();
            //����md5��
            var md5Res = md5.ComputeHash(fileStream);
            fileStream.Close();

            //��16λ�ֽ�ת��λ16����ƴ�ӳ��ַ���
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in md5Res)
            {
                stringBuilder.Append(item.ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// �����ļ�����ȡmd5��
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public string GetMD5(Stream fileStream)
        {
            //���ļ���������ʽ��
            //FileStream fileStream = new FileStream(filePath, FileMode.Open);
            //����һ��md5������������md5��
            MD5 md5 = new MD5CryptoServiceProvider();
            //����md5��
            var md5Res = md5.ComputeHash(fileStream);
            fileStream.Close();

            //��16λ�ֽ�ת��λ16����ƴ�ӳ��ַ���
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
    //ab����Դ��Ϣ
    [Serializable]
    public class ABInfo
    {
        //����
        public string name;
        //��С
        public long size;
        //md5��
        public string md5;

        public ABInfo(string name, long size, string md5)
        {
            this.name = name;
            this.size = size;
            this.md5 = md5;
        }
    }
}
