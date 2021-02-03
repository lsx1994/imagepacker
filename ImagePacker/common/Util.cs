using System;
using System.Windows.Media;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace ImagePacker
{
    internal static class Util
    {
        public static ImagePacker.MainWindow app = null;
        public static ImagePacker.Config config;   //配置表
        public static void Init(ImagePacker.MainWindow data)
        {
            app = data;

            LoadConfig();
        }

        //统一目录斜杆
        public static string unifyDir(string strDir)
        {
            strDir = strDir.Replace("\\\\", "\\");
            strDir = strDir.Replace("/", "\\");
            strDir = strDir.Replace("//", "\\");
            return strDir;
        }
        public static string safeDir(string strDir)
        {
            strDir = strDir.Replace("\\\\", "/");
            strDir = strDir.Replace("\\", "/");
            strDir = strDir.Replace("//", "/");
            return strDir;
        }

        public static void LoadConfig()
        {
            config = JsonConfig.readFromFile<ImagePacker.Config>("config.json");
            if(config == null)
            {
                config = new Config();
            }
            if(config.OnlyOnePlist == null)
            {
                config.OnlyOnePlist = new List<string>();
            }
            if (config.OnlyCopyFiles == null)
            {
                config.OnlyCopyFiles = new List<string>();
            }
            config.PlistLoadDir = unifyDir(config.PlistLoadDir);
            config.PlistOutDir = unifyDir(config.PlistOutDir);
            config.TinyLoadDir = unifyDir(config.TinyLoadDir);
            config.EncryDir = unifyDir(config.EncryDir);
            config.AutoAtlasOutDir = unifyDir(config.AutoAtlasOutDir);
            for (var i = 0; i < config.OnlyOnePlist.Count; i++)
            {
                config.OnlyOnePlist[i] = unifyDir(config.OnlyOnePlist[i]);
            }
            app.RefreshConfigUI();
        }

        public static void SaveConfig()
        {
            JsonConfig.writeToFile("config.json", config);
        }

        public static void Log(string log)
        {
            app.addMessage(log, Color.FromArgb(0xFF, 0, 255, 0));
        }

        public static void Warn(string log)
        {
            app.addMessage(log, Color.FromArgb(0xFF, 255, 255, 0));
        }
        public static void Error(string log)
        {
            app.addMessage(log, Color.FromArgb(0xFF, 255, 0, 0));
        }

        // 获取环境目录
        public static string WorkPath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        //运行命令行数组
        public static bool RunScript(List<string> scripts)
        {
            try
            {
                if (scripts.Count <= 0)
                {
                    return false;
                }
                Runspace runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                Pipeline pipeline = runspace.CreatePipeline();
                foreach (var scr in scripts)
                {
                    pipeline.Commands.AddScript(scr);
                }
                //返回结果  
                var results = pipeline.Invoke();
                runspace.Close();

                if (results != null && results.Count > 0)
                {
                    var str = results[0].ToString();
                    if (str.IndexOf("You must agree to the license agreement before using TexturePacker") >= 0)
                    {
                        Error("请先运行[图集协议.bat] 或者 [TexturePackerGUI] 并同意软件协议。");
                        return false;
                    }
                }
                if (pipeline.Error.Count > 0)
                {
                    while (!pipeline.Error.EndOfPipeline)
                    {
                        var value = pipeline.Error.Read() as PSObject;
                        if (value != null)
                        {
                            var r = value.BaseObject as ErrorRecord;
                            if (r != null && r.Exception.Message.IndexOf("Not all sprites could be packed into the texture") >= 0)
                            {
                                string ErrorDir = r.InvocationInfo.Line;
                                int index = ErrorDir.LastIndexOf(".plist");
                                if(index >= 0)
                                {
                                    Error(ErrorDir.Remove(0, index+7));
                                }
                                Error(r.Exception.Message);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Error("ShellError：" + e.Message);
                return false;
            }
            return true;
        }

        public static void WriteLogToTextFile(List<string> list, string txtFile)
        {
            if (!Directory.Exists("Log")){
                Directory.CreateDirectory("Log");
            }

            //创建一个文件流，用以写入或者创建一个StreamWriter
            FileStream fs = new FileStream(txtFile, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            // 使用StreamWriter来往文件中写入内容
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < list.Count; i++) sw.WriteLine(list[i]);
            //关闭此文件
            sw.Flush();
            sw.Close();
            fs.Close();
        }
    }
}
