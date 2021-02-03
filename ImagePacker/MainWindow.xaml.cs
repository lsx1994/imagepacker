using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Input;

namespace ImagePacker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Util.Init(this);
            ImageAutoAtlas.Init();
            //回车事件
            PlistOnlyOne.KeyUp += new KeyEventHandler(PlistOnlyOne_KeyUp);
            OnlyCopyFiles.KeyUp += new KeyEventHandler(OnlyCopyFiles_KeyUp);
        }

        public void addMessage(string msg, Color color)
        {
            Run run = new Run()
            {
                Text = string.Format("{0}    {1}\n", DateTime.Now.ToString("HH:mm:ss"), msg),
                Foreground = new SolidColorBrush(color),
            };
            phMessage.Inlines.Add(run);
            MessageList.ScrollToEnd();
        }

        //导出图集
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            PackerPlist.OutPlist();
        }
   
        //压缩
        private void BtnTiny_Click(object sender, RoutedEventArgs e)
        {
            ImageTiny.OutPng();
        }

        //刷新UI
        public void RefreshConfigUI()
        {
            PlistLoadDir.Text = Util.config.PlistLoadDir;
            PlistOutDir.Text = Util.config.PlistOutDir;
            PlistOnlyOne.Text = JsonConvert.SerializeObject(Util.config.OnlyOnePlist);
            PlistMaxSize.Text = Util.config.MaxPlistSize + "";
            OnlyCopyFiles.Text = JsonConvert.SerializeObject(Util.config.OnlyCopyFiles);
            CopyFileWidth.Text = Util.config.CopyFileWidth + "";
            CopyFileHeight.Text = Util.config.CopyFileHeight + "";
            IsForceOutPlist.IsChecked = Util.config.IsForceOutPlist;
            TinyDir.Text = Util.config.TinyLoadDir;
            TinyParm.Text = Util.config.TinyParm;
            EncryDir.Text = Util.config.EncryDir;
            AutoAtlasOutDir.Text = Util.config.AutoAtlasOutDir;
        }

        //修改加载目录
        private void PlistLoadDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var strData = Util.unifyDir(PlistLoadDir.Text);
            if (Util.config.PlistLoadDir == strData) return;
            Util.config.PlistLoadDir = strData;
            Util.SaveConfig();
        }

        //修改导出目录
        private void PlistOutDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var strData = Util.unifyDir(PlistOutDir.Text);
            if (Util.config.PlistOutDir == strData) return;
            Util.config.PlistOutDir = strData;
            Util.SaveConfig();
        }

        //修改单图图集
        private void PlistOnlyOne_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if(PlistOnlyOne.Text == "")
                    {
                        PlistOnlyOne.Text = "[]";
                    }
                    Util.config.OnlyOnePlist = JsonConvert.DeserializeObject<List<string>>(PlistOnlyOne.Text);
                    for (var i = 0; i < Util.config.OnlyOnePlist.Count; i++)
                    {
                        Util.config.OnlyOnePlist[i] = Util.unifyDir(Util.config.OnlyOnePlist[i]);
                    }
                    Util.SaveConfig();
                }
            }
            catch(Exception ex)
            {
                Util.Error(ex.Message);
            }
        }

        //修改直接复制
        private void OnlyCopyFiles_KeyUp(object sender, KeyEventArgs e)
        {
            try
            { 
                if (e.Key == Key.Enter)
                {
                    if (OnlyCopyFiles.Text == "")
                    {
                        OnlyCopyFiles.Text = "[]";
                    }
                    Util.config.OnlyCopyFiles = JsonConvert.DeserializeObject<List<string>>(OnlyCopyFiles.Text);
                    Util.SaveConfig();
                }
            }
            catch(Exception ex)
            {
                Util.Error(ex.Message);
            }
        }

        //修改忽略尺寸
        private void CopyFileWidth_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (CopyFileWidth.Text == "")
            {
                CopyFileWidth.Text = "1024";
            }
            int value = int.Parse(CopyFileWidth.Text);
            if (Util.config.CopyFileWidth == value) return;
            Util.config.CopyFileWidth = value;
            Util.SaveConfig();
        }
        private void CopyFileHeight_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (CopyFileHeight.Text == "")
            {
                CopyFileHeight.Text = "768";
            }
            int value = int.Parse(CopyFileHeight.Text);
            if (Util.config.CopyFileHeight == value) return;
            Util.config.CopyFileHeight = value;
            Util.SaveConfig();
        }

        //修改图集尺寸
        private void PlistMaxSize_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(PlistMaxSize.Text == "")
            {
                PlistMaxSize.Text = "2048";
            }
            int value = int.Parse(PlistMaxSize.Text);
            if (Util.config.MaxPlistSize == value) return;
            Util.config.MaxPlistSize = value;
            Util.SaveConfig();
        }

        //修改压缩目录
        private void TinyDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var strData = Util.unifyDir(TinyDir.Text);
            if (Util.config.TinyLoadDir == strData) return;
            Util.config.TinyLoadDir = strData;
            Util.SaveConfig();
        }

        //修改压缩参数
        private void TinyParm_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(TinyParm.Text == "")
            {
                TinyParm.Text = "--quality 65-80";
            }
            if (Util.config.TinyParm == TinyParm.Text) return;
            Util.config.TinyParm = TinyParm.Text;
            Util.SaveConfig();
        }

        //强制导出
        private void IsForceOutPlist_Checked(object sender, RoutedEventArgs e)
        {
            if (IsForceOutPlist.IsChecked == Util.config.IsForceOutPlist) return;
            Util.config.IsForceOutPlist = (IsForceOutPlist.IsChecked == true);
            Util.SaveConfig();
        }
        //修改 加密解密图片目录
        private void EncryDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            var strData = Util.unifyDir(EncryDir.Text);
            if (Util.config.EncryDir == strData) return;
            Util.config.EncryDir = strData;
            Util.SaveConfig();
        }
        //开始加密
        private void BtnEncry_Click(object sender, RoutedEventArgs e)
        {
            ImageEncode.TraversalFolder(Util.config.EncryDir, null, "ENCRY");
            Util.Log("所有图片加密完毕。");
        }
        //开始解密
        private void BtnDecry_Click(object sender, RoutedEventArgs e)
        {
            ImageEncode.TraversalFolder(Util.config.EncryDir, null, "DECRY");
            Util.Log("所有图片解密完毕。");
        }
        //自动图集目标目录
        private void AutoAtlasOutDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            var strData = Util.unifyDir(AutoAtlasOutDir.Text);
            if (Util.config.AutoAtlasOutDir == strData) return;
            Util.config.AutoAtlasOutDir = strData;
            Util.SaveConfig();
        }
        //自动图集开始复制
        private void BtnAutoAtlasCopy_Click(object sender, RoutedEventArgs e)
        {
            ImageAutoAtlas.CopyAutoAtlas(Util.config.AutoAtlasOutDir, null);
            Util.Log("自动图集复制完毕。");
        }
        //移除所有自动图集
        private void BtnAutoAtlasDel_Click(object sender, RoutedEventArgs e)
        {
            ImageAutoAtlas.DelAutoAtlas(Util.config.AutoAtlasOutDir, null);
            Util.Log("自动图集删除完毕。");
        }
    }
}
