using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImagePacker
{
    static class ImageEncode
    {
        static string Tag = "EODENCRY";
        static byte Key = 0x99;
        static int MaxImageSize = 1024 * 1024 * 20;  //图片最多20M
        static byte[] ReadBuffer = new byte[MaxImageSize]; 
        static byte[] WriteBuffer = new byte[MaxImageSize]; 

        public static void TraversalFolder(string path, DirectoryInfo theFolder, string LogicType)
        {
            if (theFolder == null)
            {
                theFolder = new DirectoryInfo(@path);
            }

            if (LogicType == "ENCRY")    //加密
            {
                var ary = Tag.ToCharArray();
                for (var i = 0; i < Tag.Length; i++)
                {
                    WriteBuffer[i] = (byte)ary[i];
                }
            }

            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                string strType = NextFile.Extension.ToLower();
                if (strType == ".png" || strType == ".jpg") //只判断图片
                {
                    var readFile = NextFile.Open(FileMode.Open);
                    int FileSize = (int)readFile.Length;
                    readFile.Read(ReadBuffer, 0, FileSize);
                    readFile.Close();

                    var writeFile = NextFile.Open(FileMode.Truncate);
                    string strHead = System.Text.Encoding.Default.GetString(ReadBuffer, 0, Tag.Length);
                    if (LogicType == "ENCRY")    //加密
                    {
                        if(strHead == Tag)
                        {
                            Util.Error("该图片已加密，请勿重复。" + NextFile.FullName);
                            continue;
                        }
                        //加密
                        for (var i=0; i < FileSize; i++)
                        {
                            int value = (ReadBuffer[i] + i - i%3) ^ Key;
                            WriteBuffer[i + Tag.Length] = (byte)value;
                        }
                        writeFile.Write(WriteBuffer, 0, FileSize + Tag.Length);
                    }
                    else if (LogicType == "DECRY")  //解密
                    {
                        if (strHead != Tag)
                        {
                            Util.Error("该图片未加密，无法解密。" + NextFile.FullName);
                            continue;
                        }
                        //解密
                        for (var i = 0; i < FileSize; i++)
                        {
                            int value = (ReadBuffer[i + Tag.Length] ^ Key) - i + i%3;
                            WriteBuffer[i] = (byte)value;
                        }
                        writeFile.Write(WriteBuffer, 0, FileSize - Tag.Length);
                    }
                    writeFile.Close();
                }
            }
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                TraversalFolder(NextFolder.FullName, NextFolder, LogicType);
            }
        }
    }
}
