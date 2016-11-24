using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Permissions;

namespace ScanFileWin
{
    public struct ScanParam
    {
        public bool shift; 

        public string path;         //路径
        public string pattern;      //模式
        public string oldstr;       //要查找的字符串
        public string newstr;       //新的字符串

        public string SRC_ENCODING; //源文件编码
        public string DEST_ENCODING;//新文件编码
    }

    public class ScanEventArgs : EventArgs
    {
        public string currentfile;
        public string trojanfile;
        public string message;

    }
    public class Scan
    {
        ScanParam _param;
        public int filenums=0;          //总扫描文件个数
        public int trojan_filenums = 0; //挂马文件个数


        public delegate void ScanDelegate(ScanEventArgs args);
        public event ScanDelegate OnRunningEvent;
        public event ScanDelegate OnRunStartEvent;
        public event ScanDelegate OnStopEvent;

        public Scan(ScanParam param)
        {
            _param = param;
        }
        
        

        private string[] EnumerateFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }


        private void _run(object f)
        {
            //if (RunStartEvent != null)
            //    RunStartEvent("开始.....");

            try
            {
                ScanParam p = _param;
                string[] files = (string[]) f;// EnumerateFiles(p.path, p.pattern);
                Encoding srcEncode = Encoding.GetEncoding(p.SRC_ENCODING);
                Encoding destEncode = Encoding.GetEncoding(p.DEST_ENCODING);
                
                foreach (var item in files)
                {                    
                    filenums++;
                    ScanEventArgs args = new ScanEventArgs();
                    StreamReader sr = new StreamReader(item, srcEncode);
                    string c = sr.ReadToEnd();
                    sr.Close();               
                    if (c.IndexOf(p.oldstr) > 0)
                    {
                        args.trojanfile = string.Format("找到特征文件 {0}", item);
                        if (p.shift) //替换
                        {
                            c = c.Replace(p.oldstr, p.newstr);
                            StreamWriter sw = new StreamWriter(item, false, destEncode);
                            sw.Write(c);
                            sw.Close();
                        }
                        trojan_filenums++;
                    }
                    if (this.OnRunningEvent != null)
                    {
                        args.currentfile = string.Format("扫描文件: {0}", item);
                        OnRunningEvent(args);
                    }
                }
            }
            catch (Exception ex)
            {
                if (this.OnRunningEvent != null)
                {
                    string msg = ex.Message + "\r\n" + ex.StackTrace;
                    //OnRunningEvent(new ScanEventArgs() { message = msg });
                }
            }
        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void working(string path)
        {
            try
            {
                string[] files = Directory.GetFiles(path, _param.pattern, SearchOption.TopDirectoryOnly);

                _run(files);
                GC.Collect();
                string[] dirs = Directory.GetDirectories(path);
                foreach (var item in dirs)
                {
                    working(item);
                }
            }
            catch (Exception ex)
            {
                
            }
        }


        public void scanning(object basepath)
        {
            working(basepath.ToString());
            if (OnStopEvent != null)
                OnStopEvent(new ScanEventArgs() { message = string.Format("共扫描文件{0}个，发现特征文件{1}个。",filenums,trojan_filenums) });
        }
      
    }
}
