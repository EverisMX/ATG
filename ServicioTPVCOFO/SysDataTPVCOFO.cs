using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;

namespace ServicioTPVAgenteLocal
{
    public class SysDataTPVCOFO
    {
        #region "Public Methods"

        /// <summary>
        /// Metodo que devuelve el uso de CPU durante la ejecución del hilo 
        /// </summary>
        /// <returns>Valor porcentual (Double) </returns>
        public double GetProcessorData()
        {
            return _GetCounterValue(_cpuCounter, "Processor", "% Processor Time", "_Total");
        }

        /// <summary>
        /// Metodo que devuelve el uso de a memoria virtual durante la ejecución del hilo 
        /// </summary>
        /// <returns>Resultado devuelve un objeto SysValues (DeviceID, Total, Used)</returns>
        public SysValues GetVirtualMemory()
        {
            double d = _GetCounterValue(_memoryCounter, "Memory", "Committed Bytes", null);
            double totalphysicalmemory = _GetCounterValue(_memoryCounter, "Memory", "Commit Limit", null);
            return new SysValues { DeviceID = "Virtual Memory", Total = totalphysicalmemory, Used = d };
        }

        /// <summary>
        /// Metodo que devuelve el uso de a memoria física durante la ejecución del hilo 
        /// </summary>
        /// <returns>Resultado devuelve un objeto SysValues (DeviceID, Total, Used)</returns>
        public SysValues GetPhysicalMemory()
        {
            string s = _QueryComputerSystem("totalphysicalmemory");
            double totalphysicalmemory = Convert.ToDouble(s);
            double d = _GetCounterValue(_memoryCounter, "Memory", "Available Bytes", null);
            return new SysValues { DeviceID = "Physical Memory", Total = totalphysicalmemory, Used = totalphysicalmemory - d };
        }


        /// <summary>
        /// Metodo que devuelve el espacio en disco utilizado 
        /// </summary>
        /// <returns>Resultado devuelve una lista de SysValues (DeviceID, Total, Used)</returns>
        public List<SysValues> GetDiskSpaces()
        {
            List<SysValues> disks = new List<SysValues>();
            object device, size, free;
            double totle;
            ManagementObjectSearcher objCS = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            foreach (ManagementObject objMgmt in objCS.Get())
            {
                device = objMgmt["DeviceID"];	
                if (null != device)
                {
                    size = objMgmt["Size"];
                    free = objMgmt["FreeSpace"];	
                    if (null != free && null != size)
                    {
                        totle = double.Parse(size.ToString());
                        disks.Add(new SysValues { DeviceID = device.ToString(), Total = totle, Used = totle - double.Parse(free.ToString()) });
                    }
                }
            }

            return disks;
        }
        #endregion
        
        #region "Private Helpers"

        /// <summary>
        /// Función que a partir de los parámetros recibidos asignada los valores al objeto de tipo PerformanceCounter.
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="categoryName"></param>
        /// <param name="counterName"></param>
        /// <param name="instanceName"></param>
        /// <returns>Resultado devuelve un valor del tipo Double tras invocar la función NextValue de la clase PerformanceCounter.</returns>
        double _GetCounterValue(PerformanceCounter pc, string categoryName, string counterName, string instanceName)
        {
            pc.CategoryName = categoryName;
            pc.CounterName = counterName;
            pc.InstanceName = instanceName;
            return pc.NextValue();
        }

        /// <summary>
        ///  Metodo utilza devuelve el uso de a memoria física durante la ejecución del hilo 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>La descripción a partir del valor tipo enviado verificando el resultado de la consulta al Win32_ComputerSystem </returns>
        string _QueryComputerSystem(string type)
        {
            string str = null;
            ManagementObjectSearcher objCS = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject objMgmt in objCS.Get())
            {
                str = objMgmt[type].ToString();
            }
            return str;
        }
        #endregion

        #region "Members"
        PerformanceCounter _memoryCounter = new PerformanceCounter();
        PerformanceCounter _cpuCounter = new PerformanceCounter();
        #endregion
    }

    public class SysValues
    {        
        public string DeviceID { get; set; }
        public double Total { get; set; }
        public double Used { get; set; }
    }
}
