using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Diary
{
    public class Program
    {
        public string dataPath = Environment.CurrentDirectory + "\\data.xml";

        public XDocument dataDoc;

        public static void Main(string[] args)
        {
            new Program().Start();
        }

        public void Start()
        {
            StartCheck();

            Console.CursorVisible = false;

            MainInterface();
        }

        private void MainInterface()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("*******************");
                Console.WriteLine("*                 *");
                Console.WriteLine("*    Diary v1.0   *");
                Console.WriteLine("*                 *");
                Console.WriteLine("*******************");
                Console.WriteLine();
                Console.WriteLine("1.写今日份的日记");
                Console.WriteLine("2.浏览过往日记");
                Console.WriteLine("任意键退出");

                Console.WriteLine();
                Console.Write("输入：");
                ConsoleKeyInfo c = Console.ReadKey();
                switch (c.Key)
                {
                    case ConsoleKey.D1:
                        WriteTodaysDiary();
                        dataDoc.Save(dataPath);
                        break;
                    case ConsoleKey.D2:
                        ReadDiary();
                        break;
                    default: return;
                }
            }
        }

        public void StartCheck()
        {

            if (!File.Exists(dataPath))
            {
                dataDoc = new XDocument();
                dataDoc.Add(new XElement("root"));
                dataDoc.Root.Add(new XElement("config"));
                dataDoc.Root.Element("config").Add(new XElement("unsaved-diary"));
                dataDoc.Root.Add(new XElement("diaries"));
                dataDoc.Save(dataPath);
            }
            else dataDoc = XDocument.Load(dataPath);

            string unsavedDiaryPath = dataDoc.Root.Element("config").Element("unsaved-diary").Value;
            if (File.Exists(unsavedDiaryPath))
                File.Delete(unsavedDiaryPath);
        }

        public void WriteTodaysDiary()
        {
            //string newFilePath = Environment.CurrentDirectory + "\\diary-" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm") + ".txt";

            string newFileId = Guid.NewGuid().ToString("N");
            string date = DateTime.Now.ToString("yyyy_MM_dd");
            string time = DateTime.Now.ToString("HH_mm");

            string newFilePath = Environment.CurrentDirectory + "\\" + newFileId + ".txt";

            dataDoc.Root.Element("config").Element("unsaved-diary").Value = newFilePath;
            dataDoc.Save(dataPath);

            OpenFile(newFilePath);

            if (File.Exists(newFilePath))
            {
                StreamReader reader = new StreamReader(newFilePath, Encoding.UTF8);
                StringBuilder builder = new StringBuilder();
                string temp;
                while ((temp = reader.ReadLine()) != null)
                {
                    builder.Append(temp);
                    builder.Append('\n');
                }
                reader.Close();

                XElement element;
                element = new XElement(newFileId);
                element.SetAttributeValue("date",date);
                element.SetAttributeValue("time", time);

                element.Value = builder.ToString();
                dataDoc.Root.Element("diaries").Add(element);
                dataDoc.Save(dataPath);

                File.Delete(newFilePath);
            }
        }

        private void ReadDiary()
        {
            while (true)
            {
                bool leave =true;

                Console.Clear();
                Console.WriteLine("*******************");
                Console.WriteLine("日记列表");
                Console.WriteLine("*******************");
                Console.WriteLine();
                XElement e = dataDoc.Root.Element("diaries");

                List<string> list = new List<string>();
                int count = 0;
                foreach (var item in e.Elements())
                {
                    Console.WriteLine(count + ". " + item.Name.ToString());
                    count++;
                    list.Add(item.Name.ToString());
                }
                Console.WriteLine();
                Console.WriteLine("输入序号按回车查看日记，输入q退出");

                int result = -1;
                while (true)
                {
                    Console.WriteLine();
                    Console.Write("输入：");
                    string command = Console.ReadLine();

                    if (command == "q")
                    {
                        return;
                    }
                    else if (!int.TryParse(command, out result))
                    {
                        Console.WriteLine("输入错误，请重新输入");
                        continue;
                    }
                    else if (result < 0 || result >= list.Count)
                    {
                        Console.WriteLine("输入错误，请重新输入");
                        continue;
                    }

                    break;
                }

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("****************************************");
                    Console.WriteLine("左右键翻页，c键修改，d键删除，r键返回，q键退出");
                    Console.WriteLine("****************************************");
                    Console.WriteLine();
                    Console.WriteLine("日期: " + list[result]);
                    Console.WriteLine();
                    Console.WriteLine();

                    Console.Write(e.Element(list[result]).Value);

                    Console.WriteLine();

                    Console.SetCursorPosition(0,0);

                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Q)
                    {
                        break;
                    }
                    else if (key.Key == ConsoleKey.C)
                    {
                        Modify(e.Element(list[result]));
                        break;
                    }
                    else if (key.Key == ConsoleKey.R)
                    {
                        leave = false ;
                        break;
                    }
                    else if (key.Key == ConsoleKey.D)
                    {
                        e.Element(list[result]).Remove();
                        dataDoc.Save(dataPath);
                        list.RemoveAt(result);
                        if (result == list.Count) result--;
                        if (list.Count == 0) break;
                    }
                    else if (key.Key == ConsoleKey.LeftArrow)
                    {
                        result--;
                        result = result < 0 ? 0 : result;
                    }
                    else if (key.Key == ConsoleKey.RightArrow)
                    {
                        result++;
                        result = result >= list.Count ? list.Count - 1 : result;
                    }
                }

                if (leave) break;
            }
        }

        public void Modify(XElement e)
        {
            string path = Environment.CurrentDirectory + "\\" + e.Name + ".txt";
            if (File.Exists(path)) File.Delete(path);
            File.Create(path).Close();
            StreamWriter sw = new StreamWriter(path);
            foreach (var item in e.Value)
                sw.Write(item);
            sw.Close();
            dataDoc.Root.Element("config").Element("unsaved-diary").Value = path;
            dataDoc.Save(dataPath);

            OpenFile(path);
        }
        public static void OpenFile(string path)
        {
            Process p = new Process();
            p.StartInfo.FileName = "E:\\Programs\\Vim\\vim82\\vim.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.Arguments = path;
            p.Start();
            p.WaitForExit();
            p.Close();
        }
    }
}
