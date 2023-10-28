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

        [MenuItem("AB������/�򿪱༭������")]
        private static void OpenEditorWindown()
        {
            var windown = GetWindowWithRect(typeof(ABTools), new Rect(0, 0, 300, 180), false, "AB���༭��");
            windown.Show();
        }

        //����AB����Դ�Ա��ļ�(����)
        void CreateABCompareFile()
        {
            //�Զ������л��б�
            var ABList = new Serialization<ABInfo>(new List<ABInfo>());
            //��ȡAB��
            var path = Application.dataPath + "/ABRes/" + target[selectIndex];
            DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            var fileInfo = directoryInfo.GetFiles();
            foreach (var item in fileInfo)
            {
                if (item.Extension == "")
                {
                    //Debug.Log(item.Name);
                    //���� ���� ��С md5��
                    ABList.ToList.Add(new ABInfo(item.Name, item.Length, UpdateABMgr.Instance.GetMD5(item.FullName)));
                    //ABList.Tolist.Add(new ABInfo() { name = item.Name, size = item.Length, md5 = GetMD5(item.FullName) });
                }
            }
            //������ת����json��ʽ
            var json = JsonUtility.ToJson(ABList, true);
            //�浵·��������
            File.WriteAllText(path + "/ABCompare.date", json);

            Debug.Log(json);
        }

        //�ϴ�AB��
        private async void UploadABFile()
        {
            //��ȡAB�ļ���
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/ABRes/" + target[selectIndex]);
            var fileInfo = directoryInfo.GetFiles();
            foreach (var item in fileInfo)
            {
                await Task.Run(() =>
                {
                    if (item.Extension == "" || item.Extension == ".date")
                    {
                    //�ϴ���ftp������
                    UploadToFTP(item.FullName, item.Name);
                    }
                });
            }
        }
        /// <summary>
        /// ��AB���ϴ���FTP������(�첽)
        /// </summary>
        /// <param name="filePath">�ļ�·��</param>
        /// <param name="fileName">�ļ���</param>
        private async void UploadToFTP(string filePath, string fileName)
        {
            await Task.Run(() =>
            {
                try
                {
                //����ļ����Ƿ����
                var request = LinkFTP(null);
                    if (!CheckFTPDirectory(request))
                    {
                        request = LinkFTP(null);
                        CreateFTPDirectory(request);
                    }

                //�ϴ�
                request = LinkFTP(fileName);
                //���ò�������Ϊ�ϴ�
                request.Method = WebRequestMethods.Ftp.UploadFile;
                //ָ����������Ϊ2����
                request.UseBinary = true;

                //FTP������
                Stream UploadStream = request.GetRequestStream();

                //��ȡ�ļ���Ϣ��д��FTP��
                using (FileStream fileStream = File.OpenRead(filePath))
                    {
                    //ÿ���ϴ�2048B
                    var bytes = new byte[2048];
                    //��ȡ�ֽ���
                    int contentLength = fileStream.Read(bytes, 0, bytes.Length);
                        while (contentLength != 0)
                        {
                        //д�������ֽ���
                        UploadStream.Write(bytes, 0, contentLength);
                            contentLength = fileStream.Read(bytes, 0, bytes.Length);
                        }
                        fileStream.Close();
                        UploadStream.Close();
                    }
                    Debug.Log(fileName + "�ϴ��ɹ�");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("�ϴ�ʧ��" + e.Message);
                }
            });
        }

        FtpWebRequest LinkFTP(string fileName)
        {
            //����һ��FTP���� �����ϴ�
            FtpWebRequest request = WebRequest.Create(serverIP + target[selectIndex] + "/" + fileName) as FtpWebRequest;
            //����һ��ͨѶƾ֤(�û���������)
            NetworkCredential credential = new NetworkCredential("FTP Upload", "123456");
            request.Credentials = credential;
            //���ô���Ϊnull
            request.Proxy = null;
            //���������رտ�������
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
                Debug.Log("����");
                return true;
            }
            catch (System.Exception)
            {
                Debug.LogError("������");
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
            GUI.Label(new Rect(10, 10, 150, 15), "ƽ̨ѡ��");
            selectIndex = GUI.Toolbar(new Rect(10, 30, 250, 20), selectIndex, target);

            GUI.Label(new Rect(10, 60, 150, 15), "��������ַ");
            serverIP = GUI.TextField(new Rect(10, 80, 150, 20), serverIP);

            if (GUI.Button(new Rect(10, 110, 130, 40), "������Դ�Ա��ļ�"))
            {
                CreateABCompareFile();
            }
            if (GUI.Button(new Rect(145, 110, 130, 40), "�ϴ�AB���ͶԱ��ļ�"))
            {
                UploadABFile();
            }
        }
    }
}
