using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace FrameworkDesign
{

    public class ABTools : EditorWindow
    {
        int selectIndex = 0;
        string[] target = { "PC", "Android", "IOS" };

        string serverIP = "ftp://192.168.84.10/AB/";

        [MenuItem("AB包工具/打开编辑器窗口")]
        private static void OpenEditorWindown()
        {
            var windown = GetWindowWithRect(typeof(ABTools), new Rect(0, 0, 300, 180), false, "AB包编辑器");
            windown.Show();
        }

        //创建AB包资源对比文件(本地)
        void CreateABCompareFile()
        {
            //自定义序列化列表
            var ABList = new Serialization<ABInfo>(new List<ABInfo>());
            //获取AB包
            var path = Application.dataPath + "/ABRes/" + target[selectIndex];
            DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            var fileInfo = directoryInfo.GetFiles();
            foreach (var item in fileInfo)
            {
                if (item.Extension == "")
                {
                    //Debug.Log(item.Name);
                    //保存 包名 大小 md5码
                    ABList.ToList.Add(new ABInfo(item.Name, item.Length, UpdateABMgr.Instance.GetMD5(item.FullName)));
                    //ABList.Tolist.Add(new ABInfo() { name = item.Name, size = item.Length, md5 = GetMD5(item.FullName) });
                }
            }
            //将数据转换成json格式
            var json = JsonUtility.ToJson(ABList, true);
            //存档路径及名称
            File.WriteAllText(path + "/ABCompare.date", json);

            Debug.Log(json);
        }

        //上传AB包
        private async void UploadABFile()
        {
            //获取AB文件夹
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/ABRes/" + target[selectIndex]);
            var fileInfo = directoryInfo.GetFiles();
            foreach (var item in fileInfo)
            {
                await Task.Run(() =>
                {
                    if (item.Extension == "" || item.Extension == ".date")
                    {
                    //上传至ftp服务器
                    UploadToFTP(item.FullName, item.Name);
                    }
                });
            }
        }
        /// <summary>
        /// 将AB包上传至FTP服务器(异步)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        private async void UploadToFTP(string filePath, string fileName)
        {
            await Task.Run(() =>
            {
                try
                {
                //检测文件夹是否存在
                var request = LinkFTP(null);
                    if (!CheckFTPDirectory(request))
                    {
                        request = LinkFTP(null);
                        CreateFTPDirectory(request);
                    }

                //上传
                request = LinkFTP(fileName);
                //设置操作命令为上传
                request.Method = WebRequestMethods.Ftp.UploadFile;
                //指定传输类型为2进制
                request.UseBinary = true;

                //FTP流对象
                Stream UploadStream = request.GetRequestStream();

                //读取文件信息，写入FTP流
                using (FileStream fileStream = File.OpenRead(filePath))
                    {
                    //每次上传2048B
                    var bytes = new byte[2048];
                    //读取字节数
                    int contentLength = fileStream.Read(bytes, 0, bytes.Length);
                        while (contentLength != 0)
                        {
                        //写入流的字节数
                        UploadStream.Write(bytes, 0, contentLength);
                            contentLength = fileStream.Read(bytes, 0, bytes.Length);
                        }
                        fileStream.Close();
                        UploadStream.Close();
                    }
                    Debug.Log(fileName + "上传成功");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("上传失败" + e.Message);
                }
            });
        }

        FtpWebRequest LinkFTP(string fileName)
        {
            //创建一个FTP连接 用于上传
            FtpWebRequest request = WebRequest.Create(serverIP + target[selectIndex] + "/" + fileName) as FtpWebRequest;
            //设置一个通讯凭证(用户名，密码)
            NetworkCredential credential = new NetworkCredential("FTP Upload", "123456");
            request.Credentials = credential;
            //设置代理为null
            request.Proxy = null;
            //请求结束后关闭控制连接
            request.KeepAlive = false;
            return request;
        }

        bool CheckFTPDirectory(FtpWebRequest request)
        {
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.UseBinary = true;
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
                Debug.Log("存在");
                return true;
            }
            catch (System.Exception)
            {
                Debug.LogError("不存在");
                return false;
            }
        }

        void CreateFTPDirectory(FtpWebRequest request)
        {
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 150, 15), "平台选择");
            selectIndex = GUI.Toolbar(new Rect(10, 30, 250, 20), selectIndex, target);

            GUI.Label(new Rect(10, 60, 150, 15), "服务器地址");
            serverIP = GUI.TextField(new Rect(10, 80, 150, 20), serverIP);

            if (GUI.Button(new Rect(10, 110, 130, 40), "创建资源对比文件"))
            {
                CreateABCompareFile();
            }
            if (GUI.Button(new Rect(145, 110, 130, 40), "上传AB包和对比文件"))
            {
                UploadABFile();
            }
        }
    }
}
