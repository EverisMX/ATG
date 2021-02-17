using ServicioTPVAgenteLocal.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ServicioTPVAgenteLocal
{
    public class ServiceBatchTPVCOFO
    {
         
        private static FileStream  _fileStreamBatch;
        private static StreamWriter  _streamWriterBatch;
    
        public static string sModuleName { get; set; }
        public ServiceBatchTPVCOFO()
        { }


        #region Batch Information

        private static void OpenFile(string pstrBatchName)
        {
            string sPath = Assembly.GetExecutingAssembly().Location;
            int iPos = sPath.IndexOf("\\");
            sPath = sPath.Substring(0, iPos) + @"\CETEL\ServiceTPVCOFO_Files\";
            sPath += pstrBatchName + ".log";
            FileMode fm = File.Exists(sPath) ? FileMode.Append : FileMode.CreateNew;
            _fileStreamBatch = new FileStream(sPath, fm, FileAccess.Write, FileShare.Read);
            _streamWriterBatch = new StreamWriter(_fileStreamBatch);
        }

        private static void OpenFileVolumen(string pstrBatchName, int pintSizeFile )
        {
            int intSizeFile = pintSizeFile;
            FileMode fm;
            long lngSize = 0;
            string sPath = "";
            string strPathFile = "";
            int iPos = 0;
            int intCorrelative = 0;
            string strFileMax = "";

            sPath = Assembly.GetExecutingAssembly().Location;
            iPos = sPath.IndexOf("\\");
            sPath = sPath.Substring(0, iPos) + @"\CETEL\ServiceTPVCOFO_Files\";
            strPathFile = sPath + pstrBatchName + ".log";
            strFileMax = fnGetFileMax(sPath, pstrBatchName);

            if (strFileMax != null)
            {
                strPathFile = strFileMax;
                lngSize = new FileInfo(strPathFile).Length + intSizeFile;

                if (lngSize <= 3072000 )
                {
                    fm = FileMode.Append;
                }
                else
                {
                    fm = FileMode.CreateNew;
                    intCorrelative = fnGetCorrelative(sPath, pstrBatchName);
                    strPathFile = strPathFile.Replace(".log", intCorrelative.ToString() + ".log");
                }
            }
            else
            {
                fm = FileMode.CreateNew;
            }

            _fileStreamBatch = new FileStream(strPathFile, fm, FileAccess.Write, FileShare.Read);
            _streamWriterBatch = new StreamWriter(_fileStreamBatch);
        }

        private static string fnGetFileMax(string pstrPath, string pstrBatchName)
        {

            string strFileMax = "";
            List<string> lstFile = new List<string>();

            Generic.DirSearch(lstFile, pstrPath, pstrBatchName);
            strFileMax = lstFile.Max(x => x);

            return strFileMax;
        }

        private static int fnGetCorrelative(string pstrPath, string pstrBatchName)
        {

            int intCorrelativo = 0;
            string strFileMax = "";

            strFileMax = fnGetFileMax(pstrPath, pstrBatchName);

            if (strFileMax.IndexOf(pstrBatchName) > 0)
            {
                strFileMax = strFileMax.Substring(strFileMax.IndexOf(pstrBatchName) + pstrBatchName.Length);
                intCorrelativo = Convert.ToInt32(strFileMax.Replace(".log", "") == "" ? "0" : strFileMax.Replace(".log", "")) + 1;
            }
            else { intCorrelativo = 0; }

            return intCorrelativo;

        }

        /// <summary>
        /// Método que se encarga de abrir el archivo BATH asociado al hilo si es que se encuentra configurado para almacenar respuesta, si valida que existe el archivo se inicializa en modo Append sino realiza la creación del archivo.
        /// </summary>
        /// <param name="iNroIntentos"> Número de intentos consecutivos</param>
        /// <param name="sName">Nombre del Archivo BATCH</param>
        public static void OpenBatchLog(string sName, int iNroIntentos = 1, bool pblnVolumen=false, int pintSizeFile=0)
        {
            try
            {
                if (pblnVolumen == false)
                    OpenFile(sName);
                else
                    OpenFileVolumen(sName, pintSizeFile);
            }
            catch (Exception e)
            {   
                if (iNroIntentos <= 3)
                    OpenBatchLog(sName, iNroIntentos + 1,pblnVolumen, pintSizeFile);
                else
                    ServiceLogTPVCOFO.Instance.WriteLine(e.InnerException==null? e.ToString():e.InnerException.ToString());
            }
        }

        /// <summary>
        /// Método utilizado para escribir la respuesta en archivo TXT asociado a un Hilo: Host, WS ó Socket. No se utiliza encolado ya que el uso del archivo es por Hilo.
        /// </summary>
        /// <param name="sArchivo">Nombre del Archivo BATCH</param>
        /// <param name="sMessage">Texto que se escribirá en el archivo  </param>
        public void WriteLineBatch(string sArchivo, string sMessage, int intSatoLinea=1, bool pblnVolumen = false, int pintSizeFile = 0)
        {
            try
            {
                if (sMessage != null)
                {
                    if (sMessage.Trim().Length > 0)
                    {
                        if (_streamWriterBatch == null) OpenBatchLog(sArchivo, 1, pblnVolumen, pintSizeFile);
                        _streamWriterBatch.BaseStream.Seek(0, SeekOrigin.End);

                        _streamWriterBatch.AutoFlush = true;
                        if (intSatoLinea == 1)
                            _streamWriterBatch.WriteLine(sMessage);
                        else
                            _streamWriterBatch.Write(sMessage);

                        Thread.Sleep(10);
                        CloseBatch();
                    }
                }
            }
            catch (Exception e)
            {
                CloseBatch();
                ServiceLogTPVCOFO.Instance.WriteLine(e.InnerException == null ? e.ToString() : e.InnerException.ToString());
            }
        }

        /// <summary>
        /// Cierra el archivo LOG
        /// </summary>
        public void CloseBatch()
        {
            if (_streamWriterBatch != null)
            {
                _streamWriterBatch.Close();
                _streamWriterBatch = null;
            }

            if (_fileStreamBatch != null)
            {
                _fileStreamBatch.Close();
                _fileStreamBatch = null;
            }
        } 
        #endregion
    }
}
