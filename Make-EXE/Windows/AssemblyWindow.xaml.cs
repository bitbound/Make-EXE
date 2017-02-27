using Make_EXE.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Make_EXE.Windows
{
    /// <summary>
    /// Interaction logic for AssemblyWindow.xaml
    /// </summary>
    public partial class AssemblyWindow : Window
    {
        public string TargetPath { get; set; }
        public List<string> Args { get; set; }
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        AssemblyInfo jsonAssembly { get; set; } = new AssemblyInfo();
        public AssemblyWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var assemblyPath = Path.Combine(Path.GetDirectoryName(TargetPath), "AssemblyInfo.json");
                if (File.Exists(assemblyPath))
                {
                    var content = File.ReadAllText(assemblyPath);
                    jsonAssembly = serializer.Deserialize<AssemblyInfo>(content);
                    checkEmbed.IsChecked = jsonAssembly.EmbedFiles;
                    textAssemblyVersion.Text = jsonAssembly.AssemblyVersion;
                    textFileVersion.Text = jsonAssembly.FileVersion;
                    textProductName.Text = jsonAssembly.ProductName;
                    textProductDescription.Text = jsonAssembly.ProductDescription;
                    textCompanyName.Text = jsonAssembly.CompanyName;
                    textCopyright.Text = jsonAssembly.Copyright;
                }
            }
            catch { }
            if (Args.Contains("-silent"))
            {
                this.Hide();
                buttonMake_Click(buttonMake, new RoutedEventArgs());
            }
        }

        private void buttonMake_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (checkSave.IsChecked == true)
                {
                    var assemblyPath = Path.Combine(Path.GetDirectoryName(TargetPath), "AssemblyInfo.json");
                    jsonAssembly = new AssemblyInfo();
                    jsonAssembly.EmbedFiles = checkEmbed.IsChecked;
                    jsonAssembly.AssemblyVersion = textAssemblyVersion.Text;
                    jsonAssembly.FileVersion = textFileVersion.Text;
                    jsonAssembly.ProductName = textProductName.Text;
                    jsonAssembly.ProductDescription = textProductDescription.Text;
                    jsonAssembly.CompanyName = textCompanyName.Text;
                    jsonAssembly.Copyright = textCopyright.Text;
                    File.WriteAllText(assemblyPath, serializer.Serialize(jsonAssembly));
                }
            }
            catch { }
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var compilerParams = new CompilerParameters();
            compilerParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            compilerParams.GenerateExecutable = true;
            compilerParams.OutputAssembly = Path.GetDirectoryName(TargetPath) + @"\" + Path.GetFileNameWithoutExtension(TargetPath) + ".exe";
            compilerParams.EmbeddedResources.Add(TargetPath);

            if (checkEmbed.IsChecked == true || Args.Contains("-embed"))
            {
                foreach (var file in Directory.GetFiles(Path.GetDirectoryName(TargetPath)).Where(strPath => strPath != TargetPath))
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
            strScript = strScript.Replace("[assembly: System.Reflection.AssemblyVersion(\"0.0.0.0\")]", "[assembly: System.Reflection.AssemblyVersion(\"" + jsonAssembly.AssemblyVersion + "\")]");
            strScript = strScript.Replace("[assembly: System.Reflection.AssemblyFileVersion(\"0.0.0.0\")]", "[assembly: System.Reflection.AssemblyFileVersion(\"" + jsonAssembly.FileVersion + "\")]");
            strScript = strScript.Replace("[assembly: System.Reflection.AssemblyProduct(\"\")]", "[assembly: System.Reflection.AssemblyProduct(\"" + jsonAssembly.ProductName + "\")]");
            strScript = strScript.Replace("[assembly: System.Reflection.AssemblyDescription(\"\")]", "[assembly: System.Reflection.AssemblyDescription(\"" + jsonAssembly.ProductDescription + "\")]");
            strScript = strScript.Replace("[assembly: System.Reflection.AssemblyCompany(\"\")]", "[assembly: System.Reflection.AssemblyCompany(\"" + jsonAssembly.CompanyName + "\")]");
            strScript = strScript.Replace("[assembly: System.Reflection.AssemblyCopyright(\"\")]", "[assembly: System.Reflection.AssemblyCopyright(\"" + jsonAssembly.Copyright + "\")]");
            var results = provider.CompileAssemblyFromSource(compilerParams, strScript);
            if (!Args.Contains("-silent"))
            {
                this.Hide();
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

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }
    }
}
