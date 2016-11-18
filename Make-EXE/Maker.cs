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
            Console.WriteLine("Extracting resources...");
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
                    workingDir = System.IO.Path.GetTempPath() + "Make-EXE" + count + @"\";
                    Directory.CreateDirectory(workingDir);
                }
                catch
                {
                    count++;
                }
            }
            foreach (var resource in resources)
            {
                using (var rs = assembly.GetManifestResourceStream(resource))
                {
                    using (var fs = new FileStream(workingDir + resource, FileMode.Create))
                    {
                        rs.CopyTo(fs);
                        fs.Flush();
                        fs.Close();
                    }
                }
            }
            Console.WriteLine("Starting up...");
            System.Diagnostics.Process.Start("powershell.exe", "-file \"" + workingDir + resources[0] + "\"");
        }
    }
}