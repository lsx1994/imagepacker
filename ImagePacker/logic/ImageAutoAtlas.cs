using System.Collections.Generic;
using System.IO;

namespace ImagePacker
{
    class ImageAutoAtlas
    {
        static List<string> filelist = new List<string>();
        public static void Init()
        {
            var theFolder = new DirectoryInfo("./AutoAtlas");
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                filelist.Add(NextFile.Name);
            }
        }

        public static void CopyAutoAtlas(string path, DirectoryInfo theFolder)
        {
            if (theFolder == null)
            {
                theFolder = new DirectoryInfo(@path);
            }
            //遍历文件
            int iImageCount = 0;
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                string strType = NextFile.Extension.ToLower();
                if (strType == ".png" || strType == ".jpg") //只判断图片
                {
                    iImageCount++;
                }
                if (iImageCount >= 2)
                {
                    foreach(string file in filelist)
                    {
                        File.Copy("./AutoAtlas/" + file, theFolder.FullName + "/" + file, true);
                    }
                    break;
                }
            }
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                CopyAutoAtlas(NextFolder.FullName, NextFolder);
            }
        }

        public static void DelAutoAtlas(string path, DirectoryInfo theFolder)
        {
            if (theFolder == null)
            {
                theFolder = new DirectoryInfo(@path);
            }
            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                if (NextFile.Name == "AutoAtlas.pac" || NextFile.Name == "AutoAtlas.pac.meta")
                {
                    File.Delete(NextFile.FullName);
                }
            }
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                DelAutoAtlas(NextFolder.FullName, NextFolder);
            }
        }
    }
}
