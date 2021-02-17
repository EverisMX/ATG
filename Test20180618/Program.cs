using PSS_Forecourt_Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test20180618
{
    class Program
    {
        static void Main(string[] args)
        {
            //Conexión de prueba
            Forecourt fc = new Forecourt();
            fc.PosId = 60;
            fc.HostName = "83.56.16.44";
            FcLogonParms fcLogonParams = new FcLogonParms();
            fc.Disconnect();
            fc.Initialize();
            IDomsPos idp = (IDomsPos)fc;
            fcLogonParams.EnableFcEvent(FcEvents.xxxxCfgChanged);
            fcLogonParams.EnableFcEvent(FcEvents.TankGaugeStatusChanged);
            fc.FcLogon2("POS,UNSO_TGSTA_2,RI,APPL_ID=SvcTPVCOFO", fcLogonParams);

            //Llamar a método estático desde libreria DOMSLibrary
            string path = Assembly.GetExecutingAssembly().Location;
            path = path.Substring(0, (path.Length - 16));
            string lib = "DOMSLibrary";
            Assembly ass = Assembly.LoadFile(path + "Lib\\" + lib + ".dll");//Ruta de DOMSLibrary.dll
            Type tipo = ass.GetType(lib + ".DOMSTankGaugeAlarms");
            MethodInfo metodo = tipo.GetMethod("GetTankGaugeAlarmTxt");
            //Ejecutar el metodo y guardar el retorno
            string resultado = metodo.Invoke(null, new object[] { idp, (byte)3 }).ToString();
            Console.WriteLine("TEST20180618: " + resultado);

            Console.WriteLine("\nA la espera.\nPulse Esc para salir...\n");
            while (Console.ReadKey().Key != ConsoleKey.Escape) ;
        }
    }
}
