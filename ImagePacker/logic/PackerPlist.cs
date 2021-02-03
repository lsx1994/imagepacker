using System;
using System.Collections.Generic;
using System.IO;


namespace ImagePacker
{
    static class PackerPlist
    {
        static Dictionary<string, string> OldImageInfo;    //缓存的图片信息
        static Dictionary<string, List<string>> FolderImageRecord = new Dictionary<string, List<string>>(); //记录每个文件夹里面的图片信息
        static Dictionary<string, string> NewImageInfo = new Dictionary<string, string>();    //缓存的图片信息

        static List<string> PlistCmdList = new List<string>();  //命令行数组
        static Dictionary<string, string> ImageMoveMap = new Dictionary<string, string>();  //图片剪切记录
        static string FirstLoadDirHead = null;  //目录头
        static string FirstOutDirHead = null;
        static List<string> LogOut = new List<string>();

        static byte[] ulongRead = new byte[8];                                      //读取缓存
        static ulong PlistPasswordInt = 123456789987654321;                         //plist密码
        static byte[] PlistPasswordByte = BitConverter.GetBytes(PlistPasswordInt);  //密码缓存
        public static void PackPlist(string path, DirectoryInfo theFolder)
        {
            string outFolder; //输出目录
            string head = "";
            if (theFolder == null)
            {
                theFolder = new DirectoryInfo(@path);
                outFolder = Util.config.PlistOutDir + "\\";   //目标文件夹
                FirstLoadDirHead = theFolder.Name;      //主目录
            }
            else
            {
                head = theFolder.FullName.Remove(0, theFolder.FullName.IndexOf(FirstLoadDirHead) + FirstLoadDirHead.Length + 1);
                outFolder = Util.config.PlistOutDir + "\\" + head + "\\"; //目标文件夹
            }
            //目标文件夹不存在就创建
            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }
            bool IsPlistChange = false;     //图集图片发生改变
            bool IsHaveImage = false;       //目录下是否有图片
            bool IsHavePlistImage = false;  //是否有需要打包的图片
            int ImageWidth = 0;             //图片宽度
            int ImageHeight = 0;            //图片高度

            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                PackPlist(NextFolder.FullName, NextFolder);
            }

            List<string> ImageRecord = null;
            FolderImageRecord.TryGetValue(head, out ImageRecord);
            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                string strType = NextFile.Extension.ToLower();
                if (strType == ".png" || strType == ".jpg") //只判断图片
                {
                    IsHaveImage = true;
                    bool IsNewImage = false;
                    var fileData = NextFile.Open(FileMode.Open);
                    //读取密码
                    fileData.Seek(-8, SeekOrigin.End);
                    fileData.Read(ulongRead, 0, 8);
                    if (PlistPasswordInt != System.BitConverter.ToUInt64(ulongRead, 0))
                    {
                        //没找到密码说明是新的
                        IsNewImage = true;
                        fileData.Seek(0, SeekOrigin.End);
                        fileData.Write(PlistPasswordByte, 0, 8);
                    }
                    //从缓存中取出数据
                    string key = head + "\\" + NextFile.Name + "_" + fileData.Length;
                    string data = null;
                    if (ImageRecord != null)
                    {
                        ImageRecord.Remove(key);
                    }
                    if (OldImageInfo != null)
                    {
                        OldImageInfo.TryGetValue(key, out data);
                    }
                    if (data != null)
                    {
                        //找到缓存了
                        NewImageInfo.Add(key, data);
                        var value = data.Split('|');
                        ImageWidth = int.Parse(value[0]);
                        ImageHeight = int.Parse(value[1]);
                    }
                    else
                    {
                        //没缓存，是新图片
                        IsNewImage = true;
                        System.Drawing.Image image = System.Drawing.Image.FromStream(fileData);
                        ImageWidth = image.Width;
                        ImageHeight = image.Height;
                        NewImageInfo.Add(key, ImageWidth + "|" + ImageHeight);
                    }
                    //关闭文件
                    fileData.Close();
                    //大图需要剪切
                    if (Util.config.OnlyCopyFiles.IndexOf(NextFile.Name) >= 0 ||
                        (ImageHeight >= Util.config.CopyFileHeight && ImageWidth >= Util.config.CopyFileWidth))
                    {
                        string strSrc = NextFile.FullName;
                        string strDesc = outFolder + Util.GetTimeStamp() + "_" + NextFile.Name;
                        string strCopy = outFolder + NextFile.Name;
                        File.Move(strSrc, strDesc);
                        ImageMoveMap.Add(strSrc, strDesc);
                        //目标位置没有图片
                        if (!File.Exists(strCopy))
                        {
                            File.Copy(strDesc, strCopy);
                            LogOut.Add("ImageCopy: " + strCopy);
                        }
                    }
                    else 
                    {
                        IsHavePlistImage = true;
                        if (IsNewImage)
                        {
                            //小图需要合并图集
                            IsPlistChange = true;
                        }    
                    }
                }
            }

            //有图片被删掉了,强制生成
            if(ImageRecord != null && ImageRecord.Count > 0)
            {
                IsPlistChange = true;
            }

            //导出的名字
            string PlistName = theFolder.Name;
            if (FirstLoadDirHead == PlistName)
            {
                PlistName = Util.config.PlistOutDir;
                PlistName = PlistName.Remove(0, PlistName.LastIndexOf("\\") + 1);
            }
            //plist是否存在
            if (!IsPlistChange && IsHavePlistImage && (!File.Exists(outFolder + PlistName + ".png") || !File.Exists(outFolder + PlistName + ".plist")))
            {
                IsPlistChange = true;
            }
            //必须有图片，有改变或者强制才导出图集
            if (IsHaveImage && (IsPlistChange || Util.config.IsForceOutPlist))
            {
                int MaxPlistSize = Util.config.MaxPlistSize;
                if (Util.config.OnlyOnePlist.IndexOf(head) >= 0) //判断是否单图模式
                {
                    MaxPlistSize = 16384;
                }
                //为了优化时间，只有在添加满足图集条件的新图片时才导出， 删除图片不检测。
                //--multipack  \"{n1}\" 多图模式有问题，不要用，分子文件夹
                LogOut.Add("PackPlist: " + path + " PlistSize: " + MaxPlistSize);
                string cmd = "./TexturePacker/bin/TexturePacker --allow-free-size --opt RGBA8888 --format cocos2d --enable-rotation --trim-sprite-names";
                if (!Util.config.IsForceOutPlist)
                {
                    cmd += " --smart-update";   //强制导出模式
                }
                cmd += " --ignore-files */" + theFolder.Name + "/*/*";  //忽略子文件夹
                cmd += " --max-size " + MaxPlistSize;                   //图集最大尺寸
                cmd += " --sheet " + outFolder + PlistName + ".png";    //图片
                cmd += " --data " + outFolder + PlistName + ".plist";   //plist
                cmd += " " + path;                                      //来源
                PlistCmdList.Add(cmd);
            }
        }

        static void ClearPlist(string path, DirectoryInfo theFolder)
        {
            if (theFolder == null)
            {
                if (!Directory.Exists(path))    //文件夹不存在
                {
                    return;
                }
                theFolder = new DirectoryInfo(@path);
                FirstOutDirHead = theFolder.Name;
            }
            else
            {
                var strFolder = Util.config.PlistLoadDir + "\\" + theFolder.FullName.Remove(0, theFolder.FullName.IndexOf(FirstOutDirHead) + FirstOutDirHead.Length + 1);
                if (!Directory.Exists(strFolder)) //源文件夹没这个目录，直接删除
                {
                    theFolder.Delete(true);
                    return;
                }
            }
            

            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                string strType = NextFile.Extension.ToLower();
                if(strType == ".png" || strType == ".jpg" || strType == ".plist")
                {
                    NextFile.Delete();
                }
            }

            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                ClearPlist(NextFolder.FullName, NextFolder);
            }
        } 

        public static void OutPlist()
        {
            try
            {
                if (Util.config.IsForceOutPlist)
                {
                    Util.Log("ClearPlist Start...");
                    ClearPlist(Util.config.PlistOutDir, null);
                    Util.Log("ClearPlist Over... Path: " + Util.config.PlistOutDir);
                }

                long startTime = Util.GetTimeStamp();
                Util.Log("OutPlist Start...");
                //读取缓存
                OldImageInfo = JsonConfig.readFromFile<Dictionary<string, string>>(Util.config.PlistLoadDir + "/ImageInfoCache.json");
                FolderImageRecord.Clear();
                if (OldImageInfo != null)
                {
                    foreach (var filename in OldImageInfo)
                    {
                        var dir = filename.Key.Substring(0, filename.Key.LastIndexOf("\\"));
                        List<string> filelist = null;
                        FolderImageRecord.TryGetValue(dir, out filelist);
                        if (filelist == null)
                        {
                            filelist = new List<string>();
                            FolderImageRecord.Add(dir, filelist);
                        }
                        filelist.Add(filename.Key);
                    }
                }
                //计算需要打包的目录
                PackPlist(Util.config.PlistLoadDir, null);
                //开始打包图集
                if(!Util.RunScript(PlistCmdList))
                {
                    Util.Log("没有图集变化。");
                }
                else
                {
                    Util.Log("所有图集导出完毕，详情查看LogPackerPlist。");
                }
                PlistCmdList.Clear();
                //剪切的大图复制到原位置
                foreach(var data in ImageMoveMap)
                {
                    File.Move(data.Value, data.Key);
                }
                ImageMoveMap.Clear();
                //保存缓存
                JsonConfig.writeToFile(Util.config.PlistLoadDir + "/ImageInfoCache.json", NewImageInfo, true);
                NewImageInfo.Clear();

                Util.Log("OutPlist Over... 总耗时: " + (Util.GetTimeStamp() - startTime) + "秒");

                //保存log
                if (LogOut.Count > 0)
                {
                    Util.WriteLogToTextFile(LogOut, "Log/LogPackerPlist.txt");
                    LogOut.Clear();
                }
            }
            catch (System.Exception ex)
            {
                Util.Error(ex.Message);
            } 
        }  
    }
}


