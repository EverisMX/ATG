using ServicioTPVAgenteLocal.BE;
using ServicioTPVAgenteLocal.Utility;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using ServicioTPVAgenteLocal.Utility.Enums;
using Newtonsoft.Json.Linq;
using System.ServiceProcess;
using System.Diagnostics;
using ServicioTPVAgenteLocal.Configuration;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using PSS_Forecourt_Lib;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

namespace ServicioTPVAgenteLocal
{
    public class ServiceThreadTPVCOFO
    {
        #region Vars
        MultiThread _multithread;
        ServiceConfigTPVCOFO _config;
        static Timer[] _myTimer = null;
        static int TOTAL_HOST = 0;
        static int TOTAL_WS = 0;
        static int TOTAL_SOCKET = 0;
        static int TOTAL_LIBRARY = 0;
        ServiceWorkerTPVCOFO[] worker = null;
        ServiceBatchTPVCOFO[] batch = null;
        ServiceConfigTPVCOFO.Batch[] batchConfig = null;
        ServiceConfigTPVCOFO.ServicioWeb objReconexion = null;
        static int intCantBatch = 0;
        Forecourt fcPrimary;
        IFCConfig ifcPrimary;
        Forecourt fcAlarm;
        IDomsPos idpAlarm;
        private Thread threadTgAlarms;
        string strUserID = "";
        string strStoreID = "";
        string strCodEmpresa = "";
        string strUrlServidor = "";
        string strPathApi = "";
        string strNameFile = "";
        // variable for lock concurrency
        private Object thisLock = new Object();
        //private Thread threadTgAlarms;
        #endregion //Vars

        /// <summary>
        /// Método que se ejecuta al inicial el Servicio de WIndows
        /// </summary>
        /// <param name="sAction">Accion asociada al inicio del Servicio: Iniciar</param>
        public void Start(string sAction)
        {
            #region Vars
            string strResultado;
            #endregion //Vars

            try
            {
                //DeleteTempFiles();
                ServiceLogTPVCOFO.Instance.WriteLine("===================================================");
                ServiceLogTPVCOFO.Instance.WriteLine(ServiceLogTPVCOFO.ModuleName + " Thread " + sAction, true);

                // ILION_MX - Se comienza en habilitar solo el control del archivo sin WS de XMLCOFO
                _config = GetXmlConfig();

                //_config = GetInitialXMLConfig(); // ILION_MX - Coment

                if (_config != null)
                {
                    //strResultado = getConfigurationXML(_config.ListServicioWeb[0]); // ILION_MX - Coment
                    strResultado = "OK";

                    if (strResultado == "OK")
                    {
                        //_config = GetXmlConfig(); // ILION_MX - Coment

                        if (_config != null)
                        {
                            ServiceLogTPVCOFO.Instance.WriteLine("ILION_MX - Comienza en la contruccion de los hilos con el XML cargado.");
                            worker = new ServiceWorkerTPVCOFO[TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY + 1];
                            batch = new ServiceBatchTPVCOFO[intCantBatch];
                            batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                            _myTimer = new Timer[TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY + 1];
                            _multithread = new MultiThread(TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY + 1);
                            CreateMultiThreads();

                            if (_config.ListAlarm != null)
                            {
                                ServiceLogTPVCOFO.Instance.WriteLine("***********Inicio de Activación de Alarmas ***********", true);
                                threadTgAlarms = new Thread(StartTgAlarmsListener);
                                threadTgAlarms.Start();
                                ServiceLogTPVCOFO.Instance.WriteLine("***********Fin de Activación de Alarmas ***********", true);
                            }
                            else
                            {
                                ServiceLogTPVCOFO.Instance.WriteLine("***********No se inicio Activación de Alarmas ***********", true);
                            }

                        }
                        else
                        {
                            ServiceLogTPVCOFO.Instance.WriteLine(ServiceLogTPVCOFO.ModuleName + " No se Ha Encontrado Configuración para procesar Hilos.", true);
                            if (_config != null)
                            {
                                worker = new ServiceWorkerTPVCOFO[2];
                                batch = new ServiceBatchTPVCOFO[intCantBatch];
                                batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                                _myTimer = new Timer[2];
                                _multithread = new MultiThread(2);
                                CreatePrimaryThreads(_config.ListServicioWeb, "Batch_Operation_Reconnection");
                            }

                        }
                    }
                    else if (strResultado == "NO")
                    {
                        ServiceLogTPVCOFO.Instance.WriteLine(ServiceLogTPVCOFO.ModuleName + " No se Ha Encontrado Configuración en Base, para procesar Hilos.", true);

                        if (_config != null)
                        {
                            worker = new ServiceWorkerTPVCOFO[2];
                            batch = new ServiceBatchTPVCOFO[intCantBatch];
                            batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                            _myTimer = new Timer[2];
                            _multithread = new MultiThread(2);
                            //CreateStopThreads(_config.ListServicioWeb);
                            CreatePrimaryThreads(_config.ListServicioWeb, "Batch_Configuration_Reconnection");
                        }

                    }
                    else
                    {
                        if (_config != null)
                        {
                            worker = new ServiceWorkerTPVCOFO[2];
                            batch = new ServiceBatchTPVCOFO[intCantBatch];
                            batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                            _myTimer = new Timer[2];
                            _multithread = new MultiThread(2);
                            CreatePrimaryThreads(_config.ListServicioWeb, "Batch_Operation_Reconnection");
                        }
                    }
                }
                else
                {
                    ServiceLogTPVCOFO.Instance.WriteLine(ServiceLogTPVCOFO.ModuleName + "No se ha Encontrado Configuración XML Inical.", true);
                    worker = new ServiceWorkerTPVCOFO[2];
                    batch = new ServiceBatchTPVCOFO[intCantBatch];
                    batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                    _myTimer = new Timer[2];
                    _multithread = new MultiThread(2);
                    CreatePrimaryThreads(_config.ListServicioWeb, "Batch_Operation_Reconnection");
                }
                ServiceLogTPVCOFO.Instance.GrabarLog();
                 
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    ServiceLogTPVCOFO.Instance.WriteLine("Error in Star Service COFO: " + e.InnerException.Message);
                else
                    ServiceLogTPVCOFO.Instance.WriteLine("Error in Star Service COFO: " + e.Message);
                ServiceLogTPVCOFO.Instance.GrabarLog();
                if (_config != null)
                {
                    worker = new ServiceWorkerTPVCOFO[2];
                    batch = new ServiceBatchTPVCOFO[intCantBatch];
                    batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                    _myTimer = new Timer[2];
                    _multithread = new MultiThread(2);
                    CreatePrimaryThreads(_config.ListServicioWeb, "Batch_Operation_Reconnection");
                }
            }

        }

        public void StopService(object o)
        {

            TimeSpan tmExpira = new TimeSpan(5000);
            using (ServiceController sc = new ServiceController("ServiceTPVATG"))
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, tmExpira);
            }
            ServiceLogTPVCOFO.Instance.WriteLine("Se ha Detenido el Servicio Correctamente.");
            ServiceLogTPVCOFO.Instance.GrabarLog();

            _multithread.Thread[0].Join(1000);
            _multithread.Thread[0].Abort();
            _multithread.Thread[1].Join(1000);
            _multithread.Thread[1].Abort();
            _myTimer[0].Dispose();
            _myTimer[1].Dispose();

        }

        /// <summary>
        /// Método que elimina archivos
        /// </summary>
        /// <param name="pathFile">Ruta del archivo seguida del Archivo a borrar.</param>
        public void DeleteFile(string pathFile)
        {
            try
            {
                string spath = "";
                string[] NameFileSplit = pathFile.Split('\\');
                int ipos2 = pathFile.IndexOf(NameFileSplit[NameFileSplit.Length - 1]);
                spath = pathFile.Substring(0, ipos2);

                if (File.Exists(pathFile))
                {
                    File.Delete(pathFile);
                }
            }
            catch (Exception ex)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error en metodo: DeleteFile." + ex.ToString());
                throw;
            }

        }

        /// <summary>
        /// Método que se ejecuta al parar el Servicio de WIndows
        /// </summary>
        /// <param name="sAction">Accion asociada al inicio del Servicio: Stop</param>
        public void Stop(string sAction)
        {
            try
            {

                string sPath = Assembly.GetExecutingAssembly().Location;
                int iPos = sPath.IndexOf(".exe");
                string sNameFile = sPath.Substring(0, iPos) + ".XML";
                //DeleteFile(sNameFile); // ILION - Se borra el XML

                ServiceLogTPVCOFO.Instance.WriteLine("Obtiene Configuración Inical - Stop", true);
                // ILION- Se comenta para que el XML no sea Precargado.
                //_config = GetInitialXMLConfig();
                ServiceLogTPVCOFO.Instance.WriteLine("ILION- Se cancelo la carga del XMLCOFO para no modificar el existente.");

                if (_myTimer != null)
                {
                    if (_myTimer.Count() > 0)
                    {
                        for (int i = 0; i < _myTimer.Count() - 1; i++)
                        {
                            _myTimer[i].Dispose();
                        }
                    }
                }

                if (_multithread != null)
                {
                    if (_multithread.Thread.Length > 0)
                    {
                        for (int i = 0; i < _multithread.Thread.Length - 1; i++)
                        {
                            _multithread.Thread[i].Join(1000);
                            _multithread.Thread[i].Abort();
                        }  
                    }
                }

                if (fcPrimary != null)
                {
                    ServiceLogTPVCOFO.Instance.WriteLine("Liberación de Conexión PSSPOS.", true);
                    fcPrimary.Disconnect();
                    fcPrimary = null;
                    ifcPrimary = null;
                }

                fnCleanAlarm();

                ServiceLogTPVCOFO.Instance.GrabarLog();
                _config = null;
            }
            catch (Exception e)
            {
                _config = null;
                if (e.InnerException != null)
                    ServiceLogTPVCOFO.Instance.WriteLine("Error in Stop Service COFO: " + e.InnerException.Message);
                else
                    ServiceLogTPVCOFO.Instance.WriteLine("Error in Stop Service COFO: " + e.Message);
                ServiceLogTPVCOFO.Instance.GrabarLog();
            }
        }

        public void Pause(string sAction)
        {
            ServiceLogTPVCOFO.Instance.WriteLine(ServiceLogTPVCOFO.ModuleName + " Thread " + sAction);
        }

        /// <summary>
        /// Método que se encarga de Llamar a una reconexión de la configuración inicial, con un tiempo Expecifico - X minutos.
        /// </summary>
        public void TimerCallReconection(object o)
        {
            string strResultado = "";
            int intTimeReconnection = 0;
            if (objReconexion != null)
            {
                intTimeReconnection = objReconexion.TimeCheckCycle / 1000 / 60;
                ServiceLogTPVCOFO.Instance.WriteLine("Se ha iniciado la TimerCallReconection.");
                strResultado = getConfigurationXML(objReconexion);
                if (strResultado == "OK")
                {
                    ServiceLogTPVCOFO.Instance.WriteLine("Se ha realizado la Reconexión correctamente.");
                    _multithread.Thread[0].Join(1000);
                    _multithread.Thread[0].Abort();
                    _multithread.Thread[1].Join(1000);
                    _multithread.Thread[1].Abort();
                    _myTimer[0].Dispose();
                    _myTimer[1].Dispose();
                    _config = GetXmlConfig();

                    if (_config != null)
                    {

                        ServiceLogTPVCOFO.Instance.WriteLine("Inicio Multihilos.");
                        worker = new ServiceWorkerTPVCOFO[TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY + 1];
                        batch = new ServiceBatchTPVCOFO[intCantBatch];
                        batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                        _myTimer = new Timer[TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY + 1];

                        _multithread = new MultiThread(TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY + 1);
                        CreateMultiThreads();
                        if (_config.ListAlarm != null)
                        {
                            ServiceLogTPVCOFO.Instance.WriteLine("***********Inicio de Activación de Alarmas ***********", true);
                            threadTgAlarms = new Thread(StartTgAlarmsListener);
                            threadTgAlarms.Start();
                            ServiceLogTPVCOFO.Instance.WriteLine("***********Fin de Activación de Alarmas ***********", true);
                        }
                        else
                        {
                            ServiceLogTPVCOFO.Instance.WriteLine("***********No se inicio Activación de Alarmas ***********", true);
                        }
                        ServiceLogTPVCOFO.Instance.WriteLine("Fin Multihilos.");
                    }
                }
                //else if (strResultado == "NO")
                //{

                //    ServiceLogTPVCOFO.Instance.WriteLine("No se ha encontrado Configuración en base. Reconexión II.");
                //    _multithread.Thread[0].Join(1000);
                //    _multithread.Thread[0].Abort();
                //    _multithread.Thread[1].Join(1000);
                //    _multithread.Thread[1].Abort();
                //    _myTimer[0].Dispose();
                //    _myTimer[1].Dispose();
                //    worker = new ServiceWorkerTPVCOFO[2];
                //    batch = new ServiceBatchTPVCOFO[intCantBatch];
                //    batchConfig = new ServiceConfigTPVCOFO.Batch[intCantBatch];
                //    _myTimer = new Timer[2];
                //    _multithread = new MultiThread(2);
                //    CreatePrimaryThreads(_config.ListServicioWeb, "Batch_Reconnection_II");

                //}
                else ServiceLogTPVCOFO.Instance.WriteLine("No Se logro la Reconexión por algún error con el Servicio Web de Configuración.");
                ServiceLogTPVCOFO.Instance.WriteLine("Fin: TimerCallReconection.. Se volverá a reintentar en " + intTimeReconnection.ToString() + " minuto(s)", true);

            }
        }

        /// <summary>
        /// Método que se encarga de crear el Hilo de Detención del Servicio.
        /// </summary>
        public void CreateStopThreads(ServiceConfigTPVCOFO.ServicioWeb[] parrServicioWeb)
        {
            int idxBatch = 0;
            Thread objThread = null;
            string strError = "";
            try
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Inicio Hilo Stop*******************", true);
                if (parrServicioWeb.Length > 1)
                {
                    if (parrServicioWeb[1].BitSaveResponse)
                    {
                        batchConfig[idxBatch] = new ServiceConfigTPVCOFO.Batch
                        {
                            BatchName = parrServicioWeb[1].BatchName,
                            iExecutionState = 0,
                            sNextExecutionDatetime = "",
                            //TimeMinuteBatchCycle = parrServicioWeb[0].TimeMinuteBatchCycle 
                        };
                        batch[idxBatch] = new ServiceBatchTPVCOFO();
                        worker[0] = new ServiceWorkerTPVCOFO(parrServicioWeb[1], 0, batch[idxBatch], batchConfig[idxBatch]);//
                        idxBatch += 1;
                    }
                    else
                        worker[0] = new ServiceWorkerTPVCOFO(parrServicioWeb[1], 0, null, null);

                    objThread = new Thread(ThreadStop);
                    objThread.Name = "" + (0);
                    objThread.Start(this);
                    _multithread.Thread[0] = objThread;

                    worker[1] = new ServiceWorkerTPVCOFO();
                    objThread = new Thread(ThreadLog);
                    objThread.Name = "" + (1);
                    objThread.Start(this);
                    _multithread.Thread[1] = objThread;
                }
                else {
                    ServiceLogTPVCOFO.Instance.WriteLine("No se tiene Confgiuración de Hilo para Detener Servicio.", true);
                }
                ServiceLogTPVCOFO.Instance.WriteLine("Fin Hilo Stop*******************", true);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    strError = e.InnerException.Message;
                else
                    strError = e.InnerException.Message;
                ServiceLogTPVCOFO.Instance.WriteLine("Error in Creación de Thread Primary " + strError);
            }
        }

        /// <summary>
        /// Método que se encarga de crear el Hilo Principal en función de la configuración del archivo XML Inicial.
        /// </summary>
        public void CreatePrimaryThreads(ServiceConfigTPVCOFO.ServicioWeb[] parrServicioWeb, string strTipoBatch)
        {
            int idxBatch = 0;
            Thread objThread = null;
            ServiceConfigTPVCOFO.ServicioWeb[] arrServicioWeb = null;
            string strError = "";
            try
            {
                arrServicioWeb = parrServicioWeb.Where(x => x.BatchName == strTipoBatch).ToArray();
                if (arrServicioWeb != null)
                {
                    if (arrServicioWeb.Length > 0)
                    {
                        objReconexion = arrServicioWeb[0];
                        if (objReconexion.BitSaveResponse)
                        {
                            batchConfig[idxBatch] = new ServiceConfigTPVCOFO.Batch
                            {
                                BatchName = objReconexion.BatchName,
                                iExecutionState = 0,
                                sNextExecutionDatetime = "",
                                //TimeMinuteBatchCycle = parrServicioWeb[0].TimeMinuteBatchCycle 
                            };
                            batch[idxBatch] = new ServiceBatchTPVCOFO();
                            worker[0] = new ServiceWorkerTPVCOFO(objReconexion, 0, batch[idxBatch], batchConfig[idxBatch]);//
                            idxBatch += 1;
                        }
                        else
                            worker[0] = new ServiceWorkerTPVCOFO(objReconexion, 0, null, null);

                        objThread = new Thread(ThreadPrimary);
                        objThread.Name = "" + (0);
                        objThread.Start(this);
                        _multithread.Thread[0] = objThread;

                        worker[1] = new ServiceWorkerTPVCOFO();
                        objThread = new Thread(ThreadLog);
                        objThread.Name = "" + (1);
                        objThread.Start(this);
                        _multithread.Thread[1] = objThread;
                    }
                    else
                    {
                        ServiceLogTPVCOFO.Instance.WriteLine("No se ha encontrado configuración inicial para la Reconexión." + strError);
                    }
                }
                else
                {
                    ServiceLogTPVCOFO.Instance.WriteLine("No se ha encontrado configuración inicial para la Reconexión." + strError);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    strError = e.InnerException.Message;
                else
                    strError = e.Message;
                ServiceLogTPVCOFO.Instance.WriteLine("Error in Creación de Thread Primary " + strError);
            }
        }

        /// <summary>
        /// Método que se encarga de crear los Multihilos en función de la configuración del archivo XML
        /// </summary>
        public void CreateMultiThreads()
        {
            try
            {
                int idxBatch = 0;
                Thread objThread = null;

                for (int i = 0; i < TOTAL_HOST; i++)
                {
                    if (_config.ListHosts != null)
                    {
                        if (_config.ListHosts[i].BitBatchResponse)
                        {
                            batchConfig[idxBatch] = new ServiceConfigTPVCOFO.Batch { BatchName = _config.ListHosts[i].BatchName, iExecutionState = 0, sNextExecutionDatetime = "", TimeMinuteBatchCycle = _config.ListHosts[i].TimeMinuteBatchCycle };
                            batch[idxBatch] = new ServiceBatchTPVCOFO();
                            worker[i] = new ServiceWorkerTPVCOFO(_config.ListHosts[i], _config.Usages, i, batch[idxBatch], batchConfig[idxBatch]);
                            idxBatch += 1;
                        }
                        else
                            worker[i] = new ServiceWorkerTPVCOFO(_config.ListHosts[i], _config.Usages, i, null, null);

                        objThread = new Thread(ThreadProc);
                        objThread.Name = "" + i;
                        objThread.Start(this);

                        _multithread.Thread[i] = objThread;
                    }
                }
                // ILION- Se coloca un validador para no colocar el WS d FuelingTank **
                for (int i = 0; i < TOTAL_WS; i++)
                {
                    //bool flagFuelling = false; // ILION- se coloca una bandera par omitir el idx y no hacer referencia a null. **
                    if (_config.ListServicioWeb != null)
                    {
                        if (_config.ListServicioWeb[i].BitSaveResponse)
                        {
                            //if (_config.ListServicioWeb[i].BatchName != "Batch_FuellingPoints")
                            //{
                                batchConfig[idxBatch] = new ServiceConfigTPVCOFO.Batch
                                {
                                    BatchName = _config.ListServicioWeb[i].BatchName,
                                    iExecutionState = 0,
                                    sNextExecutionDatetime = ""
                                    //, TimeMinuteBatchCycle = _config.ListServicioWeb[i].TimeMinuteBatchCycle
                                };
                                batch[idxBatch] = new ServiceBatchTPVCOFO();
                                worker[i + (TOTAL_HOST)] = new ServiceWorkerTPVCOFO(_config.ListServicioWeb[i], i, batch[idxBatch], batchConfig[idxBatch]);
                                //flagFuelling = true;
                            /*}
                            else
                            {
                                ServiceLogTPVCOFO.Instance.WriteLine("ILION- No se coloca la configuracion del WS de FuelingPoints.");
                                flagFuelling = false;
                            }*/
                        }
                        else
                            worker[i + (TOTAL_HOST)] = new ServiceWorkerTPVCOFO(_config.ListServicioWeb[i], i, null, null);

                        //if (flagFuelling) // ILION- el validador.
                        //{
                            objThread = new Thread(ThreadWS);
                            objThread.Name = "" + (i + TOTAL_HOST);
                            objThread.Start(this);
                            _multithread.Thread[i + TOTAL_HOST] = objThread;
                            idxBatch += 1;
                        //}
                    }
                }

                for (int i = 0; i < TOTAL_SOCKET; i++)
                {
                    if (_config.ListSocket != null)
                    {
                        if (_config.ListSocket[i].BitSaveResponse)
                        {
                            batchConfig[idxBatch] = new ServiceConfigTPVCOFO.Batch
                            {
                                BatchName = _config.ListSocket[i].BatchName,
                                iExecutionState = 0,
                                sNextExecutionDatetime = "",
                                TimeMinuteBatchCycle = _config.ListSocket[i].TimeMinuteBatchCycle
                            };
                            batch[idxBatch] = new ServiceBatchTPVCOFO();
                            worker[i + (TOTAL_HOST + TOTAL_WS)] = new ServiceWorkerTPVCOFO(_config.ListSocket[i], i, batch[idxBatch], batchConfig[idxBatch]);
                            idxBatch += 1;
                        }
                        else
                            worker[i + (TOTAL_HOST + TOTAL_WS)] = new ServiceWorkerTPVCOFO(_config.ListSocket[i], i, null, null);

                        objThread = new Thread(ThreadSocket);
                        objThread.Name = "" + (i + TOTAL_HOST + TOTAL_WS);
                        objThread.Start(this);
                        _multithread.Thread[i + TOTAL_HOST + TOTAL_WS] = objThread;
                    }
                }
                // ILION- Se comprueba que la Librery este inactiva. **
                if (TOTAL_LIBRARY > 0)
                {
                    bool blnLogueoDoms = false;
                    //bool flagFuelling = false; // ILION- Control para el Fueling.
                    for (int i = 0; i < TOTAL_LIBRARY; i++)
                    {
                        if (_config.ListLibrary != null)
                        {
                            if (_config.ListLibrary[i].BatchName != "Batch_Performance" && !blnLogueoDoms)
                            {
                                string[] stringItems = _config.ListLibrary[i].Parameters.Split('|');
                                object[] objectItems = (object[])stringItems;
                                blnLogueoDoms = Generic.fnLogonPSSPOS(objectItems[0].ToString(), objectItems[1].ToString(), out fcPrimary, out ifcPrimary, objectItems[5].ToString());
                            }
                            if (_config.ListLibrary[i].BitSaveResponse)
                            {
                                // ILION- Se agrega una condicion para que no se agarre los fuelings y las alams.
                                //if (_config.ListLibrary[i].BatchName != "Batch_FuellingPoints")
                                //{
                                    batchConfig[idxBatch] = new ServiceConfigTPVCOFO.Batch
                                    {
                                        BatchName = _config.ListLibrary[i].BatchName,
                                        iExecutionState = 0,
                                        sNextExecutionDatetime = "",
                                        //TimeMinuteBatchCycle = _config.ListLibrary[i].TimeMinuteBatchCycle
                                    };
                                    batch[idxBatch] = new ServiceBatchTPVCOFO();
                                    worker[i + (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)] = new ServiceWorkerTPVCOFO(_config.ListLibrary[i], i, batch[idxBatch], batchConfig[idxBatch], fcPrimary, ifcPrimary);
                                    //flagFuelling = true;
                                /*}
                                else
                                {
                                    ServiceLogTPVCOFO.Instance.WriteLine("ILION- No se coloca la configuracion de FuelingPoints en Librery.");
                                    flagFuelling = false;
                                }*/
                            }
                            else
                                worker[i + (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)] = new ServiceWorkerTPVCOFO(_config.ListLibrary[i], i, null, null, fcPrimary, ifcPrimary);

                            //if (flagFuelling)
                            //{
                                objThread = new Thread(ThreadLibrary);
                                objThread.Name = "" + (i + TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET);
                                objThread.Start(this);
                                _multithread.Thread[i + TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET] = objThread;
                            //}
                        }
                    }
                }

                worker[TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY] = new ServiceWorkerTPVCOFO();
                objThread = new Thread(ThreadLog);
                objThread.Name = "" + (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY);
                objThread.Start(this);
                _multithread.Thread[(TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET + TOTAL_LIBRARY)] = objThread;
                idxBatch += 1;
            }
            catch (Exception e)
            {
                if (fcPrimary != null)
                {
                    fcPrimary.Disconnect();
                    fcPrimary = null;
                    ifcPrimary = null;
                }

                if (e.InnerException != null)
                    ServiceLogTPVCOFO.Instance.WriteLine("Error in Creación de Thread " + e.InnerException.Message);
                else
                    ServiceLogTPVCOFO.Instance.WriteLine("Error in Creación de Thread " + e.Message);
            }
        }

        /// <summary>
        /// Método que se encarga de crear/leer el archivo de configuración XML Inicial, si no existe en la ruta de la App se crea con la estructura base definida.
        /// </summary>
        /// <returns>Objeto de tipo ServiceConfigTPVCOFO</returns>
        private ServiceConfigTPVCOFO GetInitialXMLConfig()
        {
            #region Vars
            string strPath;
            string iniPath = @"C:\Cetel\SpecialParameters.ini";
            int intPos;
            XmlSerializer objXmlSerializer;
            ServiceConfigTPVCOFO.ServicioWeb[] arrServicioWeb;
            string strTypeService;
            bool bolBitSaveResponse;
            bool bolBitBatchResponse;
            string strEnvironment;
            TextWriter objTextWriter;
            TextReader objTextReader;
            string strUrlServidor = "";
            #endregion //Vars

            #region Var Initialization
            objXmlSerializer = new XmlSerializer(typeof(ServiceConfigTPVCOFO));
            strTypeService = "WRITE";
            bolBitSaveResponse = true;
            bolBitBatchResponse = true;
            #endregion //Var Initialization

            try
            {
                TOTAL_WS = 2;
                //Sacar la ruta donde se encuentra el fichero
                strPath = Assembly.GetExecutingAssembly().Location;
                intPos = strPath.IndexOf(".exe");
                //**********Comentado Solicicitud de Ricardo Taboada***********************************
                //double threshold = 0;
                //Array ary = null;
                //**********Comentado Solicicitud de Ricardo Taboada***********************************
                strPath = strPath.Substring(0, intPos) + ".XML";

                //Comprobar si existe el fichero
                if (!File.Exists(strPath))
                {
                    #region El fichero no existe

                    ServiceLogTPVCOFO.Instance.WriteLine("Inicio Configuración Inicial.Sin crear");
                    //Comprobar entorno donde se encuentra el TPV y escribir EndPoint correspondiente
                    if(File.Exists(iniPath))
                    {
                        strEnvironment = File.ReadLines(@"C:\Cetel\SpecialParameters.ini").Skip(1).Take(1).First();
                        strEnvironment = strEnvironment.Substring(strEnvironment.IndexOf('=') + 1);
                        switch (strEnvironment)
                        {
                            //Entorno DES
                            case @"{4A57EAF7-71F9-471A-9EB0-BF1E3DAA827D}":
                                strUrlServidor = @"http://heraeveriliondev.cloudapp.net/ilionservices4/WS_MX_COFOWSInterface/";
                                break;
                            //Entorno PRE
                            case @"5C46414A-BAE6-48A5-B48A-6AC67C6FD6C1":
                                strUrlServidor = @"http://testenv.everilion.com/ilionservices4/WS_MX_COFOWSInterface/";
                                break;
                            default:
                                strUrlServidor = @"http://www.ilionsistemas.com/ilionservices4/WS_MX_COFOWSInterface/";
                                break;
                        }
                    }
                    else
                    {
                        strUrlServidor = @"http://heraeveriliondev.cloudapp.net/ilionservices4/WS_MX_COFOWSInterface/";
                    }
                    
                    arrServicioWeb = new ServiceConfigTPVCOFO.ServicioWeb[3];

                    #region ArrServicioWeb[0]
                    arrServicioWeb[0] = new ServiceConfigTPVCOFO.ServicioWeb
                    {
                        TypeService = strTypeService,
                        EndPoint = strUrlServidor + "api/Configuration/GetServicesConfiguration/Request",
                        XML = string.Empty,
                        TimeStartDelay = 0,
                        TimeCheckCycle = 120000,
                        BitSaveResponse = bolBitSaveResponse,
                        BitBatchResponse = bolBitBatchResponse,
                        BatchName = "Batch_Operation_Reconnection",
                        JsonName = "ConfigurationRequest"
                    };
                    #endregion //ArrServicioWeb[0]

                    #region ArrServicioWeb[1]
                    arrServicioWeb[1] = new ServiceConfigTPVCOFO.ServicioWeb
                    {
                        TypeService = strTypeService,
                        EndPoint = string.Empty,
                        XML = string.Empty,
                        TimeStartDelay = 0,
                        TimeCheckCycle = 120000,
                        BitSaveResponse = bolBitSaveResponse,
                        BitBatchResponse = bolBitBatchResponse,
                        BatchName = "Batch_Stop",
                        JsonName = "ConfigurationStop"
                    };
                    #endregion //ArrServicioWeb[1]

                    #region ArrServicioWeb[2]
                    arrServicioWeb[2] = new ServiceConfigTPVCOFO.ServicioWeb
                    {
                        TypeService = strTypeService,
                        EndPoint = strUrlServidor + "api/Configuration/GetServicesConfiguration/Request",
                        XML = string.Empty,
                        TimeStartDelay = 0,
                        TimeCheckCycle = 900000,
                        BitSaveResponse = bolBitSaveResponse,
                        BitBatchResponse = bolBitBatchResponse,
                        BatchName = "Batch_Configuration_Reconnection",
                        JsonName = "ConfigurationRequest"
                    };
                    #endregion //ArrServicioWeb[2]

                    _config = new ServiceConfigTPVCOFO
                    {
                        ServiceName = ServiceLogTPVCOFO.ModuleName,
                        ListServicioWeb = arrServicioWeb,
                    };

                    intCantBatch = 1;
                    using (objTextWriter = new StreamWriter(strPath))
                    {
                        objXmlSerializer.Serialize(objTextWriter, _config);
                    }

                    ServiceLogTPVCOFO.Instance.WriteLine("Fin de Configuración Inicial. Ha sido creada con éxito.");
                    #endregion
                }
                else
                {
                    TOTAL_WS = 0;
                    #region El fichero existe
                    using (objTextReader = new StreamReader(strPath))
                    {
                        _config = objXmlSerializer.Deserialize(objTextReader) as ServiceConfigTPVCOFO;

                        if (_config.ListServicioWeb != null)
                        {
                            foreach (ServiceConfigTPVCOFO.ServicioWeb value in _config.ListServicioWeb)
                            {
                                TOTAL_WS +=1;
                                if (value.BitSaveResponse)
                                { intCantBatch += 1; }
                            }
                        }
                        ServiceLogTPVCOFO.Instance.WriteLine("Se ha Obtenido Configuración Inicial");
                    }
                    #endregion
                }

                return _config;
            }
            catch (Exception ex)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error in XmlSerializer TextReader: " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);

                return null;
            }
        }

        /// <summary>
        /// Método que se encarga de crear/leer el archivo de configuración XML, si no existe en la ruta de la App se crea con la estructura base definida.
        /// </summary>
        /// <returns>Objeto de tipo ServiceConfigTPVCOFO</returns>
        private ServiceConfigTPVCOFO GetXmlConfig()
        {
            #region Vars
            string strPath;
            int intPos;
            XmlSerializer objXmlSerializer;
            ServiceConfigTPVCOFO.ServicioWeb[] arrServicioWeb;
            ServiceConfigTPVCOFO.Socket[] arrSocket;
            ServiceConfigTPVCOFO.Library[] arrLibrary;
            string strEnvironment;
            string[] arrEndPoint;
            string[] arrWSXML;
            string[] arrJson;
            string[] arrEndSocketIP;
            string[] arrEndSocketPuerto;
            string[] arrLibraryName;
            string[] arrLibraryClassName;
            string[] arrLibraryClasFunction;
            string[] arrLibraryClasParameter;
            bool bolBitSaveResponse;
            bool bolBitBatchResponse;
            string strNombreBatch;
            string strType;
            TextWriter objTextWriter;
            TextReader objReader;
            #endregion //Vars

            #region Var Initialization
            objXmlSerializer = new XmlSerializer(typeof(ServiceConfigTPVCOFO));
            arrEndPoint = new string[TOTAL_WS];
            #endregion //Var Initialization

            try
            {
                //Localizar fichero
                strPath = Assembly.GetExecutingAssembly().Location;
                intPos = strPath.IndexOf(".exe");
                //**********Comentado Solicicitud de Ricardo Taboada***********************************
                //double threshold = 0;
                //Array ary = null;
                //**********Comentado Solicicitud de Ricardo Taboada***********************************
                strPath = strPath.Substring(0, intPos) + ".XML";

                //Comprobar si existe el fichero
                if (!File.Exists(strPath))
                {
                    TOTAL_WS = 3;
                    TOTAL_LIBRARY = 3;
                    #region El fichero no existe
                    #region
                    //public string LibreriaName { get; set; }
                    //[XmlAttribute()]
                    //public string ClaseName { get; set; }
                    //[XmlAttribute()]
                    //public string FuncionName { get; set; }
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    //ServiceConfigTPVCOFO.Host[] ArrHosts = new ServiceConfigTPVCOFO.Host[TOTAL_HOST];
                    #endregion

                    #region Arrays
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    arrServicioWeb = new ServiceConfigTPVCOFO.ServicioWeb[TOTAL_WS];
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    arrSocket = new ServiceConfigTPVCOFO.Socket[TOTAL_SOCKET];
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    arrLibrary = new ServiceConfigTPVCOFO.Library[TOTAL_LIBRARY];

                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    //string[] arrHosts = new string[] { "www.google.com", "www.microsoft.com", "www.cnn.com" };
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************

                    //Comprobar entorno donde se encuentra el TPV y escribir EndPoints Correspondientess
                    strEnvironment = File.ReadLines(@"C:\Cetel\SpecialParameters.ini").Skip(1).Take(1).First();
                    strEnvironment = strEnvironment.Substring(strEnvironment.IndexOf('=') + 1);
                    switch (strEnvironment)
                    {
                        //Entorno DES
                        case @"{4A57EAF7-71F9-471A-9EB0-BF1E3DAA827D}":
                            arrEndPoint = new string[] {
                                @"http://heraeveriliondev.cloudapp.net/ilionservices4/WS_MX_COFOWSInterface/api/TankGauge/setAllTankGaugeDataHistory/Request",
                                @"http://heraeveriliondev.cloudapp.net/ilionservices4/WS_MX_COFOWSInterface/api/TankGauge/setAllTankGaugeDataDeliveryHistory/Request",
                                @"http://heraeveriliondev.cloudapp.net/ilionservices4/WS_MX_COFOWSInterface/api/TankGauge/setAllTankGaugeFuellingDataHistory/Request"
                            };
                            break;
                        //Entorno PRE
                        case @"5C46414A-BAE6-48A5-B48A-6AC67C6FD6C1":
                            arrEndPoint = new string[] {
                                @"http://testenv.everilion.com/ilionservices4/WS_MX_COFOWSInterface/api/TankGauge/setAllTankGaugeDataHistory/Request",
                                @"http://testenv.everilion.com/ilionservices4/WS_MX_COFOWSInterface/api/TankGauge/setAllTankGaugeDataDeliveryHistory/Request",
                                @"http://testenv.everilion.com/ilionservices4/WS_MX_COFOWSInterface/api/TankGauge/setAllTankGaugeFuellingDataHistory/Request"
                            };
                            break;
                    }

                    arrWSXML = new string[] { @"", @"", @"" };
                    arrJson = new string[] { "TankGaugeHistoryList", "tankGaugeDataDeliveryList", "tankGaugeFuellingPoingList" };

                    arrEndSocketIP = new string[] { "10.232.100.191", "127.0.0.1", "10.232.100.62" };
                    arrEndSocketPuerto = new string[] { "11000", "11111", "11001" };


                    arrLibraryName = new string[] { "DOMSLibrary", "DOMSLibrary", "DOMSLibrary" };
                    arrLibraryClassName = new string[] { "DOMSTankGauge", "DOMSTankGauge", "DOMSTankGauge" };
                    arrLibraryClasFunction = new string[] { "fnObtenerTankGaugeData", "fnObtenerTankGaugeDelivery", "fnObtenerPuntoCombustible" };
                    arrLibraryClasParameter = new string[] { "83.56.16.44|69|02957|02957TRU|02957USR|VIRROG001_69", "83.56.16.44|71|02957|02957TRU|02957USR|VIRROG001_71", "83.56.16.44|72|02957|02957TRU|02957USR|VIRROG001_72" };
                    #endregion //Arrays

                    //string pstrHost, byte pbytPosId, string pscompany, string pstoreID, string psUserID'
                    //bool bolLogon = false;

                    #region
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    ////for (int i = 0; i < TOTAL_HOST; i++)
                    ////{
                    ////    strNombreBatch = "";
                    ////    bBitSaveResponse = false;
                    ////    bBitBatchResponse = false;

                    ////    ArrHosts[i] = new ServiceConfigTPVCOFO.Host
                    ////    {
                    ////        HostName = arrHosts[i],
                    ////        TimeStartDelay = 2000,
                    ////        TimeCheckCycle = 60000,
                    ////        TimeStopTimeout = 10000,
                    ////        BitBatchResponse = bBitBatchResponse,
                    ////        BitSaveResponse = bBitSaveResponse,
                    ////        BatchName = strNombreBatch,
                    ////        TimeMinuteBatchCycle = 5

                    ////    };

                    ////}
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    #endregion

                    #region For WS
                    for (int i = 0; i < TOTAL_WS; i++)
                    {
                        //Reinicializar variables
                        bolBitSaveResponse = false;
                        bolBitBatchResponse = false;
                        strNombreBatch = string.Empty;
                        strType = string.Empty;

                        switch (i)
                        {
                            case 0:
                                bolBitSaveResponse = true;
                                bolBitBatchResponse = true;
                                strNombreBatch = "Batch_TankGauge";
                                strType = "WRITE";
                                break;
                            case 1:
                                bolBitSaveResponse = true;
                                bolBitBatchResponse = true;
                                strNombreBatch = "Batch_Deliverys";
                                strType = "WRITE";
                                break;
                            case 2:
                                bolBitSaveResponse = true;
                                bolBitBatchResponse = true;
                                strNombreBatch = "Batch_FuellingPoints";
                                strType = "WRITE";
                                break;
                        }
                        arrServicioWeb[i] = new ServiceConfigTPVCOFO.ServicioWeb
                        {
                            TypeService = strType,
                            EndPoint = arrEndPoint[i],
                            XML = arrWSXML[i],
                            TimeStartDelay = 2000,
                            TimeCheckCycle = 120000,
                            BitSaveResponse = bolBitSaveResponse,
                            BitBatchResponse = bolBitBatchResponse,
                            BatchName = strNombreBatch,
                            JsonName = arrJson[i]
                        };
                    }
                    #endregion

                    #region
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    ////for (int i = 0; i < TOTAL_SOCKET; i++)
                    ////{
                    ////    strNombreBatch = "";
                    ////    bBitSaveResponse = false;
                    ////    bBitBatchResponse = false;
                    ////    switch (i)
                    ////    {
                    ////        case 0: bBitSaveResponse = true; bBitBatchResponse = true; strNombreBatch = "Batch_SOCKET_10.232.100.62_11000"; break;
                    ////        case 2: bBitSaveResponse = true; bBitBatchResponse = true; strNombreBatch = "Batch_SOCKET_127.0.0.1_11111"; break;
                    ////    }
                    ////    ArrSocket[i] = new ServiceConfigTPVCOFO.Socket
                    ////    {
                    ////        IP = arrEndSocketIP[i],
                    ////        Puerto = Convert.ToInt32(arrEndSocketPuerto[i]),
                    ////        TimeStartDelay = 2000,
                    ////        TimeStopTimeout = 1000,
                    ////        TimeCheckCycle = 120000,
                    ////        BitSaveResponse = bBitSaveResponse,
                    ////        BitBatchResponse = bBitBatchResponse,
                    ////        BatchName = strNombreBatch,
                    ////        TimeMinuteBatchCycle = 5

                    ////    };
                    ////}
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    #endregion

                    #region For Library
                    for (int i = 0; i < TOTAL_LIBRARY; i++)
                    {
                        strNombreBatch = "";
                        bolBitSaveResponse = true;
                        bolBitBatchResponse = true;
                        switch (i)
                        {
                            case 0:
                                bolBitSaveResponse = true;
                                bolBitBatchResponse = true;
                                strNombreBatch = "Batch_TankGauge";
                                break;
                            case 1:
                                bolBitSaveResponse = true;
                                bolBitBatchResponse = true;
                                strNombreBatch = "Batch_Deliverys";
                                break;
                            case 2:
                                bolBitSaveResponse = true;
                                bolBitBatchResponse = true;
                                strNombreBatch = "Batch_FuellingPoints";
                                break;
                            case 3:
                                bolBitSaveResponse = true;
                                bolBitBatchResponse = true;
                                strNombreBatch = "Batch_Performance";
                                break;
                        }
                        arrLibrary[i] = new ServiceConfigTPVCOFO.Library
                        {
                            LibreriaName = arrLibraryName[i],
                            ClaseName = arrLibraryClassName[i],
                            FuncionName = arrLibraryClasFunction[i],
                            Parameters = arrLibraryClasParameter[i],
                            TimeStartDelay = 2000,
                            TimeCheckCycle = 120000,
                            BitSaveResponse = bolBitSaveResponse,
                            BitBatchResponse = bolBitBatchResponse,
                            BatchName = strNombreBatch,
                            Tanks = "1-0|2-1|3-2"
                            //bitLogon=bolLogon
                        };
                    }
                    #endregion

                    _config = new ServiceConfigTPVCOFO
                    {
                        ServiceName = ServiceLogTPVCOFO.ModuleName,
                        //**********Comentado Solicicitud de Ricardo Taboada***********************************
                        //ListHosts = ArrHosts,
                        //**********Comentado Solicicitud de Ricardo Taboada***********************************
                        ListServicioWeb = arrServicioWeb,
                        //**********Comentado Solicicitud de Ricardo Taboada***********************************
                        ListSocket = arrSocket,
                        //**********Comentado Solicicitud de Ricardo Taboada***********************************
                        ListLibrary = arrLibrary
                    };

                    #region
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    ////threshold = 10;
                    ////ary = Enum.GetValues(typeof(ServiceConfigTPVCOFO.DeviceType));
                    ////_config.Usages = new ServiceConfigTPVCOFO.Usage[ary.Length];
                    ////foreach (ServiceConfigTPVCOFO.DeviceType value in ary)
                    ////{
                    ////    _config.Usages[(int)value] = new ServiceConfigTPVCOFO.Usage { DeviceID = value, Enable = true, Threshold = threshold += 10 };
                    ////}
                    //**********Comentado Solicicitud de Ricardo Taboada***********************************
                    #endregion

                    intCantBatch = 6;
                    using (objTextWriter = new StreamWriter(strPath))
                    {
                        objXmlSerializer.Serialize(objTextWriter, _config);
                    }

                    ServiceLogTPVCOFO.Instance.WriteLine("Info: Service Configuration created");
                    #endregion
                }
                else
                {
                    #region El fichero existe
                    try
                    {
                        TOTAL_WS = 0;
                        TOTAL_LIBRARY = 0;
                        using (objReader = new StreamReader(strPath))
                        {
                            _config = objXmlSerializer.Deserialize(objReader) as ServiceConfigTPVCOFO;

                            if (_config.ListHosts != null)
                            {
                                foreach (ServiceConfigTPVCOFO.Host value in _config.ListHosts)
                                {
                                    TOTAL_HOST += 1;
                                    if (value.BitSaveResponse)
                                    { intCantBatch += 1; }
                                }
                            }

                            if (_config.ListServicioWeb != null)
                            {
                                foreach (ServiceConfigTPVCOFO.ServicioWeb value in _config.ListServicioWeb)
                                {
                                    TOTAL_WS += 1;
                                    if (value.BitSaveResponse)
                                    { intCantBatch += 1; }
                                }
                            }

                            if (_config.ListSocket != null)
                            {
                                foreach (ServiceConfigTPVCOFO.Socket value in _config.ListSocket)
                                {
                                    TOTAL_SOCKET += 1;
                                    if (value.BitSaveResponse)
                                    { intCantBatch += 1; }
                                }
                            }

                            if (_config.ListLibrary != null)
                            { 
                                foreach (ServiceConfigTPVCOFO.Library value in _config.ListLibrary)
                                {
                                    TOTAL_LIBRARY += 1;
                                    if (value.BitSaveResponse)
                                    { intCantBatch += 1; }
                                }
                            }

                            ServiceLogTPVCOFO.Instance.WriteLine("Info: Service Configuration retrieved");
                            ServiceLogTPVCOFO.Instance.GrabarLog();
                        }
                    }
                    catch (Exception e)
                    {
                        ServiceLogTPVCOFO.Instance.WriteLine("Error in XmlSerializer TextReader: " + e.Message);
                        ServiceLogTPVCOFO.Instance.GrabarLog();
                    }
                    #endregion
                }

                return _config;

            }
            catch (Exception e)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error in XmlSerializer TextReader: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Método que se se encargar de iniciar el Hilo de Reconexión - 15 minutos.
        /// </summary>
        /// <param name="o"></param>
        public void ThreadStop(object o)
        {
            try
            {
                int idx = 0;
                try
                {
                    idx = Convert.ToInt32(Thread.CurrentThread.Name);
                    ServiceLogTPVCOFO.Instance.WriteLine("Thread Primary called Reconection: " + idx + " " + _config.ListServicioWeb[1].EndPoint + " - StartDelay: " + _config.ListServicioWeb[1].TimeStartDelay + " - CheckCycle: " + _config.ListServicioWeb[1].TimeCheckCycle);
                    _myTimer[idx] = new Timer(StopService, o, _config.ListServicioWeb[1].TimeStartDelay, _config.ListServicioWeb[1].TimeCheckCycle);
                }
                catch (Exception e)
                {
                    ServiceLogTPVCOFO.Instance.WriteLine("Error Create Thread Primary Reconection: " + e.Message + " IDX:" + idx);
                }
            }
            catch (Exception e)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error Create ThreadProc Primary: " + e.Message + " " + worker.Count());
            }
        }

        /// <summary>
        /// Método que se se encargar de iniciar el Hilo de Reconexión - X minuto(s).
        /// </summary>
        /// <param name="o"></param>
        public void ThreadPrimary(object o)
        {
            try
            {
                int idx = 0;
                try
                {
                    idx = Convert.ToInt32(Thread.CurrentThread.Name);
                    ServiceLogTPVCOFO.Instance.WriteLine("Thread Primary called Reconection: " + idx + " " + objReconexion.EndPoint + " - StartDelay: " + objReconexion.TimeStartDelay + " - CheckCycle: " + objReconexion.TimeCheckCycle);
                    _myTimer[idx] = new Timer(TimerCallReconection, o, objReconexion.TimeStartDelay, objReconexion.TimeCheckCycle);
                }
                catch (Exception e)
                {

                    ServiceLogTPVCOFO.Instance.WriteLine("Error Create Thread Primary Reconection: " + e.Message + " IDX:" + idx);

                }
            }
            catch (Exception e)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error Create ThreadProc Primary: " + e.Message + " " + worker.Count());
            }
        }

        /// <summary>
        /// Método que se se encargar de inicial el Hilo de tipo Host
        /// </summary>
        /// <param name="o"></param>
        public void ThreadProc(object o)
        {
            try
            {
                int idx = Convert.ToInt32(Thread.CurrentThread.Name);
                ServiceLogTPVCOFO.Instance.WriteLine("Thread Proc called Host: " + idx + " " + _config.ListHosts[idx].HostName + " - StartDelay: " + _config.ListHosts[idx].TimeStartDelay + " - CheckCycle: " + _config.ListHosts[idx].TimeCheckCycle);
                _myTimer[idx] = new Timer(worker[idx].TimerProcHost, o, _config.ListHosts[idx].TimeStartDelay, _config.ListHosts[idx].TimeCheckCycle);
            }
            catch (Exception e)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error Create ThreadProc Host: " + e.Message + " " + worker.Count());
            }
        }

        /// <summary>
        /// Método que se se encargar de inicial el Hilo de tipo Web Service
        /// </summary>
        /// <param name="o"></param>
        public void ThreadWS(object o)
        {
            int idx = 0;
            try
            {
                //ILION- Se comentara la parte del WS para el volcado.
                //ServiceLogTPVCOFO.Instance.WriteLine("ILION- Se omitira la configuracion de los metodos de WS de volcado de los batchs");

                idx = Convert.ToInt32(Thread.CurrentThread.Name);
                ServiceLogTPVCOFO.Instance.WriteLine("Thread Proc called WS: " + idx + " " + _config.ListServicioWeb[idx - TOTAL_HOST].EndPoint + " - StartDelay: " + _config.ListServicioWeb[idx - TOTAL_HOST].TimeStartDelay + " - CheckCycle: " + _config.ListServicioWeb[idx - TOTAL_HOST].TimeCheckCycle);
                _myTimer[idx] = new Timer(worker[idx].TimerProcWebService, o, _config.ListServicioWeb[idx - TOTAL_HOST].TimeStartDelay, _config.ListServicioWeb[idx - TOTAL_HOST].TimeCheckCycle);
            }
            catch (Exception e)
            {

                ServiceLogTPVCOFO.Instance.WriteLine("Error Create ThreadProc WS: " + e.Message + " IDX:" + idx + " Total Host:" + TOTAL_HOST);

            }
        }

        /// <summary>
        /// Método que se se encargar de inicial el Hilo de tipo Socket
        /// </summary>
        /// <param name="o"></param>
        public void ThreadSocket(object o)
        {
            int idx = 0;
            try
            {
                idx = Convert.ToInt32(Thread.CurrentThread.Name);
                ServiceLogTPVCOFO.Instance.WriteLine("Thread Proc called Socket: " + idx + " sIP:" + _config.ListSocket[idx - (TOTAL_HOST + TOTAL_WS)].IP + " iPuerto:" + _config.ListSocket[idx - (TOTAL_HOST + TOTAL_WS)].Puerto + " - CheckCycle: " + _config.ListSocket[idx - (TOTAL_HOST + TOTAL_WS)].TimeCheckCycle);
                _myTimer[idx] = new Timer(worker[idx].TimerProcSocket, o, _config.ListSocket[idx - (TOTAL_HOST + TOTAL_WS)].TimeCheckCycle, _config.ListSocket[idx - (TOTAL_HOST + TOTAL_WS)].TimeCheckCycle);
            }
            catch (Exception e)
            {

                ServiceLogTPVCOFO.Instance.WriteLine("Error Create ThreadProc Socket: " + e.Message + e.StackTrace + " IDX:" + idx + " Total Socket:" + TOTAL_SOCKET);

            }
        }

        /// <summary>
        /// Método que se se encargar de inicial el Hilo que controla los logs generados (Encolados)
        /// </summary>
        /// <param name="o"></param>
        public void ThreadLog(object o)
        {
            int idx = 0;
            try
            {
                idx = Convert.ToInt32(Thread.CurrentThread.Name);
                ServiceLogTPVCOFO.Instance.WriteLine("Thread Proc called Log: " + idx);
                _myTimer[idx] = new Timer(worker[idx].TimerProcLog, o, 10000, 60000);
            }
            catch (Exception e)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error Create ThreadProc Log: " + e.Message + e.StackTrace + " IDX:" + idx + " Total Log: 1");
            }
        }

        public void ThreadLibrary(object o)
        {
            int idx = 0;
            try
            {
                // bloqueamos como una zona crítica para la concurrencia de hilos
                lock (thisLock)
                {
                    idx = Convert.ToInt32(Thread.CurrentThread.Name);
                    ServiceLogTPVCOFO.Instance.WriteLine("Thread Proc called Library: " + idx + " " + _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].BatchName + " " + _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].ClaseName + " " + _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].FuncionName + "(" + " " + _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].Parameters + ") " + "Tank: " + _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].Tanks + " - StartDelay: " + _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].TimeStartDelay + " - CheckCycle: " + _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].TimeCheckCycle);
                    _myTimer[idx] = new Timer(worker[idx].TimerProcLibrary, o, _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].TimeStartDelay, _config.ListLibrary[idx - (TOTAL_HOST + TOTAL_WS + TOTAL_SOCKET)].TimeCheckCycle);
                }
            }
            catch (Exception e)
            {

                ServiceLogTPVCOFO.Instance.WriteLine("Error Create ThreadProc Library: " + e.Message + " IDX:" + idx + " Total Host:" + TOTAL_HOST);

            }
        }

        /// <summary>
        /// Método que se encarga de crear/leer el archivo de configuración XML, sino existe en la ruta de la App se crea con la estructura base definida.
        /// </summary>
        /// <returns>Objeto de tipo ServiceConfigTPVCOFO</returns>
        internal string getConfigurationXML(ServiceConfigTPVCOFO.ServicioWeb arrServicioWeb)
        {
            ServiceConfigTPVCOFO.ServicioWeb[] ArrServicioWeb = new ServiceConfigTPVCOFO.ServicioWeb[1];
            ResponseApi objResponseApi = null;
            string strResultado = "";
            string sPathFile = "";
            string sPathPrincipal = "";
            int intContador = 0;
            int iPos = 0;

            try
            {

                string sPath = Assembly.GetExecutingAssembly().Location;
                sPathPrincipal = sPath;
                iPos = sPathPrincipal.IndexOf("\\");
                ArrServicioWeb[0] = arrServicioWeb;
                sPathPrincipal = sPathPrincipal.Substring(0, iPos) + @"\";
                iPos = sPath.LastIndexOf("\\");
                sPath = sPath.Substring(0, iPos);
                sPathFile = System.IO.Path.Combine(sPathPrincipal, "cetel.tpv");

                if (File.Exists(sPathFile))
                {
                    ConfigurationRequest objConfiguration = new ConfigurationRequest();
                    foreach (string strline in File.ReadLines(sPathFile))
                    {
                        if (intContador == 0)
                            objConfiguration.CodigoTPV = strline;
                        else if (intContador == 2)
                            objConfiguration.CodigoEmpresa = strline;
                        intContador++;
                    }
                    objConfiguration.ID_Tipo_Config = Convert.ToInt32(Constant.ID_TIPO_CONFIG).ToString();

                    string config = new JavaScriptSerializer().Serialize(objConfiguration);
                    File.WriteAllText(sPath + @"\CONFIG.log", config);
                    Thread.Sleep(100);

                    sPathFile = System.IO.Path.Combine(sPath, "CONFIG.log");
                    objResponseApi = Generic.InvokeServiceWrite(sPathFile, ArrServicioWeb[0], false);

                    if (objResponseApi.objResponse != null)
                    {
                        if (objResponseApi.Status == Convert.ToInt32(ApiResponseStatuses.Successful))
                        {
                            JObject rss = JObject.Parse(objResponseApi.objResponse.ToString());
                            string strXML = rss["strXML"].ToString();
                            if (strXML.Length > 500)
                            {
                                File.WriteAllText(sPath + @"\ServicioTPVAgenteLocal.xml", strXML);
                                strResultado = "OK";
                                Thread.Sleep(100);
                            }
                            else
                                strResultado = "NO";
                        }
                        else
                        {
                            strResultado = "XX";
                            ServiceLogTPVCOFO.Instance.WriteLine("Error Obtener XML:" + objResponseApi.Message + ".", true);
                        }
                    }
                    else
                    {
                        strResultado = "XX";
                        ServiceLogTPVCOFO.Instance.WriteLine("Error en el Servicio al obtener XML.", true);
                    }
                }
                else
                    ServiceLogTPVCOFO.Instance.WriteLine(@"No se ha encontrado Archivo CETEL (C:\Cetel.tpv).", true);
            }
            catch (Exception ex)
            {
                strResultado = "XX";
                ServiceLogTPVCOFO.Instance.WriteLine("Error no controlado: al obtener XML: " + ex.Message + ".", true);
            }

            return strResultado;
        }

        private void FC_TankGaugeStatusChanged(TankGauge Tg, TgMainStates MainState, byte Status, int AlarmStatus)
        {
            #region Vars
            string strAlarmTxt;
            string strBatchPath;
            string strTimeStamp;
            string strJsonTgAlarm;
            string strResultado = "";
            ResponseApi objResultado = null;
            string[] arrAlarm = null;
            string strCodeAlarm = "";
            string strTextAlarm = "";
            string strTrace = string.Empty;
            #endregion  //Vars

            try
            {

                //Guardar timestamp
                ServiceLogTPVCOFO.Instance.WriteLine("Inicio Evento FC_TankGaugeStatusChanged: Realizada desde DOMS - Servicio Windows ", true);
                strTrace = "Paso Princiapl";
                if (MainState == TgMainStates.TGMS_ALARM)
                {
                    strTrace = "Paso de Alarmas";
                    strAlarmTxt = GetAlarmText(idpAlarm, Tg.Id);
                    if (!string.IsNullOrEmpty(strAlarmTxt))
                    {
                        strTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        arrAlarm = strAlarmTxt.Split(',');
                        if (arrAlarm != null)
                        {
                            strCodeAlarm = arrAlarm[0].Trim();
                            strTextAlarm = arrAlarm[1].Trim();
                            //Localizar la ruta del archivo batch
                            strBatchPath = Assembly.GetExecutingAssembly().Location;
                            strBatchPath = strBatchPath.Substring(0, strBatchPath.IndexOf("\\"));
                            strBatchPath += @"\CETEL\ServiceTPVCOFO_Files\BatchPendientes\" + strNameFile + ".log";
                            //strJsonTgAlarm = JsonConverter

                            //Comprobar si no existe el archivo batch y crearlo
                            if (!File.Exists(strBatchPath))
                            {
                                File.Create(strBatchPath);
                            }
                            strJsonTgAlarm = "{\"NCOMPANY\":\"" + strCodEmpresa + "\",\"STOREID\":\"" + strStoreID + "\",\"DATE\":\"" + strTimeStamp + "\",\"USERID\":\"" + strUserID + "\",\"TGID\":\"" + Tg.Id + "\",\"CODESTATE\":\"" + Status + "\",\"TYPEALARM\":\"" + AlarmStatus + "\",\"CODEALARM\":\"" + strCodeAlarm + "\",\"TEXTALARM\":\"" + strTextAlarm + "\"}";

                            //strJsonTgAlarm = strJsonTgAlarm.Remove(strJsonTgAlarm.Length - 1, 1);
                            if (strUrlServidor.Trim().Length > 0)
                            {
                                //strUrlServidor = objServicioWeb.EndPoint;

                                ServiceLogTPVCOFO.Instance.WriteLine("Inicio Web Service insertAlarmTankGauge: Realizada desde Servicio Windows.", true);
                                strResultado = insertAlarmTankGauge(strUrlServidor, strPathApi, "{\"listTankGaugeAlarmsHistory\":[" + strJsonTgAlarm + "]}");

                                if (!string.IsNullOrEmpty(strResultado))
                                {
                                    objResultado = JsonConvert.DeserializeObject<ResponseApi>(strResultado);

                                    if (objResultado.Status == 200)
                                    {
                                        ServiceLogTPVCOFO.Instance.WriteLine("Mensaje de Servicio: " + objResultado.Message, true);
                                        // Generic.DeleteTempFiles("BatchPendientes", strNameFile + "*.log", 0);

                                    }
                                    else
                                    {
                                        strJsonTgAlarm = strJsonTgAlarm.Trim() + ",";
                                        File.AppendAllLines(strBatchPath, new string[] { strJsonTgAlarm }, Encoding.ASCII);
                                        ServiceLogTPVCOFO.Instance.WriteLine("Mensaje de Servicio:" + objResultado.Message, true);
                                    }
                                    ServiceLogTPVCOFO.Instance.WriteLine("Fin Web Service insertAlarmTankGauge: Realizada desde Servicio Windows.", true);
                                }
                                else
                                {
                                    strJsonTgAlarm = strJsonTgAlarm.Trim() + ",";
                                    File.AppendAllLines(strBatchPath, new string[] { strJsonTgAlarm }, Encoding.ASCII);
                                    ServiceLogTPVCOFO.Instance.WriteLine("Ha ocurrido un Error al Conectarse al Servicio Web.", true);
                                }
                            }
                            else
                            {
                                strJsonTgAlarm = strJsonTgAlarm.Trim() + ",";
                                File.AppendAllLines(strBatchPath, new string[] { strJsonTgAlarm }, Encoding.ASCII);
                                ServiceLogTPVCOFO.Instance.WriteLine("No se encontro configuración para Servicio Web (Alarmas).", true);
                            }

                        }
                    }
                }
                else
                {
                    ServiceLogTPVCOFO.Instance.WriteLine("No se ncontraron alarmas adheridas a este evento. tgID: "+ Tg.Id+" - estado: "+MainState, true);
                }
                ServiceLogTPVCOFO.Instance.WriteLine("Fin Evento FC_TankGaugeStatusChanged: Realizada desde DOMS - Servicio Windows ", true);
                ServiceLogTPVCOFO.Instance.GrabarLog();
            }
            catch (Exception ex)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error FC_TankGaugeStatusChanged: "+ ex.Message + ex.StackTrace, true);
                ServiceLogTPVCOFO.Instance.GrabarLog();
            }
        }
        
        //TODO Comentar
        private void StartTgAlarmsListener()
        {
            #region Vars
            string strParams;
            string[] arrParams;
            string strHostname;
            string strTpvId;
            string strApplId;
            FcLogonParms objParametro;
            //ServiceConfigTPVCOFO.ServicioWeb[] lstServicioWeb = null;
            ServiceConfigTPVCOFO.Alarm objAlarm = null;
            #endregion  //Vars

            #region Var Initialization
            fcAlarm = new Forecourt();
            objParametro = new FcLogonParms();
            #endregion  //Var Initialization

            try
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Iniciando hilo a la escucha de alarmas", true);

                //Coger los valores de los parametros del la configuración
                objAlarm = _config.ListAlarm[0];

                if (objAlarm != null)
                {
                    strParams = objAlarm.Parameters;
                    strNameFile = objAlarm.BatchName;
                    strUrlServidor = objAlarm.UrlServidor;
                    strPathApi = objAlarm.PathApi;
                    arrParams = strParams.Split('|');
                    strHostname = arrParams[0];
                    strTpvId = arrParams[1];
                    strCodEmpresa = arrParams[2];
                    strStoreID = arrParams[3];
                    strUserID = arrParams[4];
                    strApplId = arrParams[5];
                    fcAlarm.PosId = Convert.ToByte(strTpvId);
                    fcAlarm.HostName = strHostname;

                    //Crear parametros de logueo
                    objParametro.EnableFcEvent(FcEvents.xxxxCfgChanged);
                    objParametro.EnableFcEvent(FcEvents.TankGaugeStatusChanged);

                    //Realizar logueo
                    fcAlarm.Disconnect();
                    fcAlarm.Initialize();
                    idpAlarm = (IDomsPos)fcAlarm;
                    fcAlarm.FcLogon2("POS,UNSO_TGSTA_2,RI,APPL_ID=" + strApplId, objParametro);
                    fcAlarm.TankGaugeStatusChanged += FC_TankGaugeStatusChanged;
                }
                else
                    ServiceLogTPVCOFO.Instance.WriteLine("Configuración de Alarmas no encontrada.", true);

            }
            catch (Exception ex)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error StartTgAlarmsListener:" + ex.Message, true);
            }
        }

        /// <summary>
        /// Devuelve un texto descriptivo acerca de la alarma que ha registrado la sonda. Se llama cada vez que se produce un cambio de estado en alguna sonda del DOMS
        /// </summary>
        /// <param name="_objIdp">Interfaz que se va a utilizar para comunicarse con el DOMS</param>
        /// <param name="_btTgId">Id de la sonda donde se ha producido el cambio de estado</param>
        /// <returns></returns>
        private string GetAlarmText(IDomsPos _objIdp, byte _btTgId)
        {
            #region Vars
            string strAlarmTxt=string.Empty;
            Array arrRequest;
            System.Collections.Generic.List<byte> lstByteRequest;
            Array arrResponse;
            BitArray bitsTgSubStates;
            byte[] bytesAlarmTxt;
            #endregion //Vars

            #region Var Initialization
            strAlarmTxt = null;
            lstByteRequest = new List<byte>();
            #endregion  //Var Initialization

            try
            {
                //Añadir campos al array de bytes request
                lstByteRequest.Add(0x42);//Code
                lstByteRequest.Add(0x2);//Subc
                lstByteRequest.Add(_btTgId);//TgId
                arrRequest = lstByteRequest.ToArray();

                //Mandar el mensaje y guardar la respuesta
                _objIdp.SendDomsPosMessage(arrRequest.Length, ref arrRequest, out arrResponse);

                //Comprobar que la sonda está en estado de alarma
                if (arrResponse != null)
                {
                    bitsTgSubStates = new BitArray(new byte[] { (byte)arrResponse.GetValue(5) });
                    if (bitsTgSubStates.Get(1))
                    {
                        //El cambio de estado que se ha producido en la sonda sí es de alarma
                        //Procesar el texto
                        if ((byte)arrResponse.GetValue(12) > 0)//Comprobar longitud del texto
                        {
                            bytesAlarmTxt = new byte[(byte)arrResponse.GetValue(12)];
                            for (byte i = 0; i < bytesAlarmTxt.Length; i++)
                            {
                                bytesAlarmTxt[i] = (byte)arrResponse.GetValue(13 + i);
                            }

                            strAlarmTxt = Encoding.ASCII.GetString(bytesAlarmTxt);
                        }
                    }
                }

                return strAlarmTxt;
            }
            catch (Exception ex)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error GetAlarmText:" + ex.Message, true);
                throw ex;
            }
        }

        /// <summary>
        /// Create connection with a WebService
        /// </summary/// <summary>
        /// Set Closing Cash  - OFFLINE AND ONLINE
        /// </summary>
        /// <param name="objCashRequest">the object that contais the ncompany, GUID of TPV and tipe of configuration</param>
        /// <returns></returns>
        internal string insertAlarmTankGauge(string strUrlServidor, string strPathMethod, string strAlarmTankGauge)
        {
            #region Variables
            string strResponseWS = string.Empty;
            BE.TankGauge.TankGaugeAlarmHistoryRequest objTankGaugeAlarm = null;
            #endregion

            try
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Inicio Método insertAlarmTankGauge:", true);
                objTankGaugeAlarm = JsonConvert.DeserializeObject<BE.TankGauge.TankGaugeAlarmHistoryRequest>(strAlarmTankGauge);
                if (Generic.ServiceExists(strUrlServidor))
                {
                    ServiceLogTPVCOFO.Instance.WriteLine("Servidor Correcto - insertAlarmTankGauge  ", true);
                    strResponseWS = Generic.ConnectionWSRest(strUrlServidor, strPathMethod, objTankGaugeAlarm);
                }
                else
                {
                    ServiceLogTPVCOFO.Instance.WriteLine("No se ha conectado a servidor: " + strUrlServidor, true);
                }

            }
            catch (Exception ex)
            {
                if(ex.InnerException !=null)
                    ServiceLogTPVCOFO.Instance.WriteLine("Error insertAlarmTankGauge:" + ex.InnerException.Message, true);
                else
                    ServiceLogTPVCOFO.Instance.WriteLine("Error insertAlarmTankGauge:" + ex.Message, true);
            }

            return strResponseWS;
        }

        internal void fnCleanAlarm()
        {
            ServiceLogTPVCOFO.Instance.WriteLine("Limpiar Variables Alarma.", true);
            if (fcAlarm != null)
            {
                fcAlarm.Disconnect();
                fcAlarm = null;
                idpAlarm = null;
            }
            if (threadTgAlarms != null)
            {
                threadTgAlarms.Join(1000);
                threadTgAlarms.Abort();
            }
        }

    }
}
