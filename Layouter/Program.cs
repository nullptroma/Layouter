using System;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;



namespace Layouter
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //MessageBox.Show(Application.ExecutablePath);
            if(Application.ExecutablePath.Split('\\').Last() != "Layouter.exe")
            {
                try
                {
                    File.Move(Application.ExecutablePath, Environment.CurrentDirectory + "\\Layouter.exe");
                }
                catch
                {
                    try
                    {
                        File.Delete(Environment.CurrentDirectory + "\\Layouter.exe");
                        File.Move(Application.ExecutablePath, Environment.CurrentDirectory + "\\Layouter.exe");
                    }
                    catch
                    {
                        Directory.Delete(Environment.CurrentDirectory + "\\Layouter.exe", true);
                        File.Move(Application.ExecutablePath, Environment.CurrentDirectory + "\\Layouter.exe");
                    }
                }
                Process.Start(Environment.CurrentDirectory + "\\Layouter.exe");
                Process.GetCurrentProcess().Kill();
            }

            if (Process.GetProcessesByName("Layouter").Length>1)//если прога уже запущена то убиваем все процессы и оставляем только себя
            {
                //MessageBox.Show((Process.GetProcessesByName("Layouter") != null).ToString());
                foreach (Process prc in Process.GetProcessesByName("Layouter"))
                {
                    if(prc.Id != Process.GetCurrentProcess().Id)
                        SendMessage(FindWindow(null, "Layouter"), 0x0002,0, "");//убиваем всех кто не мы
                    
                }
            }
            Resolver.RegisterDependencyResolver();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Frm1());
        }

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);



        public static class Resolver
        {
            private static volatile bool _loaded;

            public static void RegisterDependencyResolver()
            {
                if (!_loaded)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += OnResolve;
                    _loaded = true;
                }
            }

            private static Assembly OnResolve(object sender, ResolveEventArgs args)
            {

                Assembly execAssembly = Assembly.GetExecutingAssembly();
                string resourceName = String.Format("{0}.{1}.dll",
                    execAssembly.GetName().Name,
                    new AssemblyName(args.Name).Name);
                
                using (var stream = execAssembly.GetManifestResourceStream(resourceName))
                {
                    int read = 0, toRead = (int)stream.Length;
                    byte[] data = new byte[toRead];

                    do
                    {
                        int n = stream.Read(data, read, data.Length - read);
                        toRead -= n;
                        read += n;
                    } while (toRead > 0);

                    return Assembly.Load(data);
                }
            }
        }
    }
}
