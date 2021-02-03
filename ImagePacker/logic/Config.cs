using System.Collections.Generic;

namespace ImagePacker
{
    class Config
    {
        //图集数据
        public string PlistLoadDir = "input PlistLoadDir";      //资源目录
        public string PlistOutDir = "input PlistOutDir";        //导出目录
        public int MaxPlistSize = 2048;                         //图集最大尺寸
        public int CopyFileWidth = 1024;                        //直接拷贝尺寸
        public int CopyFileHeight = 768;                        //直接拷贝尺寸
        public List<string> OnlyOnePlist = new List<string>();  //唯一大图的图集目录
        public List<string> OnlyCopyFiles = new List<string>(); //直接复制的图片名字
        public bool IsForceOutPlist = false;                    //无视历史记录强制导出plist
        //压缩数据
        public string TinyLoadDir = "input TinyLoadDir";    //压缩目录
        public string TinyParm = "--quality 65-80";         //压缩参数
        //加密数据
        public string EncryDir = "input EncryDir"; 
        //自动图集
        public string AutoAtlasOutDir = "input AutoAtlasOutDir";
    }
}
