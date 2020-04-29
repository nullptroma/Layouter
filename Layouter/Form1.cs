using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using GlobalHookKeys;
using AutoHotkey.Interop;


namespace Layouter
{
    


    public partial class Frm1 : Form
    {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private static void SendCtrlhotKey(char key)
        {
            keybd_event(0x11, 0, 0, 0);
            keybd_event((byte)key, 0, 0, 0);
            keybd_event((byte)key, 0, 0x2, 0);
            keybd_event(0x11, 0, 0x2, 0);
        }

        void Copy()
        {
            SendCtrlhotKey('C');
            Thread.Sleep(50);//это надо потому что Clipboard так быстро не понимат что мы скопировали
        }

        void Paste()
        {
            SendCtrlhotKey('V');
        }


        void EnterData(object dataObj)
        {
            var data = new DataObject();
            Thread thread;
            data.SetData(DataFormats.UnicodeText, true, dataObj);
            thread = new Thread(() => Clipboard.SetDataObject(data, true));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        void registerAutoRun()
        {
            const string applicationName = "Layouter";
            const string pathRegistryKeyStartup =
                        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            using (RegistryKey registryKeyStartup =
                        Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
            {
                registryKeyStartup.SetValue(
                    applicationName,
                    string.Format("\"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location));
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        void unRegisterAutoRun()
        {
            const string applicationName = "Layouter";
            const string pathRegistryKeyStartup =
                        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            using (RegistryKey registryKeyStartup =
                        Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
            {
                registryKeyStartup.DeleteValue(applicationName, false);
            }
        }

        List<(char, char)> LayoutSimbols = new List<(char, char)>(){
        ('^',':'),('&','?'),

        ('q','й'),('w','ц'),('e','у'),('r','к'),('t','е'),('y','н'),('u','г'),('i','ш'),('o','щ'),('p','з'),
        ('[','х'),(']','ъ'),('a','ф'),('s','ы'),('d','в'),('f','а'),('g','п'),('h','р'),('j','о'),('k','л'),
        ('l','д'),(';','ж'),('\'','э'),('z','я'),('x','ч'),('c','с'),('v','м'),('b','и'),('n','т'),('m','ь'),
        (',','б'),('.','ю'),

        ('Q','Й'),('W','Ц'),('E','У'),('R','К'),('T','Е'),('Y','Н'),('U','Г'),('I','Ш'),('O','Щ'),('P','З'),
        ('[','Х'),(']','Ъ'),('A','Ф'),('S','Ы'),('D','В'),('F','А'),('G','П'),('H','Р'),('J','О'),('K','Л'),
        ('L','Д'),(':','Ж'),('\'','Э'),('Z','Я'),('X','Ч'),('C','С'),('V','М'),('B','И'),('N','Т'),('M','Ь'),
        ('<','Б'),('>','Ю'),
        
        ('@','"'),('#','№'),('?',','),('$',';')

        };

        string replaceSimbols(string str, List<(char, char)> SimLib)
        {
            

            for (int i = 0; i < str.Length; i++)
            {
                foreach ((char, char) chrs in SimLib)
                {

                    if (str[i] == chrs.Item1)
                    {
                        str = str.Remove(i, 1).Insert(i, chrs.Item2.ToString());
                        break;
                    }
                    else if (str[i] == chrs.Item2)
                    {
                        str = str.Remove(i, 1).Insert(i, chrs.Item1.ToString());
                        break;
                    }

                }
            }

            return str;

        }



        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        void register()
        {
            RegisterHotKey(this.Handle, 0, 2, (int)'Q');
            RegisterHotKey(this.Handle, 1, 2, (int)Keys.F2);
            RegisterHotKey(this.Handle, 2, 2, (int)Keys.F3);
        }

        public Frm1()
        {
            InitializeComponent();
            notifyIcon1.Visible = false;
            secondTH.Start();
            register();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {    
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case 0:
                        Layouting();
                        break;
                    case 1:
                        string old = (string)Clipboard.GetData(DataFormats.UnicodeText);
                        EnterData(replaceSimbols(old, LayoutSimbols));
                        break;
                    case 2:
                        needClick = !needClick;
                        break;
                }
            }
            base.WndProc(ref m);
        }

        void Layouting()
        {
            Copy();
            string old = (string)Clipboard.GetData(DataFormats.UnicodeText);
            EnterData(replaceSimbols(old, LayoutSimbols));
            Paste();
        }


        static bool needClick = false;
        static Thread secondTH = new Thread(ThreadTwo);
        static uint sleepTime = 1000;
        static bool LeftOrRightButton = false;//false - левая
        static private void ThreadTwo()//функция для отдельного потока
        {
            while (true)
            {
                while (needClick)
                {
                    if (!LeftOrRightButton)
                        pressLeftMouse();
                    else
                        pressRightMouse();

                    Thread.Sleep((int)sleepTime);
                }
                Thread.Sleep(200);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        static void pressLeftMouse()
        {
            mouse_event((uint)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
            mouse_event((uint)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
        }

        static void pressRightMouse()
        {
            mouse_event((uint)MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
            mouse_event((uint)MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
        }

        [Flags]
        public enum MouseEventFlags//перечисление событий мыши
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }



        private void Frm1_Load(object sender, EventArgs e)
        {
            loadSettings();
        }


        

        private void Layouter_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Visible = true;
            // возвращаем отображение окна в панели
            
            ShowInTaskbar = true;

            WindowState = FormWindowState.Normal;
        }

        private void Frm1_Resize(object sender, EventArgs e)
        {
            
            if (WindowState == FormWindowState.Minimized)//если свёрнуто
            {
                //MessageBox.Show(Frm1.ActiveForm.ToString());
                // прячем наше окно из панели
                
                this.ShowInTaskbar = false;
                this.Visible = false;
                // делаем нашу иконку в трее активной
                notifyIcon1.Visible = true;
                //MessageBox.Show(Visible.ToString());

            }
            register();
            this.Update();
        }   

        private void button1_Click(object sender, EventArgs e)
        {
            registerAutoRun();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            unRegisterAutoRun();
        }

        private void Frm1_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            notifyIcon1.Visible = true;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        void killAll(object sender, EventArgs e)
        {
           
            try
            {
                saveSettings();
                notifyIcon1.Visible = false;
                secondTH.Abort();//убивает второй поток
            }
            catch
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private void Frm1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }


        void loadSettings()
        {
            LeftOrRightButton = Properties.Settings.Default.LeftOrRight;
            sleepTime = Properties.Settings.Default.SleepTimeAutoClick;
            checkBox1.Checked = LeftOrRightButton;
            textBox1.Text = sleepTime.ToString();
            
        }

        void saveSettings()
        {
            Properties.Settings.Default.LeftOrRight = LeftOrRightButton;
            Properties.Settings.Default.SleepTimeAutoClick = Convert.ToUInt32(textBox1.Text);
            sleepTime = Properties.Settings.Default.SleepTimeAutoClick;
            Properties.Settings.Default.Save();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                label1.Focus();
            }
        }

        private void Frm1_Click(object sender, EventArgs e)
        {
            label1.Focus();
            saveSettings();
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            saveSettings();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            LeftOrRightButton = checkBox1.Checked;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Frm1_FormClosing(object sender, FormClosingEventArgs e)
        {
            killAll(null, null);
        }
    }

    
}
