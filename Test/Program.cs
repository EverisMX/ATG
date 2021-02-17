using ServicioTPVAgenteLocal;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {


            ServiceThreadTPVCOFO obj = new ServiceThreadTPVCOFO();
            obj.Start("I");
            while (true)
            {
            };
            //This wont stop app
#if DEBUG_SERVICE_START

                     System.Diagnostics.Debugger.Launch();

#endif

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
        //static void Main(string[] args)
        //{

        //    ConParametro objParametro = new ConParametro();
        //    string json = objParametro.GetInfoDOMS("80.28.108.149", 69, "02957", "02957TRU", "02957USR");
        //    Console.WriteLine(json);
        //    //#if DEBUG_SERVICE_START

        //    //         System.Diagnostics.Debugger.Launch();

        //    //#endif

        //    //if (Environment.UserInteractive)
        //    //{
        //    //    Execute();


        //    //    SinParametro objSinParametros = new SinParametro();
        //    //    string json = objSinParametros.FuncionTestSP();

        //    //    Console.WriteLine(json);
        //    //    DeleteTempFiles();
        //    //}  


        //    //Console.ReadLine();
        //}

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
