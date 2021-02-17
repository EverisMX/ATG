using System;
using DOMSLibrary;
using System.Threading;
using PSS_Forecourt_Lib;

namespace Test_DOMS
{
    class Program
    {

        public static Boolean fnIniciarLectorSonda(string bytPosID, string strHost, out Forecourt fc, out IFCConfig ifc, string idmaquina)
        {

            fc = new Forecourt();
            ifc = null;

            //fc.TankGaugeCfgChanged += new _IForecourtEvents_TankGaugeCfgChangedEventHandler(FC_TankGaugeCfgChanged);
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
            FcLogonParms objParametro = new FcLogonParms();
            do
            {
                cnt++;
                try
                { objParametro.EnableFcEvent(FcEvents.xxxxCfgChanged);
                    fc.FcLogon2(logon, objParametro);
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

        static void Main(string[] args)
        {

            DOMSTankGauge objTanke = new DOMSTankGauge();
            DOMSTankGauge objTanke1 = new DOMSTankGauge();
                DOMSTankGauge objTanke2 = new DOMSTankGauge();
            DOMSAlarmsProbes ALARMS = new DOMSAlarmsProbes();

            string json="json: ";
            string json2= "json2: ";
            string json3= "json3: ";
            Forecourt fc;
            IFCConfig ifc;
            //string json = objTanke.GetInfoDOMS("80.28.108.149", "69", "02957", "02957TRU", "02957USR");
            //Thread thread = new Thread(() =>
            //{
            int i = 0;
                try
                {
                    bool blnResultado = fnIniciarLectorSonda("69", "83.56.16.44", out fc, out ifc, "VS00007_69");
                    if (blnResultado == true)
                    {
                        while (i < 20)
                        {
                            // json = ALARMS.fnGetAlarm("83.56.16.44", "11", "03535", "03535TRU", "03535USR", "VIRROG01_11", fc, ifc);
                            json = json + objTanke.fnObtenerTankGaugeData("83.56.16.44", "69", "03535", "03535TRU", "03535USR", "VS00007_69", "8-9|9-5", fc, ifc);
                            Thread.Sleep(60000);
                            i++;
                        }
                    }
                    Console.Write(i.ToString());
                }
                catch (Exception ex)
                {
                    //source.SetException(ex);
                    throw new Exception(ex.Message);
                }
            //});
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();

            //Thread thread2 = new Thread(() =>
            //{
            //    try
            //    {
            //        json2 = json2 + objTanke1.fnObtenerTankGaugeDelivery("80.28.108.149", "69", "02957", "02957TRU", "02957USR","VS00008_69");
            //        //Console.Write(json2.ToString());
            //    }
            //    catch (Exception ex)
            //    {
            //        //source.SetException(ex);
            //        throw new Exception(ex.Message);
            //    }
            //});
            //thread2.SetApartmentState(ApartmentState.STA);
            //thread2.Start();

            //Thread thread3 = new Thread(() =>
            //{
            //    try
            //    {
            //        json3 = json3 + objTanke2.fnObtenerPuntoCombustible("80.28.108.149", "70", "02957", "02957TRU", "02957USR", "VS00008_69");
            //        //Console.Write(json3.ToString());
            //    }
            //    catch (Exception ex)
            //    {
            //        //source.SetException(ex);
            //        throw new Exception(ex.Message);
            //    }
            //});
            //thread3.SetApartmentState(ApartmentState.STA);
            //thread3.Start();




            //string json3 = objTanke.fnObtenerTankGaugeDelivery("80.28.108.149", "69", "02957", "02957TRU", "02957USR");

            //Console.WriteLine(json2);
            //#if DEBUG_SERVICE_START

            //         System.Diagnostics.Debugger.Launch();

            //#endif

            //if (Environment.UserInteractive)
            //{
            //    Execute();


            //    SinParametro objSinParametros = new SinParametro();
            //    string json = objSinParametros.FuncionTestSP();

            //    Console.WriteLine(json);
            //    DeleteTempFiles();
            //}  


            //Console.ReadLine();
        }

        //static void Execute()
        //{
        //    ServicioTPVCOFO.ServiceConfigTPVCOFO.Library lb = new ServicioTPVCOFO.ServiceConfigTPVCOFO.Library
        //    {
        //        LibreriaName = "LibreriaTest",
        //        ClaseName = "SinParametro",
        //        FuncionName = "FuncionTestSP",
        //        Parameters = "",
        //        TimeCheckCycle = 120000,
        //        TimeStartDelay = 2000,
        //        BitSaveResponse = true,
        //        BitBatchResponse = true,
        //        BatchName = "LibreriaTest_SinParametro_FuncionTestSP01",
        //        TimeMinuteBatchCycle = 5
        //    };
        //    ServicioTPVCOFO.ServiceWorkerTPVCOFO sw = new ServicioTPVCOFO.ServiceWorkerTPVCOFO();
        //    string str = sw.LibraryResponse(lb.LibreriaName, lb.ClaseName, lb.FuncionName, lb.Parameters);


        //    ServicioTPVCOFO.ServiceThreadTPVCOFO obj = new ServicioTPVCOFO.ServiceThreadTPVCOFO();
        //    obj.Start("I");
        //    //string a = "";
        //}

        //static void DeleteTempFiles()
        //{
        //    string path = @"C:\Program Files (x86)\RepsolCOFO\System Monitor Service COFO\"; //Assembly.GetExecutingAssembly().Location;
        //    int pos = path.LastIndexOf("\\");
        //    string ModuleName = path.Substring(pos + 1);
        //    path = path.Substring(0, path.IndexOf(ModuleName));
        //    string sourcePath = path;

        //    DirectoryInfo dirInfo = new DirectoryInfo(@"C:\Program Files (x86)\RepsolCOFO\System Monitor Service COFO\");
        //    foreach (FileInfo fi in dirInfo.GetFiles("*.log"))
        //    {
        //        string assembly = "ServicioTPVCOFO";
        //        string filename = fi.Name.Substring(0, assembly.Length);
        //        if (filename.Equals(assembly))
        //        {
        //            Console.WriteLine(fi.Name);
        //        }

        //        //fi.Delete();
        //    }
        //    Thread.Sleep(100);
        //}
    }
}
