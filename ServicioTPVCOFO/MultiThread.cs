using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServicioTPVAgenteLocal
{
    public class MultiThread
    {
        public Thread[] Thread { get; set; }

        /// <summary>
        /// Metodo para Instanciar un arreglo de n hilos
        /// </summary>
        /// <param name="iNumThreads">parametro para el tamaño del arreglo del hilo</param>
        public MultiThread(int iNumThreads)
        {
            Thread = new Thread[iNumThreads];
        }


    }
}
