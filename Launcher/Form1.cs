using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Net;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Ionic.Zip;
using System.Threading;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Drawing.Text;


namespace Launcher
{

    public partial class Form1 : Form
    {

        //старт Form1
        public Form1()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            InitializeComponent();

            //сверка версии лаунчера с версией на сайте
            /*
            for (int n = 0; n < Global.LauncherVersion.Length; n++)
            {
                if (Global.LauncherVersion[n] != '.' && Global.LauncherVersion[n] != 'v')
                {
                    Global.LauncherCheckVersion *= 10;
                    Global.LauncherCheckVersion += (Global.LauncherVersion[n]) - 48;
                }
            }
            if (Global.LauncherCheckVersion < 1000)
            {
                while (Global.LauncherCheckVersion < 1000)
                {
                    Global.LauncherCheckVersion *= 10;
                }
            }
             */
            //*** сверка версии лаунчера с версией на сайте            

            this.Text = this.Text + " " + Global.LauncherVersion;
            DebugLog.Items.Add("Ghostik Launcher " + Global.LauncherVersion + " запущен (" + DateTime.Now + ")");


            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\Launcher_Options.xml"))
            {
                XmlTextReader reader = new XmlTextReader(System.IO.Directory.GetCurrentDirectory() + "\\Launcher_Options.xml");
                while (reader.Read())
                {
                    if (reader.IsStartElement("Nick") && !reader.IsEmptyElement)
                    {
                        Global.Nick = reader.ReadString();
                    }
                    if (reader.IsStartElement("GameFolder") && !reader.IsEmptyElement)
                    {
                        Global.Directory = reader.ReadString();
                    }
                    if (reader.IsStartElement("JavaFile") && !reader.IsEmptyElement)
                    {
                        Global.JavaPath = reader.ReadString();
                    }
                    if (reader.IsStartElement("Visibly") && !reader.IsEmptyElement)
                    {
                        Global.Visib = reader.ReadString();
                    }
                    if (reader.IsStartElement("Version") && !reader.IsEmptyElement)
                    {
                        Global.VersionN = reader.ReadString();

                        Global.Version = Global.VersionN.Replace("Release: ", "");
                        Global.Version = Global.Version.Replace("Snapshot: ", "");
                        Global.Version = Global.Version.Replace("Old Beta: ", "");
                        Global.Version = Global.Version.Replace("Old Alpha: ", "");
                    }
                    if (reader.IsStartElement("Memory") && !reader.IsEmptyElement)
                    {
                        Global.Memory = reader.ReadString();
                    }
                    if (reader.IsStartElement("ReleaseActive") && !reader.IsEmptyElement)
                    {
                        Global.RelCheck = bool.Parse(reader.ReadString());
                    }
                    if (reader.IsStartElement("SnapshotActive") && !reader.IsEmptyElement)
                    {
                        Global.SnapCheck = bool.Parse(reader.ReadString());
                    }
                    if (reader.IsStartElement("BetaActive") && !reader.IsEmptyElement)
                    {
                        Global.BetaCheck = bool.Parse(reader.ReadString());
                    }
                    if (reader.IsStartElement("AlphaActive") && !reader.IsEmptyElement)
                    {
                        Global.AlphaCheck = bool.Parse(reader.ReadString());
                    }
                    if (reader.IsStartElement("CustomActive") && !reader.IsEmptyElement)
                    {
                        Global.UncommonCheck = bool.Parse(reader.ReadString());
                    }
                }
                reader.Close();

                if (Global.Visib == "Закрыть после запуска")
                    Global.Vis = 1;
                if (Global.Visib == "Всегда открыт")
                    Global.Vis = 0;

                DebugLog.Items.Add("Настройки успешно загружены (" + System.IO.Directory.GetCurrentDirectory() + "\\Launcher_Options.xml)");
            }
            else
                DebugLog.Items.Add("Настройки не были загружены (не найден файл: " + System.IO.Directory.GetCurrentDirectory() + "\\Launcher_Options.xml)");

            if (Global.Nick.Length > 16)
            {
                DebugLog.Items.Add("Ник в файле настроек слишком длинный! (более 16 символов)");
                Global.Nick = Global.Nick.Substring(0, 16);
            }

            if (ConnectionAvailable("http://s3.amazonaws.com"))
            {
                DebugLog.Items.Add("Соединение с интернетом присутствует");
                System.Net.WebClient connection = new System.Net.WebClient();

                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Nothing));
                string fileContent = connection.DownloadString("http://s3.amazonaws.com/Minecraft.Download/versions/versions.json");
                Nothing vers = (Nothing)json.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(fileContent)));

                Global.Latest = vers.LT.release;
                Global.LatestSnap = vers.LT.snapshoot;

                if (Global.Version == "Последняя стабильная версия")
                    Global.Version = Global.Latest;
                if (Global.Version == "Последний снапшот")
                    Global.Version = Global.LatestSnap;

                for (int i = 0; i < vers.VS.Length; i++)
                {
                    if (vers.VS[i].Type == "release")
                    {
                        Global.Releases[Global.ReleaseInt] = "Release: " + vers.VS[i].Id;
                        Global.ReleaseInt++;
                    }
                    if (vers.VS[i].Type == "snapshot")
                    {
                        Global.Snapshots[Global.SnapshotInt] = "Snapshot: " + vers.VS[i].Id;
                        Global.SnapshotInt++;
                    }
                    if (vers.VS[i].Type == "old_beta")
                    {
                        Global.Beta[Global.BetaInt] = "Old Beta: " + vers.VS[i].Id;
                        Global.BetaInt++;
                    }
                    if (vers.VS[i].Type == "old_alpha")
                    {
                        Global.Alpha[Global.AlphaInt] = "Old Alpha: " + vers.VS[i].Id;
                        Global.AlphaInt++;
                    }
                }

                //custom
                try
                {
                    if (Directory.Exists(Global.Directory + "\\versions\\"))
                    {
                        int folderCount = 0;


                        DirectoryInfo dir = new DirectoryInfo(Global.Directory + "\\versions\\");
                        foreach (var item in dir.GetDirectories())
                        {
                            if (File.Exists(Global.Directory + "\\versions\\" + item.Name + "\\" + item.Name + ".jar"))
                            {
                                if (File.Exists(Global.Directory + "\\versions\\" + item.Name + "\\" + item.Name + ".json"))
                                {
                                    DataContractJsonSerializer json3 = new DataContractJsonSerializer(typeof(JSONN));
                                    string fileContent3 = File.ReadAllText(Global.Directory + "\\versions\\" + item.Name + "\\" + item.Name + ".json");
                                    JSONN jsonn3 = (JSONN)json3.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(fileContent3)));

                                    if (jsonn3.ID == item.Name)
                                    {
                                        int i;
                                        for (i = 0; i < vers.VS.Length; i++)
                                        {
                                            if (vers.VS[i].Id == item.Name)
                                                break;
                                        }
                                        if (i >= vers.VS.Length)
                                        {
                                            Global.Uncommon[folderCount] = item.Name;
                                            Global.UncommonInt++;
                                            folderCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                        DebugLog.Items.Add("Не удалось собрать информацию с \\versions\\ (папка отсутствует)");
                }
                catch { }
                //Custom
            }
            else
            {
                DebugLog.Items.Add("(!) Соединение с интернетом не обнаружено, возможно не удастся скачать файлы (!)");
                DebugLog.Items.Add("(!) Все существующие версии доступны в Custom  (!)");
                Global.UncommonCheck = true;
                CheckVersionsFolderOffline();
            }
            DebugLog.Items.Add(String.Empty);
            DebugLog.Items.Add("******");
            DebugLog.Items.Add(String.Empty);
            DebugLog.Items.Add("Во избежание ошибок не взаимодействуйте с файлами игры во время работы лаунчера!!!");
            DebugLog.Items.Add("При проверке файлов лаунчер может временно не отвечать. Просто подождите.");
            DebugLog.Items.Add("При возникновении ошибок сохраняйте лог и присылайте его на сайт автору лаунчера");
            DebugLog.Items.Add(String.Empty);
            if (Global.Nick == String.Empty || Global.Version == String.Empty || Global.Directory == String.Empty || Global.JavaPath == String.Empty || Global.Visib == String.Empty)
            {
                DebugLog.Items.Add("Зайдите в настройки и заполните пустующие поля!");
            }
            else
            {
                DebugLog.Items.Add("Minecraft v." + Global.Version + " будет проверен и запущен. Ваш ник: " + Global.Nick);
            }

        }
        //*** старт Form1


        //кнопка начать игру
        private void button1_Click(object sender, EventArgs e)
        {
            //очистка параметров запуска для дальнейшего заполнения
            //сброс настроек запуска lib - проверка библиотек, downloadInt - количество файлов на загрузку,
            // intList - текущий скачиваемый файл, unzipInt - кол-во архивов на распаковку, currentUnzipInt - текущий распаковываемый архив
            noExitMessageClone = false;
            jarjson = false;
            lib = false;
            asse = false;
            downloadInt = 0;
            intList = 0;
            unzipInt = 0;
            curentUnzipInt = 0;
            Global.StartLibraries = String.Empty;
            Global.MainClass = String.Empty;
            Global.MinecraftArg = String.Empty;
            Global.AssetsIndex = String.Empty;
            ready = new AutoResetEvent(false);
            go = new AutoResetEvent(false);
            path = null;
            //

            if (!Directory.Exists(Global.Directory))
            {
                Directory.CreateDirectory(Global.Directory);
                DebugLog.Items.Add("Создана папка " + Global.Directory);
            }

            if (!File.Exists(Global.Directory + "\\launcher_profiles.json"))
            {
                using (System.IO.StreamWriter file = System.IO.File.CreateText(Global.Directory + "\\launcher_profiles.json"))
                {
                    file.WriteLine("{\"profiles\": {}, \"clientToken\": \"f9123200-c659-46a6-b333-b44725f1a546\"}");
                }
            }

            if (Global.Nick == String.Empty || Global.Version == String.Empty || Global.Directory == String.Empty || Global.JavaPath == String.Empty || Global.Visib == String.Empty)
            {
                DebugLog.Items.Add("Зайдите в настройки и заполните пустующие поля!");
                if (Global.Nick == String.Empty)
                    DebugLog.Items.Add("Ошибка: Не указан ник" + " (" + DateTime.Now + ")");
                if (Global.Version == String.Empty)
                    DebugLog.Items.Add("Ошибка: Не указана версия Minecraft" + " (" + DateTime.Now + ")");
                if (Global.Directory == String.Empty)
                    DebugLog.Items.Add("Ошибка: Не указана папка Minecraft" + " (" + DateTime.Now + ")");
                if (Global.JavaPath == String.Empty)
                    DebugLog.Items.Add("Ошибка: Не указан путь к javaw.exe" + " (" + DateTime.Now + ")");
                if (Global.Visib == String.Empty)
                    DebugLog.Items.Add("Ошибка: Не указано отображение лаунчера" + " (" + DateTime.Now + ")");
            }
            else
            {
                button1.Enabled = false;

                if (!Directory.Exists(Global.Directory + "\\versions\\"))
                {
                    Directory.CreateDirectory(Global.Directory + "\\versions\\");
                    DebugLog.Items.Add("Создана папка \\versions\\");
                }
                if (!Directory.Exists(Global.Directory + "\\versions\\" + Global.Version))
                {
                    Directory.CreateDirectory(Global.Directory + "\\versions\\" + Global.Version);
                    DebugLog.Items.Add("Создана папка \\versions\\" + Global.Version + "\\");
                }
                if (!Directory.Exists(Global.Directory + "\\assets\\"))
                {
                    Directory.CreateDirectory(Global.Directory + "\\assets\\");
                    DebugLog.Items.Add("Создана папка \\assets\\");
                }
                if (!Directory.Exists(Global.Directory + "\\assets\\indexes\\"))
                {
                    Directory.CreateDirectory(Global.Directory + "\\assets\\indexes\\");
                    DebugLog.Items.Add("Создана папка \\assets\\indexes\\");
                }
                if (!Directory.Exists(Global.Directory + "\\assets\\objects\\"))
                {
                    Directory.CreateDirectory(Global.Directory + "\\assets\\objects\\");
                    DebugLog.Items.Add("Создана папка \\assets\\objects\\");
                }
                if (!Directory.Exists(Global.Directory + "\\versions\\" + Global.Version + "\\natives\\"))
                {
                    Directory.CreateDirectory(Global.Directory + "\\versions\\" + Global.Version + "\\natives\\");
                    DebugLog.Items.Add("Создана папка \\versions\\" + Global.Version + "\\natives\\");
                }
                //Скачивание основных .jar и .json
                if (!File.Exists(Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".jar"))
                {
                    DownloadList("http://s3.amazonaws.com/Minecraft.Download/versions/" + Global.Version + "/" + Global.Version + ".jar", Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".jar");
                    if (!jarjson)
                        jarjson = true;
                }

                if (!File.Exists(Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".json"))
                {
                    DownloadList("http://s3.amazonaws.com/Minecraft.Download/versions/" + Global.Version + "/" + Global.Version + ".json", Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".json");
                    if (!jarjson)
                        jarjson = true;
                }

                if (jarjson)
                {
                    DownloadFile();
                }
                else
                {
                    //проверка библиотек
                    if (!lib)
                        CheckLibraries();
                    else
                        if (unzipInt > 0)
                            Unzip();
                        else
                            if (!asse)
                                CheckAssets();
                            else
                                MineStart();
                }
            }
        }
        //*** кнопка начать игру

        //Проверка кастомных версий
        private void CheckVersionsFolderOffline()
        {
            try
            {
                int folderCount = 0;

                if (Directory.Exists(Global.Directory + "\\versions\\"))
                {
                    DirectoryInfo dir = new DirectoryInfo(Global.Directory + "\\versions\\");
                    foreach (var item in dir.GetDirectories())
                    {
                        if (File.Exists(Global.Directory + "\\versions\\" + item.Name + "\\" + item.Name + ".jar"))
                        {
                            if (File.Exists(Global.Directory + "\\versions\\" + item.Name + "\\" + item.Name + ".json"))
                            {
                                DataContractJsonSerializer json3 = new DataContractJsonSerializer(typeof(JSONN));
                                string fileContent3 = File.ReadAllText(Global.Directory + "\\versions\\" + item.Name + "\\" + item.Name + ".json");
                                JSONN jsonn3 = (JSONN)json3.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(fileContent3)));

                                if (jsonn3.ID == item.Name)
                                {
                                    Global.Uncommon[folderCount] = item.Name;
                                    Global.UncommonInt++;
                                    folderCount++;
                                }
                            }
                        }
                    }
                }
                else
                    DebugLog.Items.Add("Не удалось собрать информацию с \\versions\\ (папка отсутствует)");
            }
            catch { }
        }
        //*** Проверка кастомных версий

        //получение хэша 
        /*
        private string ComputeMD5Checksum(string path, bool web)
        {           
            if (web)
            {
                WebClient wc = new WebClient();
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    string result = BitConverter.ToString(md5.ComputeHash(wc.DownloadData(path)));
                    return result.Replace("-", String.Empty);
                }
            }
            else
            {
                FileStream fs = File.OpenRead(path);
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, (int)fs.Length);
                    byte[] checkSum = md5.ComputeHash(fileData);
                    string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                    return result;
                }
            }
        } */
        //*** получение хэша

        //список файлов на скачивание
        public string[] urlList = new string[10240];
        public string[] pathList = new string[10240];
        public int downloadInt = 0;
        public int intList = 0;
        public bool lib = false;
        public bool jarjson = false;

        private void DownloadList(string url, string path)
        {
            urlList[downloadInt] = url;
            pathList[downloadInt] = path;
            downloadInt++;
        }
        //*** список файлов на скачивание

        //скачивание        
        private void DownloadFile()
        {
            try
            {
                if (ConnectionAvailable(urlList[intList]))
                {
                    WebClient client = new WebClient();
                    Uri url;
                    url = new Uri(urlList[intList]);
                    string path = pathList[intList];
                    if (!progressBar1.Visible)
                    {
                        progressBar1.Visible = true;
                        label1.Visible = true;
                        label2.Visible = true;
                    }
                    client.DownloadFileAsync(url, path);
                    client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                    DebugLog.Items.Add("Скачивание " + url);
                    sw.Start();
                    label1.Text = "Прогресс скачивания:" + "\n" + "осталось: " + (downloadInt - intList) + "\n" + "всего: " + downloadInt;
                }
                else
                {
                    DebugLog.Items.Add(String.Empty);
                    DebugLog.Items.Add("Не удалось скачать " + urlList[intList]);
                    DebugLog.Items.Add("Файл не найден");
                    DebugLog.Items.Add("Не удастся запустить игру. Прерываем процесс проверки.");
                    button1.Enabled = true;
                }
            }
            catch
            {
                DebugLog.Items.Add(String.Empty);
                DebugLog.Items.Add("Не удалось скачать " + urlList[intList]);
                DebugLog.Items.Add("Не удастся запустить игру. Прерываем процесс проверки.");
                button1.Enabled = true;
            }
        }
        //*** скачивание

        //завершение скачивания
        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            sw.Reset();
            DebugLog.Items.Add("Скачан файл: " + urlList[intList]);
            if (intList < downloadInt - 1)
            {
                intList++;
                DownloadFile();
            }
            else
            {
                intList++;
                if (jarjson)
                {
                    intList = 0;
                    downloadInt = 0;
                }
                if (!lib)
                    CheckLibraries();
                else
                    if (unzipInt > 0)
                        Unzip();
                    else
                        if (!asse)
                            CheckAssets();
                        else
                            MineStart();
            }
        }
        //*** завершение скачивания

        //процент загрузки
        Stopwatch sw = new Stopwatch();
        void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label2.Text = string.Format("скорость: {0} кб/с", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
        }
        //*** процент загрузки

        //проверка Libraries
        private void CheckLibraries()
        {
            DebugLog.Items.Add(String.Empty);
            DebugLog.Items.Add("Идет проверка libraries. Это может занять некоторое время.");

            if (File.Exists(Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".json"))
            {

                if (!Directory.Exists(Global.Directory + "\\libraries\\"))
                {
                    Directory.CreateDirectory(Global.Directory + "\\libraries\\");
                    DebugLog.Items.Add("Создана папка \\libraries\\");
                }

                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSONN));
                string fileContent = File.ReadAllText(Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".json");
                JSONN jsonn = (JSONN)json.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(fileContent)));

                int i;
                for (i = 0; i < jsonn.Lib.Length; i++)
                {
                gO:
                    try
                    {
                        if (jsonn.Lib[i].Rul.Length > 0)
                        {
                            for (int okay = 0; okay < jsonn.Lib[i].Rul.Length; okay++)
                            {
                                if ((jsonn.Lib[i].Rul[okay].Action == "allow" && jsonn.Lib[i].Rul[okay].oss.Name != "windows") || (jsonn.Lib[i].Rul[okay].Action == "disallow" && jsonn.Lib[i].Rul[okay].oss.Name == "windows"))
                                {
                                    i++;
                                    goto gO;
                                }
                            }
                        }
                    }
                    catch { }

                    string libFileName = jsonn.Lib[i].Name;
                    string filedir, filedir2, filePath = "";
                    int j;
                    for (j = 0; j < libFileName.Length; j++)
                    {
                        if (libFileName[j] == ':')
                        {
                            libFileName = libFileName.Remove(j);
                            break;
                        }
                    }

                    filedir = libFileName.Replace(".", "\\");
                    filedir2 = jsonn.Lib[i].Name.Substring(j, jsonn.Lib[i].Name.Length - j);
                    filedir += filedir2.Replace(":", "\\");
                    filedir += "\\";
                    filedir2 = filedir2.Substring(1, filedir2.Length - 1);
                    filedir2 = filedir2.Replace(":", "-");
                    if (!Directory.Exists(Global.Directory + "\\libraries\\" + filedir))
                    {
                        Directory.CreateDirectory(Global.Directory + "\\libraries\\" + filedir);
                        DebugLog.Items.Add("Создана папка \\libraries\\" + filedir);
                    }
                    try
                    {
                        if (jsonn.Lib[i].NT.Windows == "natives-windows-${arch}")
                        {
                            if (Environment.Is64BitOperatingSystem)
                            {
                                jsonn.Lib[i].NT.Windows = "natives-windows-64";
                            }
                            else
                            {
                                jsonn.Lib[i].NT.Windows = "natives-windows-32";
                            }
                        }
                        if (jsonn.Lib[i].NT.Windows != null)
                        {
                            filePath = filedir + filedir2 + "-" + jsonn.Lib[i].NT.Windows + ".jar";
                        }
                    }
                    catch
                    {
                        filePath = filedir + filedir2 + ".jar";
                    }
                    if (File.Exists(Global.Directory + "\\libraries\\" + filePath))
                    {
                        try
                        {
                            if (jsonn.Lib[i].Ex.Exclude.Length > 0)
                            {
                                using (ZipFile zip = ZipFile.Read(Global.Directory + "\\libraries\\" + filePath))
                                {
                                    foreach (ZipEntry ze in zip)
                                    {
                                        if (!File.Exists(Global.Directory + "\\versions\\" + Global.Version + "\\natives\\" + ze.FileName) && ze.FileName != "META-INF/" && ze.FileName != "META-INF/MANIFEST.MF")
                                        {
                                            UnzipList(Global.Directory + "\\libraries\\" + filePath);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            if (jsonn.Lib[i].Ex.Exclude.Length > 0)
                            {
                                UnzipList(Global.Directory + "\\libraries\\" + filePath);
                            }
                        }
                        catch { }
                    }
                    if (!File.Exists(Global.Directory + "\\libraries\\" + filePath))
                    {
                        if (jsonn.Lib[i].Url != null)
                        {
                            DownloadList(jsonn.Lib[i].Url + filePath.Replace("\\", "/"), Global.Directory + "\\libraries\\" + filePath);
                            DebugLog.Items.Add("Необходима библиотека: " + "\\libraries\\" + filePath);
                        }
                        else
                        {
                            DownloadList("https://libraries.minecraft.net/" + filePath.Replace("\\", "/"), Global.Directory + "\\libraries\\" + filePath);
                            DebugLog.Items.Add("Необходима библиотека: " + "\\libraries\\" + filePath);
                        }
                    }
                    //создание списка библиотек для запуска
                    Global.StartLibraries = Global.StartLibraries + Global.Directory + "\\libraries\\" + filePath + ";";

                    //mainClass из "Global.Version".json
                    if (Global.MainClass == String.Empty)
                        Global.MainClass = jsonn.MainClass;

                    //читаем версию assets                
                    if (Global.AssetsIndex == String.Empty)
                    {
                        if (jsonn.Assets != null)
                            Global.AssetsIndex = jsonn.Assets;
                        else
                            Global.AssetsIndex = "legacy";
                    }

                    //преобразуем minecraft arguments
                    if (Global.MinecraftArg == String.Empty)
                    {
                        MD5 md5Hasher = MD5.Create();
                        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(Global.Nick));
                        Guid newuuid = new Guid(data);

                        Global.MinecraftArg = jsonn.MinecraftArguments;
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${auth_player_name}", Global.Nick);
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${version_name}", Global.Version);
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${game_directory}", Global.Directory);
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${assets_root}", Global.Directory + "\\assets");
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${assets_index_name}", Global.AssetsIndex);
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${auth_uuid}", newuuid.ToString()); //Guid.NewGuid().ToString()
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${auth_access_token}", "null");
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${user_properties}", "{}");
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${user_type}", "mojang");
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${auth_session}", "null");
                        Global.MinecraftArg = Global.MinecraftArg.Replace("${game_assets}", Global.Directory + "\\assets\\virtual\\legacy");
                    }
                }
                if (i >= jsonn.Lib.Length)
                    lib = true;

                if (!File.Exists(Global.Directory + "\\assets\\indexes\\" + Global.AssetsIndex + ".json"))
                {
                    DownloadList("https://s3.amazonaws.com/Minecraft.Download/indexes/" + Global.AssetsIndex + ".json", Global.Directory + "\\assets\\indexes\\" + Global.AssetsIndex + ".json");
                }

                if ((downloadInt - intList) > 0)
                    DownloadFile();
                else
                    if (unzipInt > 0)
                        Unzip();
                    else
                        if (!asse)
                            CheckAssets();
                        else
                            MineStart();
            }
        }
        //*** проверка libraries

        //список на распаковку natives
        public string[] unzipPathList = new string[1024];
        public int unzipInt = 0;

        private void UnzipList(string filePath)
        {
            unzipPathList[unzipInt] = filePath;
            unzipInt++;
        }
        //*** список на распаковку natives

        //распаковка natives
        public int curentUnzipInt;
        static EventWaitHandle ready = new AutoResetEvent(false);
        static EventWaitHandle go = new AutoResetEvent(false);
        static volatile string path = null;
        private void Unzip()
        {
            //DebugLog.Items.Add("Распаковка: " + unzipPathList[curentUnzipInt]);
            //Global.Directory + "\\versions\\" + Global.Version + "\\natives\\"

            new Thread(UnzipFiles).Start();

            for (curentUnzipInt = 0; curentUnzipInt < unzipInt; curentUnzipInt++)
            {
                ready.WaitOne();
                path = unzipPathList[curentUnzipInt];
                DebugLog.Items.Add("Распаковка: " + unzipPathList[curentUnzipInt]);
                go.Set();
            }
            ready.WaitOne();
            path = null;
            go.Set();

            unzipInt = 0;

            if (!asse)
                CheckAssets();
            else
                MineStart();
        }

        static void UnzipFiles()
        {

            while (true)
            {
                ready.Set();
                go.WaitOne();

                if (path == null)
                    return;

                Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(path);
                try { zip.ExtractAll(Global.Directory + "\\versions\\" + Global.Version + "\\natives\\", ExtractExistingFileAction.OverwriteSilently); }
                catch { }
                if (Directory.Exists(Global.Directory + "\\versions\\" + Global.Version + "\\natives\\META-INF"))
                {
                    File.Delete(Global.Directory + "\\versions\\" + Global.Version + "\\natives\\META-INF\\MANIFEST.MF");
                    Directory.Delete(Global.Directory + "\\versions\\" + Global.Version + "\\natives\\META-INF");
                }
            }
        }
        //*** распаковка natives

        //проверка assets
        public bool asse = false;
        private void CheckAssets()
        {
            DebugLog.Items.Add(String.Empty);
            DebugLog.Items.Add("Идет проверка assets. Это может занять много времени.");

            if (Global.AssetsIndex == "legacy" && !Directory.Exists(Global.Directory + "\\assets\\virtual\\"))
            {
                Directory.CreateDirectory(Global.Directory + "\\assets\\virtual\\");
                Directory.CreateDirectory(Global.Directory + "\\assets\\virtual\\legacy\\");
                DebugLog.Items.Add("Создана папка \\assets\\indexes\\legacy\\");
            }

            string str, str2;
            string assetsJson, assetsJson2;

            assetsJson = Global.Directory + "\\assets\\" + Global.AssetsIndex + ".json";
            assetsJson2 = Global.Directory + "\\assets\\" + Global.AssetsIndex + "2.json";

            #region Assets_downloadList;

            if (!File.Exists(Global.Directory + "\\assets\\" + Global.AssetsIndex + ".json"))
            {
                try
                {
                    File.Copy(Global.Directory + "\\assets\\indexes\\" + Global.AssetsIndex + ".json", Global.Directory + "\\assets\\" + Global.AssetsIndex + ".json");

                    using (System.IO.StreamReader reader = System.IO.File.OpenText(assetsJson))
                    {
                        str = reader.ReadToEnd();
                    }
                    str = str.Replace("  \"objects\": {", "  \"objects\": [");
                    str = str.Replace("  }", "  ]");
                    str = str.Replace("    ]", "    }");
                    str = str.Replace("    ],", "    },");

                    Regex r = new Regex("(    \".*.\": {)", RegexOptions.IgnoreCase);
                    str = r.Replace(str, "    {");
                    Console.WriteLine(str);

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(assetsJson))
                    {
                        file.Write(str);
                    }
                }
                catch { }
            }

            if (Global.AssetsIndex == "legacy" && !File.Exists(Global.Directory + "\\assets\\" + Global.AssetsIndex + "2.json"))
            {
                try
                {
                    File.Copy(Global.Directory + "\\assets\\indexes\\" + Global.AssetsIndex + ".json", Global.Directory + "\\assets\\" + Global.AssetsIndex + "2.json");

                    using (System.IO.StreamReader reader = System.IO.File.OpenText(assetsJson2))
                    {
                        str2 = reader.ReadToEnd();
                    }
                    str2 = str2.Replace("  \"objects\": {", "  \"objects\": [");
                    str2 = str2.Replace("  }", "  ]");
                    str2 = str2.Replace("    ]", "    }");
                    str2 = str2.Replace("    ],", "    },");
                    str2 = str2.Replace(": {\n", ",");

                    Regex r2 = new Regex("(      \".*.\",)", RegexOptions.IgnoreCase);
                    str2 = r2.Replace(str2, "del");
                    Regex r3 = new Regex("(      \".*)", RegexOptions.IgnoreCase);
                    str2 = r3.Replace(str2, "del");
                    Console.WriteLine(str2);

                    str2 = str2.Replace("del\ndel\n    },", "");
                    str2 = str2.Replace("del\ndel\n    }", "");

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(assetsJson2))
                    {
                        file.Write(str2);
                    }
                }
                catch { }
            }

            #endregion Assets_downloadList;

            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ASSETSS));
            string fileContent = File.ReadAllText(assetsJson);
            ASSETSS assetss = (ASSETSS)json.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(fileContent)));

            string shortName = String.Empty, fullName = String.Empty;
            int k;

            if (Global.AssetsIndex != "legacy")
            {

                for (k = 0; k < assetss.Obj.Length; k++)
                {
                    shortName = "" + (char)assetss.Obj[k].hash[0] + (char)assetss.Obj[k].hash[1];
                    fullName = assetss.Obj[k].hash;

                    if (!Directory.Exists(Global.Directory + "\\assets\\objects\\" + shortName + "\\"))
                    {
                        Directory.CreateDirectory(Global.Directory + "\\assets\\objects\\" + shortName + "\\");
                    }
                    if (!File.Exists(Global.Directory + "\\assets\\objects\\" + shortName + "\\" + fullName))
                    {
                        DownloadList("http://resources.download.minecraft.net/" + shortName + "/" + fullName, Global.Directory + "\\assets\\objects\\" + shortName + "\\" + fullName);
                    }
                }
            }
            else
            {
                DataContractJsonSerializer json1 = new DataContractJsonSerializer(typeof(ASSETSS_Legacy));
                string fileContent2 = File.ReadAllText(assetsJson2);
                ASSETSS_Legacy assetss_legacy = (ASSETSS_Legacy)json1.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(fileContent2)));

                string shortName2 = String.Empty, fullName2 = String.Empty;
                for (k = 0; k < assetss_legacy.Objects.Length; k++)
                {
                    shortName = String.Empty;
                    fullName = String.Empty;
                    fullName2 = String.Empty;
                    shortName2 = String.Empty;

                    shortName2 = "" + (char)assetss.Obj[k].hash[0] + (char)assetss.Obj[k].hash[1];
                    fullName2 = assetss.Obj[k].hash;

                    if ((assetss_legacy.Objects[k] != "pack.mcmeta") && (assetss_legacy.Objects[k] != "READ_ME_I_AM_VERY_IMPORTANT.txt") && (assetss_legacy.Objects[k] != "sounds.json"))
                    {
                        for (int t = assetss_legacy.Objects[k].Length - 1; t >= 0; t--)
                        {
                            if (assetss_legacy.Objects[k][t] == '/')
                            {
                                fullName = assetss_legacy.Objects[k].Substring(t + 1);
                                break;
                            }
                        }

                        shortName = assetss_legacy.Objects[k];
                        shortName = shortName.Replace(fullName, "");
                        shortName = shortName.Replace("/", "\\");

                        if (!Directory.Exists(Global.Directory + "\\assets\\virtual\\legacy\\" + shortName))
                            Directory.CreateDirectory(Global.Directory + "\\assets\\virtual\\legacy\\" + shortName);

                        if (!File.Exists(Global.Directory + "\\assets\\virtual\\legacy\\" + shortName + fullName))
                        {
                            DownloadList("http://resources.download.minecraft.net/" + shortName2 + "/" + fullName2, Global.Directory + "\\assets\\virtual\\legacy\\" + shortName + fullName);
                        }
                    }
                    else
                    {
                        fullName = assetss_legacy.Objects[k];

                        if (!File.Exists(Global.Directory + "\\assets\\virtual\\legacy\\" + fullName))
                        {
                            DownloadList("http://resources.download.minecraft.net/" + shortName2 + "/" + fullName2, Global.Directory + "\\assets\\virtual\\legacy\\" + fullName);
                        }
                    }
                }
            }

            if (k >= assetss.Obj.Length)
                asse = true;

            //File.Delete(assetsJson);
            //File.Delete(assetsJson2);

            if ((downloadInt - intList - 1) > 0)
                DownloadFile();
            else
                MineStart();
        }
        //*** проверка assets


        //запуск Minecraft
        System.Diagnostics.Process mc = new System.Diagnostics.Process();
        private void MineStart()
        {
            progressBar1.Visible = false;
            label1.Visible = false;
            label2.Visible = false;

            DebugLog.Items.Add("Попытка запуска Minecraft " + Global.Version);
            DebugLog.Items.Add(String.Empty);
            if (File.Exists(Global.JavaPath))           
            {
                
                ProcessStartInfo mcStartInfo = new ProcessStartInfo(Global.JavaPath, Global.Memory +

                                   " -Djava.library.path=\"" +

                                    Global.Directory + "\\versions\\" + Global.Version + "\\natives\"" + " -cp \"" +

                                    Global.StartLibraries +

                                    Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".jar\" " +

                                    Global.MainClass + " " +

                                    Global.MinecraftArg);

                mc.StartInfo = mcStartInfo;
                mc.EnableRaisingEvents = true;
                if (Global.Vis != 1)
                    mc.Exited += new EventHandler(mcProcess_Exited);
                mc.Start();
                noExitMessageClone = true;

                //проверка видимости лаунчера, если "1", то закрыть лаунчер; если "2", то скрыть
                if (Global.Vis == 1)
                    Application.Exit();
                if (Global.Vis == 2)
                    this.Hide();
            }
            else
            {
                DebugLog.Items.Add("Не найден Java по указанному в настройках пути");
                DebugLog.Items.Add("Запуск прерван");
            }
        }
        //*** запуск Minecraft 

        void mcProcess_Exited(object sender, System.EventArgs e)
        {
            //System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            Action<string> action = AddToListBox;
            if (DebugLog.InvokeRequired)
            {               
                Invoke(action, "Minecraft v." + Global.Version + " был завершен ExitCode: " + mc.ExitCode + ", ExitTime: " + mc.ExitTime + ")");
            }
            else
            {
                action("Minecraft v." + Global.Version + " был завершен (ExitCode: " + mc.ExitCode + ", ExitTime: " + mc.ExitTime + ")");
            }
        }
        bool noExitMessageClone;
        void AddToListBox(string msg)
        {
            if (noExitMessageClone)
            {
                if (Global.Vis == 2)
                    this.Show();
                noExitMessageClone = false;
                DebugLog.Items.Add(msg);
                button1.Enabled = true;
            }
        }

        //проверка соединения с инетом
        public bool ConnectionAvailable(string strServer)
        {
            try
            {
                HttpWebRequest reqFP = (HttpWebRequest)HttpWebRequest.Create(strServer);
                HttpWebResponse rspFP = (HttpWebResponse)reqFP.GetResponse();
                if (HttpStatusCode.OK == rspFP.StatusCode)
                {
                    rspFP.Close();
                    return true;
                }
                else
                {
                    rspFP.Close();
                    return false;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
        //*** проверка соединения с инетом

        //чтение assets json LEGACY
        [DataContract]
        public class ASSETSS_Legacy
        {
            [DataMember(Name = "virtual")]
            public bool Virtual { get; set; }
            [DataMember(Name = "objects")]
            public string[] Objects { get; set; }
        }
        //*** чтение assets json LEGACY

        //чтение assets json
        [DataContract]
        public class ASSETSS
        {
            [DataMember(Name = "objects")]
            public Objects[] Obj { get; set; }
        }
        [DataContract]
        public class Objects
        {
            [DataMember(Name = "hash")]
            public string hash { get; set; }
            [DataMember(Name = "size")]
            public string size { get; set; }
        }
        //*** чтение assets json

        //чтение versions.json
        [DataContract]
        public class Nothing
        {
            [DataMember(Name = "latest")]
            public Latest LT { get; set; }
            [DataMember(Name = "versions")]
            public Versions[] VS { get; set; }
        }
        [DataContract]
        public class Latest
        {
            [DataMember(Name = "snapshot")]
            public string snapshoot { get; set; }
            [DataMember(Name = "release")]
            public string release { get; set; }
        }
        [DataContract]
        public class Versions
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }
            [DataMember(Name = "time")]
            public string Time { get; set; }
            [DataMember(Name = "releaseTime")]
            public string ReleaseTime { get; set; }
            [DataMember(Name = "type")]
            public string Type { get; set; }
        }
        //*** чтение versions.json

        //чтение "Global.Version".json
        [DataContract]
        public class JSONN
        {
            [DataMember(Name = "id")]
            public string ID { get; set; }
            [DataMember(Name = "time")]
            public string Time { get; set; }
            [DataMember(Name = "releaseTime")]
            public string ReleaseTime { get; set; }
            [DataMember(Name = "type")]
            public string Type { get; set; }
            [DataMember(Name = "minecraftArguments")]
            public string MinecraftArguments { get; set; }
            [DataMember(Name = "minimumLauncherVersion")]
            public string MinimumLauncherVersion { get; set; }
            [DataMember(Name = "assets")]
            public string Assets { get; set; }
            [DataMember(Name = "libraries")]
            public Libraries[] Lib { get; set; }
            [DataMember(Name = "mainClass")]
            public string MainClass { get; set; }
        }
        [DataContract]
        public class Libraries
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
            [DataMember(Name = "url")]
            public string Url { get; set; }
            [DataMember(Name = "natives")]
            public Natives NT { get; set; }
            [DataMember(Name = "extract")]
            public Extract Ex { get; set; }
            [DataMember(Name = "rules")]
            public Rules[] Rul { get; set; }
        }
        [DataContract]
        public class Natives
        {
            [DataMember(Name = "linux")]
            public string Linux { get; set; }
            [DataMember(Name = "windows")]
            public string Windows { get; set; }
            [DataMember(Name = "osx")]
            public string OSX { get; set; }
        }
        [DataContract]
        public class Extract
        {
            [DataMember(Name = "exclude")]
            public string[] Exclude { get; set; }
        }
        [DataContract]
        public class Rules
        {
            [DataMember(Name = "action")]
            public string Action { get; set; }
            [DataMember(Name = "os")]
            public OS oss { get; set; }
        }
        [DataContract]
        public class OS
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
            [DataMember(Name = "version")]
            public string Version { get; set; }
        }
        //*** чтение "Global.Version".json

        //кнопка options
        private void button2_Click(object sender, EventArgs e)
        {
            if (Global.Opt == 0)
            {
                Global.Opt = 1;
                Form myform = new Form2();
                myform.Show();
            }
        }
        //*** кнопка options  

        private void button3_Click(object sender, EventArgs e)
        {
            string fileName = Global.Directory + "\\GhostikLauncher Logs\\DebugLog_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("HH-mm-ss") + ".txt";

            if (!Directory.Exists(Global.Directory + "\\GhostikLauncher Logs\\"))
                Directory.CreateDirectory(Global.Directory + "\\GhostikLauncher Logs\\");

            using (System.IO.StreamWriter file = System.IO.File.CreateText(fileName))
            {
                for (int i = 0; i < DebugLog.Items.Count; i++)
                {
                    file.WriteLine(DebugLog.Items[i]);
                }
                file.WriteLine("Лог сохранен в: " + fileName);
                file.WriteLine("");
                file.WriteLine("Nickname: " + Global.Nick);
                file.WriteLine("JavaPath: " + Global.JavaPath);
                file.WriteLine("JVM Arguments: " + Global.Memory);
                file.WriteLine("MainClass: " + Global.MainClass);
                file.WriteLine("Arguments: " + Global.MinecraftArg);
                file.WriteLine("Visibly: " + Global.Visib);                
                file.WriteLine("GameDir: " + Global.Directory);
                file.WriteLine("GamePath: " + Global.Directory + "\\versions\\" + Global.Version + "\\" + Global.Version + ".jar");                 
                file.WriteLine("Version: " + Global.VersionN + ", realName: " + Global.Version);
                file.WriteLine("Custom: " + Global.UncommonCheck + ", release: " + Global.RelCheck + ", alpha: " + Global.AlphaCheck + ", beta: " + Global.BetaCheck +", snapshot: " + Global.SnapCheck);
                file.WriteLine("");
                file.WriteLine("Libraries: " + Global.StartLibraries);
            }
            DebugLog.Items.Add("Лог сохранен в: " + fileName);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Global.Directory);
            }
            catch
            {
                DebugLog.Items.Add("Не удалось открыть папку игры");
            }
        }
    }
}
