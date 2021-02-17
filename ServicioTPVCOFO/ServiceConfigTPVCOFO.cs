using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServicioTPVAgenteLocal
{
    public class ServiceConfigTPVCOFO
    {
        #region Properties
        [XmlAttribute()]
        public string ServiceName { get; set; }
        public Host[] ListHosts { get; set; }
        public ServicioWeb[] ListServicioWeb { get; set; }
        public Socket[] ListSocket { get; set; }
        public Library[] ListLibrary { get; set; }
        public Usage[] Usages;
        public enum DeviceType { CPU, PhysicalMemory, VirtualMemory, DiskSpace };
        public Alarm[] ListAlarm { get; set; }
        #endregion //Properties

        public class Usage
        {
            [XmlAttribute()]
            public DeviceType DeviceID;
            [XmlAttribute()]
            public bool Enable;
            [XmlAttribute()]
            public double Threshold;
        }
        
        public class Host
        {
            [XmlAttribute()]
            public string HostName { get; set; }
            [XmlAttribute()]
            public int TimeCheckCycle { get; set; }
            [XmlAttribute()]
            public int TimeStartDelay { get; set; }
            [XmlAttribute()]
            public int TimeStopTimeout { get; set; }
            [XmlAttribute()]
            public bool BitSaveResponse { get; set; }
            [XmlAttribute()]
            public bool BitBatchResponse { get; set; }
            [XmlAttribute()]
            public string BatchName { get; set; }
            [XmlAttribute()]
            public int TimeMinuteBatchCycle { get; set; }


        }
        public class ServicioWeb
        {
            [XmlAttribute()]
            public string EndPoint { get; set; }
            [XmlAttribute()]
            public string XML { get; set; }
            [XmlAttribute()]
            public string JsonName { get; set; } //Nombre del Archivo Json Post Body Request
            [XmlAttribute()]
            public int TimeCheckCycle { get; set; }
            [XmlAttribute()]
            public int TimeStartDelay { get; set; }
            [XmlAttribute()]
            public bool BitSaveResponse { get; set; }
            [XmlAttribute()]
            public bool BitBatchResponse { get; set; }
            [XmlAttribute()]

            public string BatchName { get; set; }
            //[XmlAttribute()]
            //public int TimeMinuteBatchCycle { get; set; }
            [XmlAttribute()]
            public string TypeService { get; set; }//READ OR WRITE

        }
        public class Socket
        {
            [XmlAttribute()]
            public string IP { get; set; }

            [XmlAttribute()]
            public int Puerto { get; set; }

            [XmlAttribute()]
            public int TimeCheckCycle { get; set; }
            [XmlAttribute()]
            public int TimeStartDelay { get; set; }
            [XmlAttribute()]
            public int TimeStopTimeout { get; set; }
            [XmlAttribute()]
            public bool BitSaveResponse { get; set; }
            [XmlAttribute()]
            public bool BitBatchResponse { get; set; }
            [XmlAttribute()]
            public string BatchName { get; set; }
            [XmlAttribute()]
            public int TimeMinuteBatchCycle { get; set; }            
        }
        public class Library
        {
            [XmlAttribute()]
            public string LibreriaName { get; set; }
            [XmlAttribute()]
            public string ClaseName { get; set; }
            [XmlAttribute()]
            public string FuncionName { get; set; }
            [XmlAttribute()]
            public string Parameters { get; set; }
            [XmlAttribute()]
            public int TimeCheckCycle { get; set; }
            [XmlAttribute()]
            public int TimeStartDelay { get; set; }
            [XmlAttribute()]
            public bool BitSaveResponse { get; set; }
            [XmlAttribute()]
            public bool BitBatchResponse { get; set; }
            [XmlAttribute()]
            public string BatchName { get; set; }
            //ILION Se agrega el valor del Tank.
            [XmlAttribute()]
            public string Tanks { get; set; }
            //[XmlAttribute()]
            //public bool bitLogon { get; set; }
        }

        public class Batch
        {
            public string BatchName { get; set; }
            public int TimeMinuteBatchCycle { get; set; }
            public int iExecutionState { get; set; }
            public string sNextExecutionDatetime { get; set; }
        }

        public class Alarm
        {
            #region Properties
            [XmlAttribute()]
            public string BatchName { get; set; }
            [XmlAttribute()]
            public string Parameters { get; set; }
            [XmlAttribute()]
            public string UrlServidor { get; set; }
            [XmlAttribute()]
            public string PathApi { get; set; }
            #endregion //Properties
        }
    }
}
