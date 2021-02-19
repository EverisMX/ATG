using ServicioTPVAgenteLocal.BE;
using ServicioTPVAgenteLocal.Utility;
using ServicioTPVAgenteLocal.Utility.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace ServicioTPVAgenteLocal
{
    public class ServiceWorkerTPVCOFO
    {

        ServiceConfigTPVCOFO.ServicioWeb SW = null;
        ServiceConfigTPVCOFO.Socket SK = null;
        ServiceConfigTPVCOFO.Host host = null;
        ServiceConfigTPVCOFO.Library lib = null;
        ServiceConfigTPVCOFO.Usage[] usage = null;
        ServiceBatchTPVCOFO batchH = null;
        ServiceConfigTPVCOFO.Batch batchConfig;
        enum TipoRespuesta { RPT_NO_MANAGE, RPT_REALTIME, RPT_BATCH };
        enum TipoExecuteBatch { NO_EXECUTE, EXECUTING, EXECUTED };

        int idxThread = -1;

        string msg = "";
        SysDataTPVCOFO _SysData = new SysDataTPVCOFO();
        enum Unit { B, KB, MB, GB, TB, ER };
        

        #region Contructor
        /// <summary>
        ///  Constructor asociado a los Hilos tipo Host
        /// </summary> 
        /// 
        public ServiceWorkerTPVCOFO(ServiceConfigTPVCOFO.Host phost, ServiceConfigTPVCOFO.Usage[] pusage, int iPidxThread, ServiceBatchTPVCOFO pHilobatch, ServiceConfigTPVCOFO.Batch pbatchConfig)
        {
            idxThread = iPidxThread;
            host = phost;
            usage = pusage;
            batchH = pHilobatch;
            batchConfig = pbatchConfig;
        }
        /// <summary>
        ///  Constructor asociado a los Hilos tipo Servicio Web
        /// </summary> 
        /// 
        public ServiceWorkerTPVCOFO(ServiceConfigTPVCOFO.ServicioWeb pSW, int iPidxThread, ServiceBatchTPVCOFO pHilobatch, ServiceConfigTPVCOFO.Batch pbatchConfig)
        {
            idxThread = iPidxThread;
            SW = pSW;
            batchH = pHilobatch;
            batchConfig = pbatchConfig;
        }

        /// <summary>
        ///  Constructor asociado a los Hilos tipo Socket
        /// </summary> 
        public ServiceWorkerTPVCOFO(ServiceConfigTPVCOFO.Socket pSK, int iPidxThread, ServiceBatchTPVCOFO pHilobatch, ServiceConfigTPVCOFO.Batch pbatchConfig)
        {
            idxThread = iPidxThread;
            SK = pSK;
            batchH = pHilobatch;
            batchConfig = pbatchConfig;
        }

        /// 
        public ServiceWorkerTPVCOFO(ServiceConfigTPVCOFO.Library pLib, int iPidxThread, ServiceBatchTPVCOFO pHilobatch, ServiceConfigTPVCOFO.Batch pbatchConfig)
        {
            idxThread = iPidxThread;
            lib = pLib;
            batchH = pHilobatch;
            batchConfig = pbatchConfig;
        }
        /// <summary>
        ///  Constructor asociado al Hilo que controlar el Archivo Log
        /// </summary> 
        public ServiceWorkerTPVCOFO()
        {
        }

        #endregion

        #region "Timer Proc Log"

        /// <summary>
        /// Método que se desencadenara por cada vez que se ejecute el Timer Configurado para la generación del LOG encolado. 
        /// </summary>
        /// <param name="o"></param>
        public void TimerProcLog(object o)
        {
            try
            {
                Thread.Sleep(1000 + (idxThread * 250));
                ServiceLogTPVCOFO.Instance.GrabarLog();
            }
            catch (Exception e)
            {
                ServiceLogTPVCOFO.Instance.WriteLine(e.InnerException.ToString());
            }
        }

        #endregion

        #region "Timer Proc Socket"

        /// <summary>
        /// Método que se desencadenara por cada vez que se ejecute el Timer Configurado para ejecución de los Hilos Tipo Socket. 
        /// </summary>
        /// <param name="o"></param>
        public void TimerProcSocket(object o)
        {
            string msge = "";
            SocketAsynchronousClient socketHilos = null;
            AsyncCallback a = null;

            if (SK.BitSaveResponse)
                SendMessageByTipoRespuesta((SK.BitBatchResponse ? 2 : 1), batchConfig, SK.BatchName);

            msge += "\r\n" + "Inicia Timer Socket  ... " + idxThread + " - IP: " + SK.IP + " - Puerto: " + SK.Puerto;
            string s = string.Empty;

            msge += "\r\n" + "------ Check Socket ------";

            socketHilos = new SocketAsynchronousClient();
            a = new AsyncCallback(socketHilos.ConnectCallback);
            try
            {
                Thread.Sleep(1000 + (idxThread * 250));
                if (SK.BatchName != null)
                    socketHilos.StartClient(SK.IP, SK.Puerto, idxThread, msge, a, SK.TimeStopTimeout, batchH, SK.BatchName);
                else
                    socketHilos.StartClient(SK.IP, SK.Puerto, idxThread, msge, a, SK.TimeStopTimeout, batchH, "");
            }
            catch (Exception e)
            {
                msge += "Error al iniciar Socket Client: " + e.Message;
            }
        }
        #endregion

        #region "Timer Proc HOST"

        /// <summary>
        /// Método que concatena los valores relacionado a uso de memoria fisica y virtual.
        /// </summary>
        /// <param name="val"> Variable de tipo SysValues la cual contiene los valores: DeviceID, Total, Used</param>
        /// <param name="usage"> Variable del tipo Usage, la cual contiene las variables: DeviceID, Enable, Threshold, indicadas en el archivo XML de configuración </param>
        /// <returns></returns>
        public string LogSysValueWithUsage(SysValues val, ServiceConfigTPVCOFO.Usage usage)
        {

            double d = 100 * val.Used / val.Total;
            string s = (d >= usage.Threshold ? " Over Threshold(" + usage.Threshold + ")" : "");
            return val.DeviceID + " " + d.ToString("F") + "% ("
                 + FormatBytes(double.Parse(val.Used.ToString())) + "/"
                 + FormatBytes(double.Parse(val.Total.ToString())) + ")" + s;
        }

        /// <summary>
        /// Formato que convierte de bytes a GB
        /// </summary>
        /// <param name="bytes">Valor en Bytes que se convertiran a GB </param>
        /// <returns></returns>
        public string FormatBytes(double bytes)
        {
            int unit = 0;
            while (bytes > 1024)
            {
                bytes /= 1024;
                ++unit;
            }

            return bytes.ToString("F") + " " + ((Unit)unit).ToString();
        }

        /// <summary>
        /// Método que se desencadenara por cada vez que se ejecute el Timer Configurado para ejecución de los Hilos Tipo Host. 
        /// </summary>
        /// <param name="o"></param>
        internal void TimerProcHost(object o)
        {
            try
            {
                String ipRpta = string.Empty;
                IPHostEntry hostInfo = null;
                if (host.BitSaveResponse)
                    SendMessageByTipoRespuesta((host.BitBatchResponse ? 2 : 1), batchConfig, host.BatchName);

                msg = "";
                msg += "\r\n" + "Inicia Timer Host  ... " + idxThread + " - " + host.HostName;
                string s = string.Empty;
                msg += "\r\n" + "------ Check Host ------";

                try
                {
                    ipRpta = String.Empty;
                    hostInfo = Dns.GetHostEntry(host.HostName);
                    Thread.Sleep(1000 + (idxThread * 250));
                    s = string.Empty;
                    foreach (IPAddress ip in hostInfo.AddressList)
                    {
                        ipRpta = ip.ToString();
                        s += "(" + ipRpta + ") ";
                    }

                    msg += "\r\n" + "IP: " + s;
                    if (batchH != null)
                        batchH.WriteLineBatch(host.BatchName, ipRpta);


                }
                catch (Exception e)
                {
                    msg += "\r\n" + "Check " + host + ", Error: " + e.Message;
                }


                msg += "\r\n" + "----- Check Local Computer -----";
                foreach (ServiceConfigTPVCOFO.Usage u in usage)
                {
                    if (!u.Enable)
                        continue;

                    switch (u.DeviceID)
                    {
                        case ServiceConfigTPVCOFO.DeviceType.CPU:
                            double d = _SysData.GetProcessorData();
                            msg += "\r\n" + "CPU Used " + d.ToString("F") + "%"
                               + (d >= u.Threshold ? " Over Threshold(" + u.Threshold + ")" : "");
                            break;
                        case ServiceConfigTPVCOFO.DeviceType.PhysicalMemory:
                            msg += LogSysValueWithUsage(_SysData.GetPhysicalMemory(), u);
                            break;
                        case ServiceConfigTPVCOFO.DeviceType.VirtualMemory:
                            msg += LogSysValueWithUsage(_SysData.GetVirtualMemory(), u);
                            break;
                        case ServiceConfigTPVCOFO.DeviceType.DiskSpace:
                            msg += "\r\n" + "Disk Space:";
                            foreach (SysValues v in _SysData.GetDiskSpaces())
                                LogSysValueWithUsage(v, u);
                            break;
                        default:
                            break;
                    }
                }
                msg += "\r\n" + "Finaliza Proceso Timer ... ..." + host.HostName;
                ServiceLogTPVCOFO.Instance.WriteLine(msg + "\r\n" + "---------------------------------------------------",true);
            }
            catch (Exception e)
            {
                msg += "\r\n" + "Check " + host + ", Error: " + e.Message;
                ServiceLogTPVCOFO.Instance.WriteLine(msg + "\r\n" + "---------------------------------------------------",true);
            }
        }

        /// <summary>
        /// Método que se encargará de determinará el manejo de la respuesta en relación de la configuración BATCH para el Hilo
        /// </summary>
        /// <param name="iTipoRespuesta">Tipo de control de Respuesta --> No Controlado, Tiempo Real o batch  </param>
        /// <param name="bt"></param>
        /// <param name="strNombreArchBatch"></param>
        private void SendMessageByTipoRespuesta(int iTipoRespuesta, ServiceConfigTPVCOFO.Batch bt, String strNombreArchBatch, string strCarpetaOrigen = "", string strCarpetaDestino = "BatchEnviados", bool bolConFecha = true, bool pbolVolumen=false, bool pbolenParalelo=true)
        {
            switch (iTipoRespuesta)
            {
                case (int)TipoRespuesta.RPT_NO_MANAGE:
                    // no se realiza nada
                    break;
                case (int)TipoRespuesta.RPT_REALTIME:
                    ServiceLogTPVCOFO.Instance.WriteLine("Ejecución de Envío de respuesta en Línea" + host.HostName);
                    // configurar para realtime
                    // invocación de servicio
                    break;
                case (int)TipoRespuesta.RPT_BATCH:
                    // armar el batch y verificar si está en ejecución
                    SendMessageToBatch(bt, strNombreArchBatch,strCarpetaOrigen,strCarpetaDestino,bolConFecha, pbolVolumen, pbolenParalelo);
                    break;
            }
        }

        /// <summary>
        /// Método creado para simular la ejecución BATCH, si cumple las condiciones validadas se simulará el copiado de las Respuestas a la carpeta BatchEnviados.
        /// </summary>
        /// <param name="bt"> Variable tipo BATCH que contiene información requerida para la ejecución del proceso </param>
        /// <param name="strNombreArchBatch">Nombre del archivo BATCH</param>
        public void SendMessageToBatch(ServiceConfigTPVCOFO.Batch bt, string strNombreArchBatch, string strCarpetaOrigen = "", string strCarpetaDestino = "BatchEnviados", bool bolConFecha = true, bool pbolVolumen=false, bool pbolenParalelo = true)
        {
            DateTime fechaActual ;
            string strError = "";
            try
            {
                if (pbolenParalelo)
                {
                    if (bt != null)
                    {
                        DateTime curDate = DateTime.Now;
                        string date = ((bt.sNextExecutionDatetime == "") ? curDate.ToString("dd/MM/yyyy HH:mm:ss") : bt.sNextExecutionDatetime);

                        DateTime executionDate = Convert.ToDateTime(date);

                        if (DateTime.Now >= executionDate)
                        {
                            switch (bt.iExecutionState)
                            {
                                case (int)TipoExecuteBatch.NO_EXECUTE:

                                    if (bt.sNextExecutionDatetime == string.Empty)
                                    {
                                        bt.sNextExecutionDatetime = curDate.AddMinutes(bt.TimeMinuteBatchCycle).ToString("dd/MM/yyyy HH:mm:ss");
                                        ServiceLogTPVCOFO.Instance.WriteLine("\r\n" + "Asignación de Valores Iniciales y Ejecución de Primer Batch, sino existe configuración previa: " + bt.sNextExecutionDatetime + " " + strNombreArchBatch + "\r\n");
                                    }
                                    else
                                    {
                                        //bt.sNextExecutionDatetime = curDate.AddMinutes(bt.TimeMinuteBatchCycle).ToString("dd/MM/yyyy HH:mm:ss");
                                        DateTime fechaEjecucion = Convert.ToDateTime(bt.sNextExecutionDatetime);
                                        fechaActual = DateTime.Now;

                                        ServiceLogTPVCOFO.Instance.WriteLine("RptaBatch" + strNombreArchBatch + "\r\n RESPONSE: Fecha Actual : " + fechaActual.ToString() + " - Fecha Ejecucion : " + fechaEjecucion.ToString() + "\r\n");

                                        if (fechaActual > fechaEjecucion)
                                        {
                                                ServiceLogTPVCOFO.Instance.WriteLine("RptaBatch" + strNombreArchBatch + "\r\n RESPONSE 2: " + fechaActual.ToString() + "\r\n");
                                                bt.sNextExecutionDatetime = fechaActual.AddMinutes(bt.TimeMinuteBatchCycle).ToString("dd/MM/yyyy HH:mm:ss");
                                                bt.iExecutionState = (int)TipoExecuteBatch.EXECUTING;
                                                ServiceLogTPVCOFO.Instance.WriteLine("\r\n RptaBatch" + strNombreArchBatch + "Ejecución de Batch " + strNombreArchBatch + "\r\n");
                                                //Hacer traslado de archivo 
                                                MoverArchivoBatch(bt, strNombreArchBatch, strCarpetaOrigen, strCarpetaDestino, bolConFecha, pbolVolumen);
                                                bt.iExecutionState = (int)TipoExecuteBatch.NO_EXECUTE;
                                        }
                                    }
                                    break;

                                case (int)TipoExecuteBatch.EXECUTED:
                                    bt.iExecutionState = (int)TipoExecuteBatch.NO_EXECUTE;
                                    break;
                            }
                        }
                        else
                        {
                            bt.iExecutionState = (int)TipoExecuteBatch.NO_EXECUTE;
                            ServiceLogTPVCOFO.Instance.WriteLine("\r\n" + "No se encuentra Fichero " + strNombreArchBatch + "\r\n");
                        }
                    }
                }
                else
                {
                    if (bt != null)
                    {
                        bt.iExecutionState = (int)TipoExecuteBatch.EXECUTING;
                        ServiceLogTPVCOFO.Instance.WriteLine("\r\n RptaBatch" + strNombreArchBatch + ": Ejecución de Batch Secuencial: " + strNombreArchBatch + " | Carpeta Origen: "+ strCarpetaOrigen+ " - " + "Carpeta Destino: " + strCarpetaDestino + "\r\n",true);
                        MoverArchivoBatch(bt, strNombreArchBatch, strCarpetaOrigen, strCarpetaDestino, bolConFecha, pbolVolumen);
                        ServiceLogTPVCOFO.Instance.WriteLine("\r\n RptaBatch" + strNombreArchBatch + ": Fin de Batch Secuencial " + strNombreArchBatch + "\r\n", true);
                        bt.iExecutionState = (int)TipoExecuteBatch.NO_EXECUTE;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    strError = e.InnerException.Message;
                else
                    strError = e.Message;

                bt.iExecutionState = (int)TipoExecuteBatch.NO_EXECUTE;
                ServiceLogTPVCOFO.Instance.WriteLine("\r\n" + "Error en SendMessageToBatch: " + strError + "\r\n",true);
            }

        }

        private void fnMoveFileAll(string pstrPathSource,string pstrNombreArchBatch,string pstrPathDest, bool pbolconFecha)
        {
            List<string> lstFile = new List<string>();
            int intPos = 0;

            Generic.DirSearch(lstFile, pstrPathSource, pstrNombreArchBatch);

            if (lstFile != null)
            {
                foreach (string strPathFileSource in lstFile)
                {
                    intPos = strPathFileSource.LastIndexOf("\\");
                    string strFileDest = strPathFileSource.Substring(intPos + 1);
                    string strPathFileDest = System.IO.Path.Combine(pstrPathDest, strFileDest);
                    strPathFileDest = pbolconFecha == false ? strPathFileDest : strPathFileDest.Replace(".log", "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".log");
                    File.Move(strPathFileSource, strPathFileDest);
                }
            }
        }

        /// <summary>
        /// Método creado para mover el archivo donde almacenan los resultados a la carpeta BatchEnviados,  simular la ejecución BATCH.
        /// </summary>
        /// <param name="bt"> Variable tipo BATCH que contiene información requerida para la ejecución del proceso </param>
        /// <param name="fileName">Nombre del archivo BATCH</param> 
        public void MoverArchivoBatch(ServiceConfigTPVCOFO.Batch bt, string sFileName,string strCarpetaOrigen="", string strCarpetaDestino= "BatchEnviados",bool bolConFecha=true,bool bolVolumen=false) 
        {
            string sourceFile = "";
            string destFile = "";
            int pos = 0;
            string ModuleName = "";
            string sourcePath = "";
            string targetPath = "";
            string path = "";
            string strError = "";
            try
            {
                path = Assembly.GetExecutingAssembly().Location;
                pos = path.IndexOf("\\");
                path = path.Substring(0, pos);
                targetPath = path + @"\CETEL\ServiceTPVCOFO_Files\" + strCarpetaDestino;


                //if (bolTargeDir)
                //{
                //    path = Assembly.GetExecutingAssembly().Location;
                //    pos = path.IndexOf("\\");
                //    path = path.Substring(0, pos) + @"\CETEL\ServiceTPVCOFO_Files\";
                //    sourcePath = path + strCarpetaOrigen;
                //}
                //else
                //{
                    path = Assembly.GetExecutingAssembly().Location;
                    pos = path.IndexOf("\\");
                    path = path.Substring(0, pos);
                    sourcePath = path + @"\CETEL\ServiceTPVCOFO_Files\" + strCarpetaOrigen;
                //}

                if (!File.Exists(sFileName))
                {
                    sourceFile = System.IO.Path.Combine(sourcePath, sFileName+".log");
                    destFile = System.IO.Path.Combine(targetPath, sFileName+".log");
                }
                else
                {
                    sourceFile = sFileName;
                    pos = sFileName.LastIndexOf("\\");
                    ModuleName = sFileName.Substring(pos + 1);
                    destFile = System.IO.Path.Combine(targetPath, ModuleName);
                }

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                try
                {
                    if (File.Exists(sourceFile))
                    {
                        long length = new System.IO.FileInfo(sourceFile).Length;
                        if (length > 200)
                        {
                            ServiceLogTPVCOFO.Instance.WriteLine("Copia de Archivo Origen" + sourceFile + " Destino: " + destFile + "\r\n" + "---------------------------------------------------", true);

                            if (bolVolumen == false)
                            {
                                string destFileName = bolConFecha == false ? destFile : destFile.Replace(".log", "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".log");
                                File.Move(sourceFile, destFileName);
                            }
                            else
                                fnMoveFileAll(sourcePath, sFileName, targetPath, bolConFecha);
                        }
                    }
                    else
                    {
                        if (bolVolumen)
                            fnMoveFileAll(sourcePath, sFileName, targetPath, bolConFecha);
                    }
                }
                catch(Exception e) {
                    if (e.InnerException != null)
                        strError = e.InnerException.Message;
                    else
                        strError = e.Message;

                    ServiceLogTPVCOFO.Instance.WriteLine("Mover Archivo: " + strError + "\r\n" + "---------------------------------------------------", true);
                }

            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    strError = e.InnerException.Message;
                else
                    strError= e.Message;

                ServiceLogTPVCOFO.Instance.WriteLine("Error MoverArchivoBatch: " + strError + "\r\n" + "---------------------------------------------------",true);
            }
        }
        #endregion

        #region "Timer Proc Web Service"
        /// <summary>
        /// Método creado para varios envios según el numero de archivos que se encuentren en carpeta y filtrando por BatchName.
        /// </summary>
        /// <param name="pstrNombreArchBatch"> Nombre del BATCH </param>
        /// <param name="pstrRptaServicio">Respuesta del Servicio</param> 
        internal void fnInvokeServiceWriteVolumen(string pstrNombreArchBatch, out string pstrRptaServicio )
        {
            List<string> lstFile = new List<string>();
            string sModuleName = "";
            ResponseApi objResponseApi = null;
            string rptaServicio = string.Empty;

            string sPath = Assembly.GetExecutingAssembly().Location;
            int iPos = sPath.IndexOf("\\");
            sModuleName = sPath.Substring(iPos + 1);
            sPath = sPath.Substring(0, sPath.IndexOf(sModuleName))+ @"CETEL\ServiceTPVCOFO_Files\BatchPendientes\";
            Generic.DirSearch(lstFile, sPath, pstrNombreArchBatch);

            foreach (string strPathFileSource in lstFile)
            {
                //ILION- Se cambia a la invocacion del metodo secundario para el WS de producto.
                //objResponseApi = Generic.InvokeServiceWrite(strPathFileSource, SW);
                objResponseApi = Generic.InvokeServiceWriteParams(strPathFileSource, SW);

                if (objResponseApi.Status == Convert.ToInt32(ApiResponseStatuses.Successful))
                {
                    batchH = null;
                    rptaServicio = rptaServicio + pstrNombreArchBatch + ": " + objResponseApi.Message + "\r\n";
                    if (SW.BitSaveResponse)
                        Generic.DeleteTempFiles("BatchPendientes", pstrNombreArchBatch+"*.log", 0);
                    //    SendMessageByTipoRespuesta((SW.BitBatchResponse ? 2 : 1), batchConfig, strPathFileSource, "BatchPendientes", "BatchEnviados", true, false, false);
                    //else

                }
                else
                {
                    batchH = null;
                    rptaServicio = rptaServicio + pstrNombreArchBatch + ": " + objResponseApi.Message + "\r\n";
                }
            }

            pstrRptaServicio = rptaServicio;
        }

        /// <summary>
        /// Método que se desencadenara por cada vez que se ejecute el Timer Configurado para ejecución de los Hilos Tipo Web Service. 
        /// </summary>
        /// <param name="o"></param>
        internal void TimerProcWebService(object o)
        {

            try
            {
                string s = string.Empty;
                string rptaServicio = string.Empty;
                //ResponseApi objResponseApi = null;

                msg = "";
                msg += "\r\n" + "Inicia Timer Web Service  ... " + idxThread + " - " + SW.EndPoint;
                msg += "\r\n" + "------ Check Web Service ------";

                Thread.Sleep(1000 + (idxThread * 250));
                switch (SW.TypeService)
                {
                    case "READ":
                        rptaServicio = InvokeServiceRead();
                        break;
                    case "WRITE":
                        fnInvokeServiceWriteVolumen(SW.BatchName, out rptaServicio);
                        break;
                }
                
                msg += rptaServicio;
                if (batchH != null)
                    batchH.WriteLineBatch(SW.BatchName, rptaServicio);

                msg += "\r\n" + "Finaliza Proceso Timer WS... ..." + SW.EndPoint;
                ServiceLogTPVCOFO.Instance.WriteLine(msg + "\r\n" + "---------------------------------------------------",true);
            }
            catch (Exception e)
            {
                if( e.InnerException!= null)
                    ServiceLogTPVCOFO.Instance.WriteLine(e.InnerException.ToString());
                else
                    ServiceLogTPVCOFO.Instance.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// Método que se encarga de invocar al servicio utilizando la configuración sXML del archivo de configuración del Hilo.
        /// </summary> 
        /// <returns>El resultado de la invocación al servicio </returns>
        public string InvokeServiceRead()
        {
            var ServiceResult = "";
            XmlDocument SOAPReqBody = null;
            HttpWebRequest request = null;
            try
            {
                request = CreateSOAPWebRequest();
                SOAPReqBody = new XmlDocument();
                SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""> 
                                    <soap:Body>" + SW.XML + "</soap:Body></soap:Envelope>");


                using (Stream stream = request.GetRequestStream())
                {
                    SOAPReqBody.Save(stream);
                }

                using (WebResponse Serviceres = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                    {
                        ServiceResult = rd.ReadToEnd();
                        if (SW.BitSaveResponse)
                        { return "Servicio Devolvio Respuesta: " + ServiceResult; }
                        else { return "Servicio Devolvio Respuesta. Longitud: " + ServiceResult.ToString().Length; }

                    }
                }

            }
            catch (Exception e)
            {
                msg += "\r\n" + "Check " + SW.EndPoint + ", Error WS: " + e.Message + e.StackTrace + "COnsulta Error" + SW.EndPoint + "Detalle Consulta" + @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""> 
                         <soap:Body>" + SW.XML + "</soap:Body></soap:Envelope>";
                ServiceLogTPVCOFO.Instance.WriteLine(msg + "\r\n" + "---------------------------------------------------");
                return "Servicio No Devolvio Respuesta. InfoError: " + e.Message;

            }

        }

        /// <summary>
        /// Inicializa el HttpWebRequest requerido para la invocación genérica de WS
        /// </summary>
        /// <returns>HttpWebRequest con la instanciación requerida para la invocación al servicio</returns>
        public HttpWebRequest CreateSOAPWebRequest()
        {
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(SW.EndPoint);
            Req.Headers.Add(@"SOAP:Action");
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            Req.Method = "POST";
            return Req;
        }

        #endregion

        #region "Timer Proc Library"
            
        /// <summary>
        /// Método que se desencadenara por cada vez que se ejecute el Timer Configurado para ejecución de los Hilos Tipo Web Library. 
        /// </summary>
        /// <param name="o"></param>
        internal void TimerProcLibrary(object o)
        {
            string s = string.Empty;
            string ResponseLibrary = string.Empty;
            int intFileSize = 0;
            string strResultado = "(Archivo Vacío)";
            try
            {
                // ILION Original
                /*if (lib.BitSaveResponse)
                    SendMessageByTipoRespuesta((lib.BitBatchResponse ? 2 : 1), batchConfig, lib.BatchName,"", "BatchPendientes", false,true,false);*/

                msg = "";
                msg += "\r\n" + "Inicia Timer Library  ... " + idxThread + " - " + lib.BatchName + " " + lib.ClaseName + " " + lib.FuncionName + "(" + " " + lib.Parameters + ") " + "Tanks " + lib.Tanks;
                msg += "\r\n" + "------ Check Library ------";

                ResponseLibrary = LibraryResponse(lib.LibreriaName, lib.ClaseName, lib.FuncionName, lib.Parameters, lib.Tanks);

                intFileSize = ResponseLibrary.Length;
                if (intFileSize > 50)
                {
                    strResultado = "OK";
                    //if (lib.bitLogon)
                        ResponseLibrary = ResponseLibrary.Substring(1, intFileSize - 2);
  
                    ResponseLibrary += ",";
                    if (batchH != null)
                        batchH.WriteLineBatch(lib.BatchName, ResponseLibrary, 0, true, intFileSize);//acepta división de archivos por volumen de data (4MB)
                }
                else
                { ResponseLibrary = ""; }

                //DeleteTempFiles("BatchEnviados", lib.BatchName + "*.log",-15);
                Generic.DeleteTempFiles("Log",  "LOG*.log", -15);

                if (lib.BitSaveResponse && strResultado != "(Archivo Vacío)")
                    SendMessageByTipoRespuesta((lib.BitBatchResponse ? 2 : 1), batchConfig, lib.BatchName, "", "BatchPendientes", false, true, false);

                msg += "\r\n" + "Finaliza Proceso "+ strResultado + " Timer WS... ..." + " - " + lib.BatchName + " " + lib.ClaseName + " " + lib.FuncionName + "(" + " " + lib.Parameters + ") " + "Tanks " + lib.Tanks;
                ServiceLogTPVCOFO.Instance.WriteLine(msg + "\r\n" + "---------------------------------------------------",true);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    ServiceLogTPVCOFO.Instance.WriteLine(e.InnerException.ToString());
                }
                else
                {
                    ServiceLogTPVCOFO.Instance.WriteLine(e.Message.ToString());
                }
            }
        }

        /// <summary>
        /// Método para cargar Librerias Dinamicas se llamara desde el Método  TimerProcLibrary
        /// </summary>
        /// <param name="sLibName"></param>
        /// <param name="sClassName"></param>
        /// <param name="sFuncionName"></param>
        /// <param name="sParameters"></param>
        public string LibraryResponse(string sLibName, string sClassName, string sFuncionName, string sParameters, string sTanks)
        {
            string strError = "";
            try
            {
                string path = Assembly.GetExecutingAssembly().Location;
                string ModuleName = path.Substring(path.LastIndexOf("\\") + 1);
                path = path.Substring(0, path.IndexOf(ModuleName));

                Assembly a = Assembly.LoadFile(path + "Lib\\" + sLibName + ".dll");
                // Get the type to use.
                Type myType = a.GetType(sLibName + "." + sClassName);
                // Get the method to call.
                MethodInfo myMethod = myType.GetMethod(sFuncionName);
                // Create an instance.
                object obj = Activator.CreateInstance(myType);

                // ILION- Se coloca un mensaje al Log para ver que metodo invoca a la libreria.
                ServiceLogTPVCOFO.Instance.WriteLine("ILION- Se invoca la libreria -" + sLibName + ".dll con la funcion --" + sFuncionName);

                // Execute the method.
                if (sParameters == "")
                    return myMethod.Invoke(obj, null).ToString();
                else
                {
                    string[] stringItems = sParameters.Split('|');
                    object[] objectItems = (object[])stringItems;

                    //ILION- Se agrega la lista de tanques
                    if (lib.BatchName == "Batch_TankGauge" || lib.BatchName == "Batch_Deliverys")
                    {
                        objectItems = objectItems.Concat(new object[] { sTanks }).ToArray();
                    }

                    return myMethod.Invoke(obj, objectItems).ToString();
                }

            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    strError = e.InnerException.ToString();
                else
                    strError = e.ToString();
                ServiceLogTPVCOFO.Instance.WriteLine(strError);
                return "";
            }

        }

        ///// <summary>
        ///// Método para Eliminar Archivos para un periodo de tiempo dado.(15 dias hacia atras por defecto)
        ///// </summary>
        ///// <param name="strSource"></param>
        ///// <param name="strFilter"></param>
        ///// <param name="intNroDias"></param>
        //private void DeleteTempFiles(string strSource, string strFilter, int intNroDias)
        //{
        //    string path = Assembly.GetExecutingAssembly().Location;
        //    int pos = path.IndexOf("\\");

        //    int posXML = path.IndexOf(".exe");
        //    string filename = string.Empty;

        //    string ModuleName = path.Substring(pos + 1);
        //    path = path.Substring(0, path.IndexOf(ModuleName));
        //    string sourcePath = path + @"CETEL\ServiceTPVCOFO_Files\" + strSource+@"\";
        //    DateTime DateNow = DateTime.Now.AddDays(intNroDias);

        //    DirectoryInfo dirInfo = new DirectoryInfo(@sourcePath);

        //    FileInfo[] Files = dirInfo.GetFiles(strFilter)
        //            .Where(p => p.CreationTime.Date <= DateNow).ToArray();

        //    foreach (FileInfo fi in Files)
        //    {
        //        fi.Delete();
        //    }

        //    Thread.Sleep(100);
        //}
        #endregion
    }


}
