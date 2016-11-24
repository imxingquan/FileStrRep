using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Linq;

namespace SFile
{
    public struct Param
    {
        public string path;
        public string pattern;
        public string oldstr;
        public string newstr;

        public string SRC_ENCODING;
        public string DEST_ENCODING;
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 6)
                {
                    Console.WriteLine("文件编码批量转换语法:\n   et.exe c:\\temp *.txt old new src_encoding dest_encoding \n\t et.exe c:/temp *.txt hi hello utf-8 gb2312");
                    return;
                }
                
                Param p = new Param();
                p.path = args[0];
                p.pattern = args[1];
                p.oldstr = args[2];
                p.newstr = args[3];
                p.SRC_ENCODING = args[4];
                p.DEST_ENCODING = args[5];

                Thread th = new Thread(Program.run);
                th.Start(p);

            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }
            catch (Exception ex) { }
            finally { }
            //Console.ReadKey();
        }

        //public static List<string> ReadFile(string file)
        //{
        //    List<string> list = new List<string>();
        //    StreamReader sr = new StreamReader(file);
        //    int i = 0;
        //    while (sr.Peek() > 0)
        //    {
        //        list.Add(sr.ReadLine());
        //        if (++i > 5)
        //            break;
        //    }
        //    sr.Close();
        //    return list;

        //}

        List<string> fileList = new List<string>();

        public static string[] EnumerateFiles(string path,string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }


        public static void run(object key)
        {
            Param p = (Param)key;
            string[] files = EnumerateFiles(p.path, p.pattern);
            Encoding srcEncode = Encoding.GetEncoding(p.SRC_ENCODING);
            Encoding destEncode = Encoding.GetEncoding(p.DEST_ENCODING);

            foreach (var item in files)
            {
                StreamReader sr = new StreamReader(item,srcEncode);
                string c = sr.ReadToEnd();
                sr.Close();
                
                //byte[] srcb = srcEncode.GetBytes(c);
                //byte[] destb = Encoding.Convert(srcEncode, destEncode, srcb);
                //c = destEncode.GetString(destb);
                c = c.Replace(p.oldstr, p.newstr);
                              
                StreamWriter sw = new StreamWriter(item, false,destEncode);
                sw.Write(c);
                sw.Close();

                //byte[] b = File.ReadAllBytes(item);
                //byte[] nb = Encoding.Convert(Encoding.GetEncoding(p.SRC_ENCODING), Encoding.GetEncoding(p.DEST_ENCODING), b);
                //File.WriteAllBytes(item, nb);
            }
        }


        public static void run2(object key)
        {
            Param p = (Param)key;
            string[] files = EnumerateFiles(p.path, p.pattern);
            Encoding srcEncode = Encoding.GetEncoding(p.SRC_ENCODING);
            Encoding destEncode = Encoding.GetEncoding(p.DEST_ENCODING);

            foreach (var item in files)
            {
                byte[] srcb = File.ReadAllBytes(item);
                byte[] destb = Encoding.Convert(Encoding.ASCII, Encoding.UTF8, srcb);
                string c = destEncode.GetString(destb);
                c = c.Replace(p.oldstr, p.newstr);

                File.WriteAllText(item, c);
            
            }
        }
    }
}
