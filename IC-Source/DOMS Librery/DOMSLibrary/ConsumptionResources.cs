
using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;
using DOMSLibrary.BE;

namespace DOMSLibrary
{
    public class ConsumptionResources
    {
       public string ConsumptionResourcesService(string procName)
        {
            List<PerformanceResourcesBE> ListConsResour = new List<PerformanceResourcesBE>();
            PerformanceResourcesBE ConsResour = new PerformanceResourcesBE();
            DOMSTankGauge doms = new DOMSTankGauge();
            string JsonResources = "";

            Process[] runningNow;
            runningNow =  Process.GetProcessesByName(procName);

            if (runningNow.Length > 0)
            {
                using (PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set - Private", runningNow[0].ProcessName))
                using (PerformanceCounter pcProcess = new PerformanceCounter("Process", "% Processor Time", runningNow[0].ProcessName))
                using (PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "Disk Transfers/sec", "_Total"))
                {
                    pcProcess.NextValue();
                    //float cpuUseage = pcProcess.NextValue();
                    Thread.Sleep(1000);
                    ConsResour.CPU_Usage = (pcProcess.NextValue() / Environment.ProcessorCount * 1.00).ToString("0.0") + "%";

                    // Get Process Memory Usage
                    ConsResour.Memory_Usage = (ramCounter.NextValue() / 1048576.00).ToString("0.00") + "MB";

                    // Get Total Space in disk
                    diskCounter.NextValue();
                    Thread.Sleep(1000);
                    double memSpaceDisk = diskCounter.NextValue() / 1048576.00;
                    ConsResour.Disk_Usage = memSpaceDisk.ToString("0.00") + "MB";

                    // Get thread Count
                    int memThread = runningNow[0].Threads.Count;
                    ConsResour.Thread_count = memThread.ToString();

                    // Get Network Usage
                    ConsResour.Network_Usage = fnGetNewtWorkUsage(runningNow[0].ProcessName) + "MB";

                    ListConsResour.Add(ConsResour);
                    JsonResources = TransformJson(ListConsResour);

                    return JsonResources;
                }
            }
            else
            {
                return "";
            }
            
            
        }

        private string fnGetNewtWorkUsage(string strProccess)
        {
            string pn = strProccess;
            //var readOpSec = new PerformanceCounter("Process", "IO Read Operations/sec", pn);
            //var writeOpSec = new PerformanceCounter("Process", "IO Write Operations/sec", pn);
            //var dataOpSec = new PerformanceCounter("Process", "IO Data Operations/sec", pn);
            //var readBytesSec = new PerformanceCounter("Process", "IO Read Bytes/sec", pn);
            //var writeByteSec = new PerformanceCounter("Process", "IO Write Bytes/sec", pn);
            var dataBytesSec = new PerformanceCounter("Process", "IO Data Bytes/sec", pn);

            //PerformanceCounter netSentCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", ns);
            //PerformanceCounter netRecCounter =
            //    new PerformanceCounter("Network Interface", "Bytes Sent/sec", ns);

            return (dataBytesSec.RawValue/ 1048576.00).ToString("0.00");
        }

        public string TransformJson(object ObjectTransform)
        {
            return new JavaScriptSerializer().Serialize(ObjectTransform);
        }

    }
}
