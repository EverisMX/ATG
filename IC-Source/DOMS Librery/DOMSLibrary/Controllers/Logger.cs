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
        /// <summary>
        /// Guarda la fecha actual en la que se crea el log
        /// </summary>
        private static DateTime? _dateOfLogFile = null;
        /// <summary>
        /// Mantiene en cache la ruta al fichero log del dia
        /// </summary>
        private static string _cachePathFile = null;
        /// <summary>
        /// Path del fichero
        /// </summary>
        private static string _pathFile
        {
            get
            {
                // si hoy no es la misma fecha del log, borramos cache de ruta al fichero
                if (!_dateOfLogFile.HasValue || DateTime.Today.Date != _dateOfLogFile.Value.Date)
                {
                    _cachePathFile = string.Empty;
                    _dateOfLogFile = DateTime.Today;
                }
                // si no esta cacheado el path, generamos ruta con la fecha del log
                if (string.IsNullOrEmpty(_cachePathFile))
                {
                    _cachePathFile = Assembly.GetExecutingAssembly().Location;
                    int iPos = _cachePathFile.IndexOf(".dll");

                    _cachePathFile = _cachePathFile.Substring(0, iPos);
                    iPos = _cachePathFile.IndexOf("\\");
                    _cachePathFile = _cachePathFile.Substring(0, iPos) + @"\CETEL\ServiceTPVCOFO_Files\Log\";
                    _cachePathFile += "DOMSLibrary" + _dateOfLogFile.Value.ToString("yyyyMMdd") + ".log";
                }
                return _cachePathFile;
            }
        }

        /// <summary>
        /// semaforo para acceso a fichero log
        /// </summary>
        private static SemaphoreSlim _logSemaphore = new SemaphoreSlim(1, 1);



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
                    string text = $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} {callerType.Namespace}.{callerType.Name}.{callerMethod.Name} => {msg} {Environment.NewLine} {jsonData}";
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
        /// <param name="e"></param>
        /// <param name="msg"></param>
        public static void LogException(Exception e, string msg = "")
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
                    string text = $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} {callerType.Namespace}.{callerType.Name}.{callerMethod.Name} => {msg} {Environment.NewLine} {exMsj}";
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
            ret = ret.AppendLine($"Excepcion: {e.ToString()}");
            if (e.InnerException != null)
            {
                ret = ret.AppendLine($"Inner Exception: {GetStringFromException(e.InnerException)}" );
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
