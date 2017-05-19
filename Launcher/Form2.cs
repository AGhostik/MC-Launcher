using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

namespace Launcher
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            if (Global.JavaPath == String.Empty)
                Global.JavaPath = findJava(); 

            if (textBox1.Text == String.Empty)
                textBox1.Text = Global.Nick;

            if (textBox2.Text == String.Empty)
                textBox2.Text = Global.Directory;

            if (textBox3.Text == String.Empty)
                textBox3.Text = Global.JavaPath;

            if (textBox4.Text == String.Empty)
                textBox4.Text = Global.Memory;

            if (comboBox1.Text == String.Empty)
                comboBox1.Text = Global.Visib;

            checkBox1.Checked = Global.RelCheck;
            checkBox2.Checked = Global.SnapCheck;
            checkBox3.Checked = Global.BetaCheck;
            checkBox4.Checked = Global.AlphaCheck;
            checkBox5.Checked = Global.UncommonCheck;

            //ChangeMine();

            if (comboBox2.Text == String.Empty)
                comboBox2.Text = Global.VersionN;
        }

        private string findJava()
        {
            string javaKey = @"SOFTWARE\JavaSoft\Java Runtime Environment";
            string javaHome = null;
            using (var baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(javaKey))
            {
                string currentVersion;
                try
                {
                    currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                }
                catch
                {
                    return "Java not found";
                }
                javaKey = String.Format(@"{0}\{1}", javaKey, currentVersion);
                using (var homeKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(javaKey))
                {
                    javaHome = homeKey.GetValue("JavaHome").ToString();
                }
            }

            return javaHome + "\\bin\\javaw.exe";
        }

        private void ChangeMine()
        {
            if (Global.UncommonCheck == true)
                for (int i = 0; i < Global.UncommonInt; i++)
                {
                    if (Global.Uncommon[i] != null)
                        comboBox2.Items.Add(Global.Uncommon[i]);
                }

            if (Global.Latest != null && Global.RelCheck)
                comboBox2.Items.Add("Последняя стабильная версия");

            if (Global.RelCheck == true)
                for (int i = 0; i < Global.ReleaseInt; i++)
                {
                    if (Global.Releases[i] != null)
                        comboBox2.Items.Add(Global.Releases[i]);
                }

            if (Global.LatestSnap != null && Global.SnapCheck)
                comboBox2.Items.Add("Последний снапшот");

            if (Global.SnapCheck == true)
                for (int i = 0; i < Global.SnapshotInt; i++)
                {
                    if (Global.Snapshots[i] != null)
                        comboBox2.Items.Add(Global.Snapshots[i]);
                }
            if (Global.BetaCheck == true)
                for (int i = 0; i < Global.BetaInt; i++)
                {
                    if (Global.Beta[i] != null)
                        comboBox2.Items.Add(Global.Beta[i]);
                }
            if (Global.AlphaCheck == true)
                for (int i = 0; i < Global.AlphaInt; i++)
                {
                    if (Global.Alpha[i] != null)
                        comboBox2.Items.Add(Global.Alpha[i]);
                }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            if ((e.KeyChar >= '0') && (e.KeyChar <= '9'))
            {
                return;
            }
            if (((e.KeyChar >= 'a') && (e.KeyChar <= 'z')) || ((e.KeyChar >= 'A') && (e.KeyChar <= 'Z')))
            {
                return;
            }
            if (e.KeyChar == '_')
            {
                return;
            }

            if (Char.IsControl(e.KeyChar))
            {
                return;
            }
            // остальные символы запрещены
            e.Handled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == String.Empty || textBox2.Text == String.Empty || textBox3.Text == String.Empty || comboBox1.Text == String.Empty || comboBox2.Text == String.Empty)
            {
                labelWarn.Text = "Заполните все поля!";
            }
            else
            {
                if (File.Exists(textBox3.Text))
                {
                    Global.Nick = textBox1.Text;
                    Global.Directory = textBox2.Text;
                    Global.VersionN = comboBox2.Text;
                    Global.Visib = comboBox1.Text;
                    Global.Memory = textBox4.Text;
                    Global.JavaPath = textBox3.Text;
                    Global.RelCheck = checkBox1.Checked;
                    Global.SnapCheck = checkBox2.Checked;
                    Global.BetaCheck = checkBox3.Checked;
                    Global.AlphaCheck = checkBox4.Checked;
                    Global.UncommonCheck = checkBox5.Checked;

                    Global.Version = Global.VersionN.Replace("Release: ", "");
                    Global.Version = Global.Version.Replace("Snapshot: ", "");
                    Global.Version = Global.Version.Replace("Old Beta: ", "");
                    Global.Version = Global.Version.Replace("Old Alpha: ", "");

                    if (comboBox2.Text == "Последняя стабильная версия")
                        Global.Version = Global.Latest;
                    if (comboBox2.Text == "Последний снапшот")
                        Global.Version = Global.LatestSnap;

                    if (Global.Visib == "Закрыть после запуска")
                        Global.Vis = 1;
                    if (Global.Visib == "Всегда открыт")
                        Global.Vis = 0;
                    if (Global.Visib == "Скрыть на время запуска")
                        Global.Vis = 2;

                    XmlDocument xmlDoc = new XmlDocument();
                    XmlNode rootNode = xmlDoc.CreateElement("Options");
                    xmlDoc.AppendChild(rootNode);

                    XmlNode userNode;

                    userNode = xmlDoc.CreateElement("Nick");
                    userNode.InnerText = Global.Nick;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("GameFolder");
                    userNode.InnerText = Global.Directory;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("JavaFile");
                    userNode.InnerText = Global.JavaPath;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("Visibly");
                    userNode.InnerText = Global.Visib;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("Version");
                    userNode.InnerText = Global.VersionN;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("Memory");
                    userNode.InnerText = Global.Memory;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("ReleaseActive");
                    userNode.InnerText = "" + Global.RelCheck;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("SnapshotActive");
                    userNode.InnerText = "" + Global.SnapCheck;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("BetaActive");
                    userNode.InnerText = "" + Global.BetaCheck;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("AlphaActive");
                    userNode.InnerText = "" + Global.AlphaCheck;
                    rootNode.AppendChild(userNode);

                    userNode = xmlDoc.CreateElement("CustomActive");
                    userNode.InnerText = "" + Global.UncommonCheck;
                    rootNode.AppendChild(userNode);

                    xmlDoc.Save(System.IO.Directory.GetCurrentDirectory() + "\\Launcher_Options.xml");

                    Global.Opt = 0;
                    Program.form1.DebugLog.Items.Add(String.Empty);
                    Program.form1.DebugLog.Items.Add("Настройки сохранены (" + DateTime.Now + ")");
                    Program.form1.DebugLog.Items.Add("Minecraft v." + Global.Version + " будет проверен и запущен. Ваш ник: " + Global.Nick);
                    Close();
                }
                else
                {
                    labelWarn.Text = "Путь до javaw.exe неверный - файл не существует!";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.Opt = 0;
            Close();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Global.Opt = 0;
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Global.RelCheck = checkBox1.Checked;
            comboBox2.Items.Clear();
            ChangeMine();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Global.SnapCheck = checkBox2.Checked;
            comboBox2.Items.Clear();
            ChangeMine();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Global.BetaCheck = checkBox3.Checked;
            comboBox2.Items.Clear();
            ChangeMine();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Global.AlphaCheck = checkBox4.Checked;
            comboBox2.Items.Clear();
            ChangeMine();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            Global.UncommonCheck = checkBox5.Checked;
            comboBox2.Items.Clear();
            ChangeMine();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog Fld = new FolderBrowserDialog();

            if (Directory.Exists(Global.Directory))
                Fld.SelectedPath = Global.Directory;
            else
                Fld.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            Fld.ShowNewFolderButton = true;

            if (Fld.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = Fld.SelectedPath;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (File.Exists(Global.JavaPath))
                openFileDialog1.InitialDirectory = Global.JavaPath.Replace("javaw.exe", "");
            else
                openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            openFileDialog1.Filter = "javaw.exe |javaw.exe|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            textBox3.Text = openFileDialog1.FileName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.form1.DebugLog.Items.Add("Ошибка: не удалось прочитать файл с диска. (" + ex.Message + ")");
                }
            }
        }
    }
}
