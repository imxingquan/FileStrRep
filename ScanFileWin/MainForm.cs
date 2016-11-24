using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace ScanFileWin
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            //CheckForIllegalCrossThreadCalls = true;
            this.Text += Application.ProductVersion;
            init();

        }
    
        Thread thread;

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (0 == (int)btnStart.Tag )
            {
                btnStart.Tag = 1;
                btnStart.Text = "停止";

                textBox1.Text = "";

                string path = textBox_DestDir.Text.Trim();
                if (!Directory.Exists(path))
                {

                    ErrorTips(textBox_DestDir, "目录不存在");
                    textBox_DestDir.Focus();
                    return;
                }
                string pattern = textBox_Pattern.Text.Trim();
                if (pattern.Length == 0)
                {
                    ErrorTips(textBox_Pattern, "文件名模式不能为空啊啊!");
                    return;
                }


                string src_str = textBox_SrcStr.Text;
                if (src_str.Length == 0)
                {
                    ErrorTips(textBox_SrcStr, "查找的字符串不能为空啊!!");
                    return;
                }

                if ("" == comboBox_SrcEncode.Text)
                {
                    ErrorTips(textBox_Pattern, "原文件编码不能为空啊啊!");
                    return;
                }
                if ("" == comboBox_DescEncode.Text)
                {
                    ErrorTips(textBox_Pattern, "目标文件编码不能为空啊啊!");
                    return;
                }

                ScanParam p = new ScanParam();
                p.path = path;
                p.oldstr = src_str;
                p.newstr = textBox_DestStr.Text;
                p.pattern = textBox_Pattern.Text.Trim();
                p.SRC_ENCODING = comboBox_SrcEncode.Text;
                p.DEST_ENCODING = comboBox_DescEncode.Text;
                p.shift = !checkBox_OnlyScan.Checked;

                Scan scan = new Scan(p);
                scan.OnRunningEvent += new Scan.ScanDelegate(scan_RunningEvent);
                scan.OnStopEvent += new Scan.ScanDelegate(scan_OnStopEvent);
                
                thread = new Thread(scan.scanning);
                thread.Start(path);

            }
            else
            {
                if (thread != null)
                {
                   
                    btnStart.Text = "开始";
                    btnStart.Tag = 0;
                    if (thread.ThreadState == ThreadState.Running) 
                     thread.Abort();
                    
                }
                
            }
        }

        void scan_OnStopEvent(ScanEventArgs args)
        {
            label9.Text = args.message;
            if (thread != null)
            {
                btnStart.Text = "开始";
                btnStart.Tag = 0;
                if (thread.ThreadState == ThreadState.Running)
                    thread.Abort();
               
            }
        }
              

        void scan_RunningEvent(ScanEventArgs e)
        {
            label9.Text = e.currentfile;
            if(!string.IsNullOrEmpty(e.trojanfile))
                textBox1.AppendText(e.trojanfile + "\r\n");
            
        }

        public void init()
        {
            EncodingInfo[] info = Encoding.GetEncodings();
            foreach (var item in info)
            {
                comboBox_SrcEncode.Items.Add(item.GetEncoding().BodyName);
                comboBox_DescEncode.Items.Add(item.GetEncoding().BodyName);
            }

            comboBox_SrcEncode.Text = Encoding.Default.BodyName;
            comboBox_DescEncode.Text = Encoding.Default.BodyName;
            btnStart.Tag = 0;
        }

        public void ErrorTips(Control ctrl,string message)
        {
            MessageBox.Show(message);
            //errorProvider1.SetError(ctrl, message);
            
            ctrl.Focus();
        }

        
              
    }
}
