using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make_EXE.Models
{
    public class AssemblyInfo
    {
        public bool? EmbedFiles { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string CompanyName { get; set; }
        public string Copyright { get; set; }
    }
}
