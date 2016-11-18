using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Make_EXE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] args = Environment.GetCommandLineArgs();
        string installedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Make-EXE\Make-EXE.exe";
        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            if (args.Length > 1 && !File.Exists(args[1]))
            {
                MessageBox.Show("The only argument you should give is the path to a PS1 file that you want to compile.", "Invalid Argument", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            InitializeComponent();
            AutoUpdater.AutoUpdater.RemoteFileURI = "https://translucency.info/Downloads/" + AutoUpdater.AutoUpdater.FileName;
            AutoUpdater.AutoUpdater.ServiceURI = "https://translucency.info/Services/VersionCheck.cshtml?Path=/Downloads/" + AutoUpdater.AutoUpdater.FileName;
            AutoUpdater.AutoUpdater.CheckCommandLineArgs();
        }

        private void Current_DispatcherUnhandledException(Object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var sb = new StringBuilder();
            sb.AppendLine("Oops.  There was an error.  Please report the below message.");
            sb.AppendLine();
            sb.AppendLine("Error: " + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
            sb.AppendLine();
            var error = e.Exception;
            while (error.InnerException != null)
            {
                sb.AppendLine(error.InnerException.Message + Environment.NewLine + error.InnerException.StackTrace);
                sb.AppendLine();
                error = error.InnerException;
            }
            MessageBox.Show(sb.ToString(), "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Loaded(Object sender, RoutedEventArgs e)
        {
            AutoUpdater.AutoUpdater.CheckForUpdates(true);
            if (args.Length > 1 && File.Exists(args[1]))
            {
                this.Hide();
                var ask = MessageBox.Show("Embed all files in the script's folder as resources?", "Embed Files", MessageBoxButton.YesNo, MessageBoxImage.Question);
                var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp");
                var compilerParams = new System.CodeDom.Compiler.CompilerParameters();
                compilerParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                compilerParams.ReferencedAssemblies.Add("System.dll");
                compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                compilerParams.ReferencedAssemblies.Add("System.Data.dll");
                compilerParams.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
                compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
                compilerParams.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                compilerParams.GenerateExecutable = true;
                compilerParams.OutputAssembly = Path.GetDirectoryName(args[1]) + @"\" + Path.GetFileNameWithoutExtension(args[1]) + ".exe";
                MessageBox.Show("Building EXE at " + compilerParams.OutputAssembly, "Building File", MessageBoxButton.OK, MessageBoxImage.Information);
                compilerParams.EmbeddedResources.Add(args[1]);
                if (ask == MessageBoxResult.Yes)
                {
                    foreach (var file in Directory.GetFiles(Path.GetDirectoryName(args[1])).Where(strPath => strPath != args[1]))
                    {
                        compilerParams.EmbeddedResources.Add(Path.GetFileName(file));
                    }
                }
                var fs = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Make_EXE.Maker.cs");
                var strScript = new StreamReader(fs).ReadToEnd();
                fs.Close();
                var results = provider.CompileAssemblyFromSource(compilerParams, strScript);
                
                if (results.Errors.HasErrors)
                {
                    MessageBox.Show("There were errors compiling the EXE.", "Compiler Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    foreach (CompilerError error in results.Errors)
                    {
                        MessageBox.Show("Error: " + error.ErrorNumber + ": " + error.ErrorText + "  Column: " + error.Column + "  Line: " + error.Line);
                    }
                }
                else
                {
                    MessageBox.Show("Your EXE has been compiled!", "EXE Compiled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                this.Close();
            }
            else
            {
                var isInstalled = false;
                var psKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(".ps1");
                if (psKey.GetSubKeyNames().Contains("shell"))
                {
                    if (psKey.OpenSubKey("shell").GetSubKeyNames().Contains("MakeEXE"))
                    {
                        isInstalled = true;
                    }
                }
                if (File.Exists(installedPath) && isInstalled)
                {
                    buttonInstall.IsEnabled = false;
                    buttonRemove.IsEnabled = true;
                }
                else
                {
                    buttonInstall.IsEnabled = true;
                    buttonRemove.IsEnabled = false;
                }
            }
            
        }
        private void buttonInstall_Click(Object sender, RoutedEventArgs e)
        {
            var assem = System.Reflection.Assembly.GetExecutingAssembly();
            var reStream = assem.GetManifestResourceStream("Make_EXE.Assets.MakeReg.reg");
            var reRead = new StreamReader(reStream);
            var content = reRead.ReadToEnd();
            reRead.Close();
            reStream.Close();
            File.WriteAllText(System.IO.Path.GetTempPath() + @"\MakeReg.reg", content);
            var psi = new ProcessStartInfo("reg.exe", "import " + System.IO.Path.GetTempPath() + @"\MakeReg.reg");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            var proc = Process.Start(psi);
            proc.WaitForExit();
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Make-EXE");
            File.Copy(Application.ResourceAssembly.ManifestModule.Assembly.Location, installedPath, true);
            buttonInstall.IsEnabled = false;
            buttonRemove.IsEnabled = true;
            MessageBox.Show("Install completed!  Now right-click some PowerShell files!  If the 'Make EXE' option isn't showing up, reset your program defaults and reinstall Make-EXE.", "Install Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void buttonRemove_Click(Object sender, RoutedEventArgs e)
        {
            Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Make-EXE", true);
            var psi = new ProcessStartInfo("reg.exe", @"delete HKCR\.ps1\shell\MakeEXE /f");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            var proc = Process.Start(psi);
            proc.WaitForExit();
            MessageBox.Show("Make EXE has been removed.", "Uninstall Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            buttonInstall.IsEnabled = true;
            buttonRemove.IsEnabled = false;
        }

        private void buttonInfo_Click(Object sender, RoutedEventArgs e)
        {
            new Windows.AboutWindow().ShowDialog();
        }
    }
}
