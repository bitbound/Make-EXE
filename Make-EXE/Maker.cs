using System;
using System.IO;
namespace Make_EXE
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            Console.Title = resources[0];
            Console.WriteLine("Extracting resource files...");
            var workingDir = "";
            var count = 0;
            while (workingDir == "")
            {
                try
                {
                    if (Directory.Exists(System.IO.Path.GetTempPath() + "Make-EXE" + count))
                    {
                        Directory.Delete(System.IO.Path.GetTempPath() + "Make-EXE" + count, true);
                    }
                    else
                    {
                        workingDir = System.IO.Path.GetTempPath() + "Make-EXE" + count + @"\";
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
            if (Path.GetExtension(resources[0]).ToLower() == ".ps1")
            {
                System.Diagnostics.Process.Start("powershell.exe", "-executionpolicy bypass -file \"" + workingDir + resources[0] + "\"");
            }
            else if (Path.GetExtension(resources[0]).ToLower() == ".bat")
            {
                System.Diagnostics.Process.Start("cmd.exe", "/c " + resources[0]);
            }
        }
    }
}