using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace DOMSLibrary
{
    /// <summary>
    /// Clase para loggar DOMSLibrary
    /// </summary>
    internal static class Logger
    {

        private static string _cachePathFile = null;
        /// <summary>
        /// Path del fichero
        /// </summary>
        private static string _pathFile
        {
            get
            {
                if (string.IsNullOrEmpty(_cachePathFile))
                {
                    _cachePathFile = Assembly.GetExecutingAssembly().Location;
                    int iPos = _cachePathFile.IndexOf(".exe");
                    DateTime dt = DateTime.Now;

                    _cachePathFile = _cachePathFile.Substring(0, iPos);
                    iPos = _cachePathFile.IndexOf("\\");
                    _cachePathFile = _cachePathFile.Substring(0, iPos) + @"\CETEL\ServiceTPVCOFO_Files\Log\";
                    _cachePathFile += "DOMSLibrary" + dt.ToString("yyyyMMdd") + ".log";
                }
                return _cachePathFile;
            }
        }

        /// <summary>
        /// semaforo para acceso a fichero log
        /// </summary>
        private static SemaphoreSlim _logSemaphore = new SemaphoreSlim(0, 1);

        

        /// <summary>
        /// Escribe una traza
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data">objeto JSON que se quiera logar</param>
        public static void Log(string msg, object data = null)
        {
            try
            {
                _logSemaphore.Wait();
                MethodBase callerMethod = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod();
                Type callerType = callerMethod.ReflectedType;
                StreamWriter stream = _openFile();

                try
                {
                    string jsonData = data == null ? string.Empty : new JavaScriptSerializer().Serialize(data);
                    string text = $"{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss.SSS")} {callerType.Namespace}.{callerType.Name}.{callerMethod.Name} => {msg} {Environment.NewLine} {jsonData}";
                    stream.WriteLine(text);
                }
                finally
                {
                    _closeFile(stream);

                }

            }
            catch
            {
                // no hacemos nada
            }
            finally
            {
                _logSemaphore.Release();
            }
        }

        /// <summary>
        /// Logea una excepcion
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public static void LogException(string msg, Exception e = null)
        {
            try
            {
                _logSemaphore.Wait();
                MethodBase callerMethod = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod();
                Type callerType = callerMethod.ReflectedType;
                StreamWriter stream = _openFile();
                try
                {
                    string exMsj = GetStringFromException(e);
                    string text = $"{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss.SSS")} {callerType.Namespace}.{callerType.Name}.{callerMethod.Name} => {msg} {Environment.NewLine} {exMsj}";
                    stream.WriteLine(text);
                }
                finally
                {
                    _closeFile(stream);

                }

            }
            catch
            {
                // no hacemos nada
            }
            finally
            {
                _logSemaphore.Release();
            }
        }

        public static void LogException(Exception e)
        {
            LogException("", e);
        }
        /// <summary>
        /// Obtiene la representacion en string de la Excepcion
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static string GetStringFromException(Exception e)
        {
            if (e == null)
            {
                return string.Empty;
            }
            StringBuilder ret = new StringBuilder();
            ret = ret.AppendLine($"Excepcion Message:{e.Message}").AppendLine($"en: {e.Source}").AppendLine($"StackTrace: {e.StackTrace}");
            if (e.InnerException != null)
            {
                ret = ret.AppendLine(GetStringFromException(e.InnerException));
            }
            return ret.ToString();
        }

        /// <summary>
        /// Debe llamarse dentro de bloque critico.
        /// </summary>
        private static StreamWriter _openFile()
        {
            FileMode fm = File.Exists(_pathFile) ? FileMode.Append : FileMode.CreateNew;
            return new StreamWriter(new FileStream(_pathFile, fm, FileAccess.Write, FileShare.Read));
        }

        /// <summary>
        /// Debe llamarse dentro de bloque critico
        /// </summary>
        /// <param name="stream"></param>
        private static void _closeFile(StreamWriter stream)
        {
            if (stream == null)
            {
                return;
            }
            stream.Close();
        }

    }
}
