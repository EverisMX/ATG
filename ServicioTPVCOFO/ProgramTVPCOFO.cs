using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServicioTPVAgenteLocal
{
    static class ProgramTVPCOFO
    {
        
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServiceTPVATG()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
