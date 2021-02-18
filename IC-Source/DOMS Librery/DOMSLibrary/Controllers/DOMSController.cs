using PSS_Forecourt_Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal class DOMSController
    {
        #region Singleton
        private static DOMSController _instance = null;

        protected DOMSController()
        {

        }
        public static DOMSController GetInstance
        {
            get
            {
                if (_instance == null)
                    _instance = new DOMSController();

                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// Semaforo  para acceso a doms
        /// </summary>
        private SemaphoreSlim _DOMSSemaphore = new SemaphoreSlim(0, 1);

        private Forecourt Forecourt = null;

        private IFCConfig IFCConfig = null;

        public IEnumerable<TankInfo> ObtenerTanksGaugeData(string host, string posId, string maquina)
        {
            try
            {
                _DOMSSemaphore.Wait();
                var ret = new List<TankInfo>();
                // BLOQUE CRITICO
                ConnectToDOMS();

                TankGaugeCollection tgcSondaa = IFCConfig.TankGauges;
                foreach (TankGauge tankInfo in tgcSondaa)
                {
                    var tank = new TankInfo
                    {
                        Id = Convert.ToInt32(tankInfo.Id),
                        DataCollection = new List<TankDataCollection>(tankInfo.DataCollection.Count)
                    };

                    foreach (TankGaugeData tgdData in tankInfo.DataCollection)
                    {
                        tank.DataCollection.Add(new TankDataCollection
                        {
                            Data = tgdData.Data,
                            TankDataId = (TankDataId)tgdData.TankDataId
                        });
                    }
                    ret.Add(tank);
                }
                return ret;
            }
            catch (Exception e)
            {
                // LOG???
                throw;
            }
            finally
            {
                _performDisconnect();
                _DOMSSemaphore.Release();
            }
        }

        private void ConnectToDOMS()
        {
            try
            {
                _DOMSSemaphore.Wait();

                // BLOQUE CRITICO
                DOMSConnection._performanceConectionDOMS();
            }
            finally
            {
                _DOMSSemaphore.Release();

            }
        }

        /// <summary>
        /// PRECONDITION: LLAMAR DENTRO DE BLOQUE CRITICO
        /// </summary>
        private void _performDisconnect()
        {
            Forecourt.Disconnect();
        }
    }
}
