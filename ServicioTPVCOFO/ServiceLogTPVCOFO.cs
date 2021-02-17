using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace ServicioTPVAgenteLocal
{
    public class ServiceLogTPVCOFO
    {
        public static ConcurrentQueue<string> itemsToWrite = new ConcurrentQueue<string>();
        private static ServiceLogTPVCOFO _instance;
        private static FileStream _fileStream;
        private static StreamWriter _streamWriter;

        public static string ModuleName { get; set; }
        public ServiceLogTPVCOFO()
        {
        }

        /// <summary>
        /// metodo singleton para instanciar la clase
        /// </summary>
        public static ServiceLogTPVCOFO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLogTPVCOFO();
                }
                return _instance;
            }
        }

        #region Log Information
        /// <summary>
        /// Método que se encarga de abrir el archivo log, si valida que existe el archivo se inicializa en modo Append sino realiza la creación del archivo.
        /// </summary>
        /// <param name="iNroIntentos"> Número de intentos consecutivos </param>
        public static void OpenLog(int iNroIntentos = 1)
        {
            try
            {
                //int iWeek;
                string sPath = Assembly.GetExecutingAssembly().Location;
                int iPos = sPath.IndexOf(".exe");

                //DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                DateTime dt = DateTime.Now;

                sPath = sPath.Substring(0, iPos);
                iPos = sPath.IndexOf("\\");
                sPath = sPath.Substring(0, iPos) + @"\CETEL\ServiceTPVCOFO_Files\Log\";
                //iWeek = dfi.Calendar.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                sPath += "LOG" + dt.ToString("yyyyMMdd") + ".log";
                FileMode fm = File.Exists(sPath) ? FileMode.Append : FileMode.CreateNew;
                _fileStream = new FileStream(sPath, fm, FileAccess.Write, FileShare.Read);
                _streamWriter = new StreamWriter(_fileStream);

            }
            catch
            {
                if (iNroIntentos <= 3)
                    OpenLog(iNroIntentos + 1);

            }
        }

        /// <summary>
        /// Método que se encarga de adicionar a la cola: variable itemsToWrite los mensajes de LOG, el Hilo Log utiliza esta variable para su registro en el archivo Físico. El uso del método previene problemas de multiacceso a un único archivo
        /// </summary>
        /// <param name="logMsg">Mensaje LOG que se encolará para registro en archivo</param>
        /// <param name="timeStamp">Indica si se debe concatenar la fecha </param>
        public void WriteLine(string logMsg, bool timeStamp = false)
        {

            string sTime = string.Empty;
            if (timeStamp)
                sTime = DateTime.Now.ToString("G") + ", ";
            itemsToWrite.Enqueue(sTime + logMsg);

        }

        /// <summary>
        /// Cierra el archivo LOG
        /// </summary>
        public static void Close()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Close();
                _streamWriter = null;
            }

            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream = null;
            }
        }

        /// <summary>
        /// Método que se invoca por Hilo Log para volcar los mensajes encolados en itemsToWrite al archivo .txt
        /// </summary>
        public void GrabarLog()
        {
            try
            {
                if (_streamWriter == null) OpenLog();
                _streamWriter.BaseStream.Seek(0, SeekOrigin.End);

                _streamWriter.AutoFlush = true;
                string itemToWrite;
                while (itemsToWrite.TryDequeue(out itemToWrite))
                {
                    _streamWriter.WriteLine(itemToWrite);
                }
                Thread.Sleep(10);  //sleep for 10 milliseconds

                Close();
            }
            catch (Exception e)
            {
                itemsToWrite.Enqueue(e.InnerException==null?e.ToString(): e.InnerException.ToString());
            }
        }
        #endregion
    }
}
