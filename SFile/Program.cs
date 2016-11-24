using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SFile
{
    public struct Param
    {
        public string[] args;
        public string temp_dir;
    }

    class Program
    {
        static void Main(string[] args)
        {
            
            try
            {
                //string temp = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"temp");
                //if (!Directory.Exists(temp)) Directory.CreateDirectory(temp);
                if (args.Length < 4)
                {
                    Console.WriteLine("语法:\n   SFile.exe c:\\temp *.txt searchkey log.txt -del");
                    return;
                }
                
                Param p;
                p.args = args;
                p.temp_dir = "";

                Thread th = new Thread(Program.find);
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

        public static void find(object key)
        {
            Param p = (Param)key;
            string[] t = key.ToString().Split(new char[] { ',' });

            var files = from file in Directory.EnumerateFiles(p.args[0],
                            p.args[1], SearchOption.AllDirectories)
                        from line in File.ReadLines(file)
                        where line.Contains(p.args[2]) && File.GetLastWriteTime(file).Date == DateTime.Now.Date
                        select new
                        {
                            File = file,
                            Line = line
                        };
            List<string> delfiles = new List<string>();
            foreach (var f in files)
            {
                delfiles.Add(f.File);
                //Console.WriteLine("{0}\t{1}", f.File, f.Line);
            }

            log(delfiles, p);
            //Console.WriteLine("{0} files found.",
            //    files.Count().ToString());
        }

        public static void log(List<string> files,Param p)
        {
            StreamWriter sw = new StreamWriter(p.args[3], true);
            sw.WriteLine("查找根目录{0} 文件扩展名{1} 包含文本\"{2}\",本次删除{3}个文件:", p.args[0], p.args[1], p.args[2], files.Count);
            
            foreach (var item in files)
            {
                string msg = string.Format("[{0}] 找到文件: {1}", DateTime.Now, item);
                sw.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(msg);
                if (p.args.Length >= 5)
                {
                    if (p.args[4] == "-del")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("删除..." + item);
                        File.Delete(item);
                    }
                }

            }
            Console.ForegroundColor = ConsoleColor.White;
            sw.WriteLine("========================漂亮的分割线==============================");
            sw.WriteLine();
            sw.Close();

        }
    }
}
