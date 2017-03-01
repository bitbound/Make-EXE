using System;
using System.IO;
using System.Diagnostics;
[assembly: System.Reflection.AssemblyVersion("0.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("0.0.0.0")]
[assembly: System.Reflection.AssemblyProduct("")]
[assembly: System.Reflection.AssemblyCompany("")]
[assembly: System.Reflection.AssemblyDescription("")]
[assembly: System.Reflection.AssemblyCopyright("")]
namespace Make_EXE
{
    class Program
    {
        static bool redirect = false;
        static void Main(string[] args)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            Console.Title = resources[0];
            Console.WriteLine("Extracting resource files...");
            var basePath = System.IO.Path.GetTempPath();
            if (System.Security.Principal.WindowsIdentity.GetCurrent().IsSystem)
            {
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            }
            var workingDir = "";
            var count = 0;
            while (workingDir == "")
            {
                try
                {
                    if (Directory.Exists(Path.Combine(basePath, "Make-EXE" + count)))
                    {
                        Directory.Delete(Path.Combine(basePath, "Make-EXE" + count), true);
                    }
                    else
                    {
                        workingDir = Path.Combine(basePath, "Make-EXE" + count + "\\");
                        Directory.CreateDirectory(workingDir);
                    }
                }
                catch
                {
                    count++;
                }
            }
            count = 1;
            foreach (var resource in resources)
            {
                Console.WriteLine("Extracting file " + count + " of " + resources.Length + "...");
                using (var rs = assembly.GetManifestResourceStream(resource))
                {
                    using (var fs = new FileStream(workingDir + resource, FileMode.Create))
                    {
                        rs.CopyTo(fs);
                        fs.Flush();
                        fs.Close();
                    }
                }
                count++;
            }
            Console.WriteLine("Starting up...");
            if (redirect)
            {
                var psi = new ProcessStartInfo();
                var proc = new Process();
                proc.EnableRaisingEvents = true;
                proc.StartInfo = psi;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                if (Path.GetExtension(resources[0]).ToLower() == ".ps1")
                {
                    psi.FileName = "powershell.exe";
                    psi.Arguments = "-executionpolicy bypass -file \"" + workingDir + resources[0] + "\"";
                    proc.Start();
                    proc.WaitForExit();
                    Console.WriteLine(proc.StandardOutput.ReadToEnd());
                }
                else if (Path.GetExtension(resources[0]).ToLower() == ".bat")
                {
                    psi.FileName = "cmd.exe";
                    psi.Arguments = "/c \"" + workingDir + resources[0] + "\"";
                    proc.Start();
                    proc.WaitForExit();
                    Console.WriteLine(proc.StandardOutput.ReadToEnd());
                }
            }
            else
            {
                if (Path.GetExtension(resources[0]).ToLower() == ".ps1")
                {
                    System.Diagnostics.Process.Start("powershell.exe", "-executionpolicy bypass -file \"" + workingDir + resources[0] + "\"");
                }
                else if (Path.GetExtension(resources[0]).ToLower() == ".bat")
                {
                    System.Diagnostics.Process.Start("cmd.exe", "/c \"" + workingDir + resources[0] + "\"");
                }
            }
            
        }
    }
}