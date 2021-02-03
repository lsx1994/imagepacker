using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImagePacker
{
    class ImageTiny
    {
        static Dictionary<string, int> TinyFileList = new Dictionary<string, int>();
        static List<string> TinyCmdList = new List<string>();  //命令行数组
        static List<string> LogOut = new List<string>();

        static byte[] ulongRead = new byte[8];                                      //读取缓存
        static ulong TinyPasswordInt = 987654321123456789;                          //tiny密码
        static byte[] TinyPasswordByte = BitConverter.GetBytes(TinyPasswordInt);    //密码缓存
        //开始压缩图片
        public static void TinyPng(string path, DirectoryInfo theFolder)
        {
            if (theFolder == null)
            {
                theFolder = new DirectoryInfo(@path);
            }
            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                string strType = NextFile.Extension.ToLower();
                if (strType == ".png" || strType == ".jpg") //只判断图片
                {
                    var fileData = NextFile.OpenRead();
                    //读取密码
                    fileData.Seek(-8, SeekOrigin.End);
                    fileData.Read(ulongRead, 0, 8);
                    fileData.Close();
                    if (TinyPasswordInt != System.BitConverter.ToUInt64(ulongRead, 0))
                    {
                        //没找到密码说明是新的
                        TinyCmdList.Add("./pngquant/pngquant " + Util.config.TinyParm + " " + NextFile.FullName + " --ext .png --force --skip-if-larger");
                        TinyFileList.Add(NextFile.FullName, (int)NextFile.Length);
                    }
                }
            }
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                TinyPng(NextFolder.FullName, NextFolder);
            }
        }

        public static void OutPng()
        {
            long startTime = Util.GetTimeStamp();
            Util.Log("TinyPng Start...");

            TinyPng(Util.config.TinyLoadDir, null);
            if (!Util.RunScript(TinyCmdList))
            {
                Util.Log("没有图片需要压缩。");
            }
            else
            {
                Util.Log("所有图片压缩完毕，详情查看LogImageTiny。");
            }
            //压缩完成
            foreach (var data in TinyFileList)
            {
                FileInfo tinyFile = new FileInfo(data.Key);
                var tinyData = tinyFile.OpenWrite();
                tinyData.Seek(0, SeekOrigin.End);
                tinyData.Write(TinyPasswordByte, 0, 8);
                var str = new StringBuilder();
                str.AppendFormat("TinyPng: {0} 压缩前: {1}KB 压缩后: {2}KB", data.Key, data.Value / 1024, tinyData.Length / 1024);
                LogOut.Add(str.ToString());
                tinyData.Close();
            }
            TinyFileList.Clear();
            TinyCmdList.Clear();

            Util.Log("TinyPng Over... 总耗时: " + (Util.GetTimeStamp() - startTime) + "秒");
            //保存log
            if (LogOut.Count > 0)
            {
                Util.WriteLogToTextFile(LogOut, "Log/LogImageTiny.txt");
                LogOut.Clear();
            }
        }
    }
}
