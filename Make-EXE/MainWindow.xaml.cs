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
        List<string> args = new List<string>();
        string installedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Make-EXE\Make-EXE.exe";
        string targetPath;
        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            {
                // If true, invalid argument was passed.
                if (arg.ToLower() != "-file" && arg.ToLower() != "-silent" && arg.ToLower() != "-embed" && !File.Exists(arg.ToLower()))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Command Line Syntax");
                    sb.AppendLine();
                    sb.AppendLine("make-exe.exe [-file <path>] [-silent] [-embed]");
                    sb.AppendLine();
                    sb.AppendLine("Options:");
                    sb.AppendLine("    -file   The full path to the PS1 or BAT file to be packaged.  Use quotes if there are spaces.");
                    sb.AppendLine("    -silent   Silently package without any prompts.");
                    sb.AppendLine("    -embed   Used with silent option to embed sibling files.");
                    MessageBox.Show(sb.ToString(), "Make-EXE Help", MessageBoxButton.OK, MessageBoxImage.Information);
                    Environment.Exit(1);
                    return;
                }
                // Maintain case of file name.
                if (File.Exists(arg))
                {
                    args.Add(arg);
                }
                else
                {
                    args.Add(arg.ToLower());
                }
            }
            if (args.Count > 0)
            {
                if (args.Contains("-file"))
                {
                    if (args.IndexOf("-file") + 1 >= args.Count || !File.Exists(args[args.IndexOf("-file") + 1]))
                    {
                        MessageBox.Show("The -file argument should be the full path to a PS1 or BAT file that you want to package.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(1);
                        return;
                    }
                    else
                    {
                        targetPath = args[args.IndexOf("-file") + 1];
                    }
                }
            }
            InitializeComponent();
            AutoUpdater.AutoUpdater.RemoteFileURI = "https://translucency.info/Downloads/" + AutoUpdater.AutoUpdater.FileName;
            AutoUpdater.AutoUpdater.ServiceURI = "https://translucency.info/Services/VersionCheck.cshtml?Path=/Downloads/" + AutoUpdater.AutoUpdater.FileName;
            AutoUpdater.AutoUpdater.CheckCommandLineArgs();
        }

        private void Current_DispatcherUnhandledException(Object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (args.Contains("-silent"))
            {
                return;
            }
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
            if (targetPath != null)
            {
                this.Hide();
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
                compilerParams.OutputAssembly = Path.GetDirectoryName(targetPath) + @"\" + Path.GetFileNameWithoutExtension(targetPath) + ".exe";
                compilerParams.EmbeddedResources.Add(targetPath);

                MessageBoxResult ask = new MessageBoxResult();
                if (!args.Contains("-silent"))
                {
                    ask = MessageBox.Show("Embed all files in the script's folder as resources?", "Embed Files", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }

                if (ask == MessageBoxResult.Yes || args.Contains("-embed"))
                {
                    foreach (var file in Directory.GetFiles(Path.GetDirectoryName(targetPath)).Where(strPath => strPath != targetPath))
                    {
                        if (Path.GetExtension(file) == ".ico")
                        {
                            compilerParams.CompilerOptions = "/win32icon:\"" + file + "\" /fullpaths";
                        }
                        compilerParams.EmbeddedResources.Add(file);
                    }
                }
                var fs = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Make_EXE.Maker.cs");
                var strScript = new StreamReader(fs).ReadToEnd();
                fs.Close();
                var results = provider.CompileAssemblyFromSource(compilerParams, strScript);
                
                if (!args.Contains("-silent"))
                {
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
                        MessageBox.Show("Your EXE has been packaged!  Location: " + compilerParams.OutputAssembly, "EXE Packaged", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                if (results.Errors.HasErrors)
                {
                    Environment.Exit(1);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                if (File.Exists(installedPath))
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
            MessageBox.Show("Install completed!  If the 'Make EXE' option isn't showing up, reset your program defaults and reinstall Make-EXE.", "Install Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void buttonRemove_Click(Object sender, RoutedEventArgs e)
        {
            foreach (var file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Make-EXE"))
            {
                File.Delete(file);
            }
            try
            {
                Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Make-EXE", true);
            }
            catch { }
            var psi = new ProcessStartInfo("cmd.exe", @"/c reg.exe delete HKCR\.ps1\shell\MakeEXE /f&reg.exe delete HKCR\Microsoft.PowerShellScript.1\Shell\MakeEXE /f&reg.exe delete HKCR\Applications\powershell_ise.exe\shell\MakeEXE /f&reg.exe delete HKCR\batfile\shell\MakeEXE /f");
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
