using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary.BE
{
    public class PerformanceResourcesBE
    {
        #region Fields
        public string CPU_Usage { get; set; }
        public string Memory_Usage { get; set; }
        public string Disk_Usage { get; set; }
        public string Thread_count { get; set; }
        public string Network_Usage { get; set; }
        #endregion
    }
}
