using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal class DOMSConnection
    {
        /// <summary>
        /// Método que se desencadenara el Logueo hacia los controladores del DOMS, para poder obtener información. 
        /// </summary>
        /// <param name="o"></param>
        internal static bool _performanceConectionDOMS(string strHost, string bytPosID, string idmaquina)
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
    }
}
