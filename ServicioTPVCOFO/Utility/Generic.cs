using Newtonsoft.Json;
using PSS_Forecourt_Lib;
using ServicioTPVAgenteLocal.BE;
using ServicioTPVAgenteLocal.BE.TankGauge;
using ServicioTPVAgenteLocal.Configuration;
using ServicioTPVAgenteLocal.Utility;
using ServicioTPVAgenteLocal.Utility.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

namespace ServicioTPVAgenteLocal.Utility
{
    public class Generic
    {

        #region Methods 

        /// <summary>
        /// Method to generate a Clase to Json
        /// </summary>
        /// <param name="oHash"></param>
        /// <param name="sTimeStamp"></param>
        /// <returns></returns>
        internal static object fnGenerateClasstoJsonName(string strJson, string JsonName)
        {
            object objGenerico;// = null;

            switch (JsonName)
            {
                case "TankGaugeHistoryList":
                    objGenerico = JsonConvert.DeserializeObject<TankGaugeDataHistoryRequest>(strJson);
                    break;
                case "tankGaugeDataDeliveryList":
                    objGenerico = JsonConvert.DeserializeObject<TankGaugeDataDeliveryHistoryRequest>(strJson);
                    break;
                case "tankGaugeFuellingPoingList":
                    objGenerico = JsonConvert.DeserializeObject<TankGaugeFuellingPointHistoryRequest>(strJson);
                    break;
                case "ConfigurationRequest":
                    objGenerico = JsonConvert.DeserializeObject<ConfigurationRequest>(strJson);
                    break;
                default:
                    objGenerico = null;
                    break;

            }

            return objGenerico;

        }

        internal static void DirSearch(List<string> files, string startDirectory, string strBatchName)
        {
            try
            {
                foreach (string file in Directory.GetFiles(startDirectory, strBatchName + "*.log"))
                {
                    string extension = Path.GetExtension(file);

                    if (extension != null)
                    {
                        files.Add(file);
                    }
                }

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Método que se encarga de invocar al servicio utilizando la configuración sXML del archivo de configuración del Hilo.
        /// </summary> 
        /// <returns>El resultado de la invocación al servicio </returns>
        internal static ResponseApi InvokeServiceWrite(string pstrBatchPathFile, ServiceConfigTPVCOFO.ServicioWeb pSW, bool pbolObjectArray = true)
        {
            #region Vars
            // ILION- Se comentara el signature y el timestamp para la invocacion.
            string strData;
            //string strTimeStamp; 
            //string strSignature;
            string strJsonName;
            ResponseApi objReponseApi;
            ServiceConfigTPVCOFO.ServicioWeb SW;
            string strMsg;
            #endregion //Vars

            #region Var Initialization
            //strTimeStamp = DateTime.UtcNow.ToString("yyyyMMdd HH:mm:ss");
            objReponseApi = new ResponseApi();
            strMsg = string.Empty;
            #endregion //Var Initialization

            try
            {
                SW = pSW;
                if (File.Exists(pstrBatchPathFile))
                {
                    strJsonName = SW.JsonName;
                    using (StreamReader sr = new StreamReader(pstrBatchPathFile))
                    {
                        strData = sr.ReadToEnd();
                        if (strData.Length > 0)
                        {
                            strData = strData.Remove(strData.Length - 1, 1);
                            if (pbolObjectArray)
                                strData = "{\"" + strJsonName + "\":[" + strData + "]" + "}";
                            else
                                strData = strData + "}";

                        }
                    }

                    if (strData.Length > 20)
                    {
                        //object objGenerico = Generic.fnGenerateClasstoJsonName(strData, strJsonName);
                        //strSignature = GenericHelper.GeneratedSignature(objGenerico, strTimeStamp);

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SW.EndPoint);
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.Accept = "application/json";
                        request.ContentLength = strData.Length;
                        request.KeepAlive = true;
                        request.SendChunked = true;


                        //request.Headers.Add("Signature", strSignature);
                        //request.Headers.Add("TimeStamp", strTimeStamp);

                        try
                        {

                            using (Stream webStream = request.GetRequestStream())
                            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                            {
                                requestWriter.Write(strData);
                            }

                            using (HttpWebResponse webResponse = request.GetResponse() as HttpWebResponse)
                            {
                                using (Stream webStream = webResponse.GetResponseStream())
                                {
                                    using (StreamReader responseReader = new StreamReader(webStream))
                                    {
                                        string response = responseReader.ReadToEnd();
                                        //Console.Out.WriteLine(response);
                                        if (SW.BitSaveResponse)
                                        {
                                            objReponseApi = JsonConvert.DeserializeObject<ResponseApi>(response);
                                        }
                                        else { objReponseApi = JsonConvert.DeserializeObject<ResponseApi>(response); }

                                    }
                                }
                            }

                        }
                        catch (WebException ex)
                        {
                            using (var stream = ex.Response.GetResponseStream())
                            using (var reader = new StreamReader(stream))
                            {
                                strMsg += "\r\n" + " ILION- Error Peticion: " + SW.EndPoint + "\n Detalle Consulta: " + reader.ReadToEnd();
                                ServiceLogTPVCOFO.Instance.WriteLine(strMsg + "\r\n" + "---------------------------------------------------");
                                objReponseApi.Message = strMsg;
                                objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                                return objReponseApi;
                            }
                        }
                        catch (Exception e)
                        {
                            strMsg += "\r\n" + "Check " + SW.EndPoint + ", Error WS: " + e.Message + e.StackTrace + "COnsulta Error" + SW.EndPoint + "Detalle Consulta: " + SW.XML;
                            ServiceLogTPVCOFO.Instance.WriteLine(strMsg + "\r\n" + "---------------------------------------------------");
                            objReponseApi.Message = strMsg;
                            objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                            return objReponseApi;
                        }
                    }
                    else
                    {
                        objReponseApi.Message = "Archivo con formato json vacío o inválido";
                        objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                    }
                }
                else
                {
                    objReponseApi.Message = "Archivo no Encontrado.";
                    objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                }

            }
            catch (Exception ex)
            {
                objReponseApi.Message = ex.Message;
                objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
            }

            return objReponseApi;
        }



        /// <summary>
        /// ILION Método que se encarga de invocar al servicio.
        /// </summary> 
        /// <returns>El resultado de la invocación al servicio </returns>
        internal static ResponseApi InvokeServiceWriteParams(string pstrBatchPathFile, ServiceConfigTPVCOFO.ServicioWeb pSW, bool pbolObjectArray = true)
        {
            #region Vars
            // ILION- Se comentara el signature y el timestamp para la invocacion.
            string strData;
            string strJsonName;
            ResponseApi objReponseApi;
            ServiceConfigTPVCOFO.ServicioWeb SW;
            string strMsg;
            #endregion //Vars

            #region Var Initialization
            objReponseApi = new ResponseApi();
            strMsg = string.Empty;
            #endregion //Var Initialization

            try
            {
                SW = pSW;
                if (File.Exists(pstrBatchPathFile))
                {
                    strJsonName = SW.JsonName;
                    using (StreamReader sr = new StreamReader(pstrBatchPathFile))
                    {
                        strData = sr.ReadToEnd();
                        if (strData.Length > 0)
                        {
                            strData = strData.Remove(strData.Length - 1, 1);
                            if (pbolObjectArray)
                                strData = "{\"" + strJsonName + "\":[" + strData + "]" + "}";
                            else
                                strData = strData + "}";

                        }
                    }

                    if (strData.Length > 20)
                    {
                        Dictionary<string, string> paramaters = new Dictionary<string, string>()
                        {
                            { "request", strData }
                        };

                        string postData = "";

                        foreach (string key in paramaters.Keys)
                        {
                            postData += "?" +  HttpUtility.UrlEncode(key) + "="
                                  + HttpUtility.UrlEncode(paramaters[key]);
                        }

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SW.EndPoint + postData);
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = strData.Length;

                        try
                        {

                            using (Stream webStream = request.GetRequestStream())
                            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                            {
                                requestWriter.Write(strData);
                            }

                            using (HttpWebResponse webResponse = request.GetResponse() as HttpWebResponse)
                            {
                                using (Stream webStream = webResponse.GetResponseStream())
                                {
                                    using (StreamReader responseReader = new StreamReader(webStream))
                                    {
                                        string response = responseReader.ReadToEnd();
                                        if (SW.BitSaveResponse)
                                        {
                                            objReponseApi = JsonConvert.DeserializeObject<ResponseApi>(response);
                                        }
                                        else { objReponseApi = JsonConvert.DeserializeObject<ResponseApi>(response); }

                                    }
                                }
                            }

                        }
                        catch (WebException ex)
                        {
                            using (var stream = ex.Response.GetResponseStream())
                            using (var reader = new StreamReader(stream))
                            {
                                strMsg += "\r\n" + " ILION- Error Peticion: " + SW.EndPoint + "\n Detalle Consulta: " + reader.ReadToEnd();
                                ServiceLogTPVCOFO.Instance.WriteLine(strMsg + "\r\n" + "---------------------------------------------------");
                                objReponseApi.Message = strMsg;
                                objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                                return objReponseApi;
                            }
                        }
                        catch (Exception e)
                        {
                            strMsg += "\r\n" + "Check " + SW.EndPoint + ", Error WS: " + e.Message + e.StackTrace + "COnsulta Error" + SW.EndPoint + "Detalle Consulta: " + SW.XML;
                            ServiceLogTPVCOFO.Instance.WriteLine(strMsg + "\r\n" + "---------------------------------------------------");
                            objReponseApi.Message = strMsg;
                            objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                            return objReponseApi;
                        }
                    }
                    else
                    {
                        objReponseApi.Message = "Archivo con formato json vacío o inválido";
                        objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                    }
                }
                else
                {
                    objReponseApi.Message = "Archivo no Encontrado.";
                    objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
                }

            }
            catch (Exception ex)
            {
                objReponseApi.Message = ex.Message;
                objReponseApi.Status = (int)ApiResponseStatuses.BadRequest;
            }

            return objReponseApi;
        }




        /// <summary>
        /// Método que se desencadenara el Logueo hacia los controladores del DOMS, para poder obtener información. 
        /// </summary>
        /// <param name="o"></param>
        internal static bool fnLogonPSSPOS(string strHost, string bytPosID, out Forecourt fc, out IFCConfig ifc, string idmaquina)
        {

            fc = new Forecourt();
            ifc = null;

            string logon = "POS,UNSO_FPSTA_2,APPL_ID=" + idmaquina;

            fc.PosId = Convert.ToByte(bytPosID);
            fc.HostName = strHost;
            fc.Disconnect();
            string strEstadoSonda = "Initialize";
            fc.Initialize();
            strEstadoSonda = "Logon";

            ifc = (IFCConfig)fc;
            int cnt = 0;
            bool auxLogon = false;
            FcLogonParms flpParametro = null;

            do
            {
                cnt++;
                try
                {
                    flpParametro = new FcLogonParms();
                    flpParametro.EnableFcEvent(FcEvents.xxxxCfgChanged);
                    fc.FcLogon2(logon, flpParametro);
                    auxLogon = true;

                }
                catch
                {
                    if (cnt == 3)
                        throw new Exception("Intento de Conexión 3 veces fallida" + strEstadoSonda);
                    Thread.Sleep(1000); // Pausa de 1 segundo para reintentar el Logon
                }
            } while ((cnt < 3) && !auxLogon);

            return auxLogon;
        }


        /// <summary>
        /// Try to connect to the indicated URL
        /// </summary>
        /// <param name="sUrl">URL to connect </param>
        /// <returns>True if the connection could be performed or false if it could not be</returns>
        internal static Boolean ServiceExists(String sUrl)
        {
            try
            {
                // try accessing the web service directly via it's URL
                HttpWebRequest request = WebRequest.Create(sUrl) as HttpWebRequest;

                request.Timeout = 30000;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK) return false;
                }


                return true;
            }
            catch (WebException ex)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error insertAlarmTankGauge:" + ex.InnerException.Message);
                return false;
            }
            catch (Exception ex)
            {
                ServiceLogTPVCOFO.Instance.WriteLine("Error insertAlarmTankGauge:" + ex.InnerException.Message);
                return false;
            }
        }

        /// <param name="sURL">The URL of WebService</param>
        /// <param name="sMethod">Method "POST" or "GET"</param>
        /// <param name="sContentType">Type of body content</param>
        /// <param name="oHeaders">Collection Headers</param>
        /// <param name="sBody">Value body</param>
        /// <returns>Return the WebRequest with Method and Contype specific</returns>
        internal static string ConnectionWSRest(string sURL, string strMetodo, Object obj)
        {
            #region Variables  
            WebRequest requestWS;
            String sResponseWSREST;
            WebHeaderCollection headers;
            Stream stream;
            StreamReader reader;
            #endregion //Variables

            try
            {
                String sJson = JsonConvert.SerializeObject(obj);

                requestWS = (HttpWebRequest)WebRequest.Create(sURL + strMetodo);
                requestWS.Method = "POST";
                requestWS.ContentType = "application/json";

                String sTimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                //HEADERS
                headers = new WebHeaderCollection();
                headers.Add("TimeStamp", sTimeStamp);
                headers.Add("Signature", GenericHelper.GeneratedSignature(obj, sTimeStamp));
                requestWS.Headers = headers;

                //BODY
                Byte[] bt = Encoding.UTF8.GetBytes(sJson);
                requestWS.ContentLength = bt.Length;

                using (Stream st = requestWS.GetRequestStream())
                    st.Write(bt, 0, bt.Length);


                // Obtemos el Response de la petición
                WebResponse responseHttp = (HttpWebResponse)requestWS.GetResponse();
                stream = responseHttp.GetResponseStream();
                reader = new StreamReader(stream);

                sResponseWSREST = reader.ReadToEnd();
            }
            catch
            {
                sResponseWSREST = null;
                throw;
            }
            return sResponseWSREST;
        }

        /// <summary>
        /// Método para Eliminar Archivos para un periodo de tiempo dado.(15 dias hacia atras por defecto)
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strFilter"></param>
        /// <param name="intNroDias"></param>
        internal static void DeleteTempFiles(string strSource, string strFilter, int intNroDias)
        {
            string path = Assembly.GetExecutingAssembly().Location;
            int pos = path.IndexOf("\\");

            int posXML = path.IndexOf(".exe");
            string filename = string.Empty;

            string ModuleName = path.Substring(pos + 1);
            path = path.Substring(0, path.IndexOf(ModuleName));
            string sourcePath = path + @"CETEL\ServiceTPVCOFO_Files\" + strSource + @"\";
            DateTime DateNow = DateTime.Now.AddDays(intNroDias);

            DirectoryInfo dirInfo = new DirectoryInfo(@sourcePath);

            FileInfo[] Files = dirInfo.GetFiles(strFilter)
                    .Where(p => p.CreationTime.Date <= DateNow).ToArray();

            foreach (FileInfo fi in Files)
            {
                fi.Delete();
            }

            Thread.Sleep(100);
        }
        #endregion

    }
}
