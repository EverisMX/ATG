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

        /// <summary>
        /// Metodo encargado para obtener la informacion de los tanques del varillaje.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="posId"></param>
        /// <param name="maquina"></param>
        /// <returns IEnumerable<TankInfo>></returns>
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

        public IEnumerable<TankDeliveryInfo> ObtenerDeliveriesTanksGauge(string host, string posId, string maquina)
        {
            try
            {
                _DOMSSemaphore.Wait();
                var ret = new List<TankDeliveryInfo>();
                ConnectToDOMS();

                TankGaugeCollection objtgcTankeDel;
                DeliveryDataCollection ddcDelivery;

                byte bytPosID;
                byte bitDeliverySeq;
                byte bitEstadoFlag;
                bytPosID = Convert.ToByte(posId);

                Forecourt.EventsDisabled = false;
                Forecourt.GetSiteDeliveryStatus(out bitEstadoFlag, out bitDeliverySeq, out objtgcTankeDel);

                if (bitEstadoFlag == 0) // indica que no se obtuvo informe report deilverys.
                {
                    return new List<TankDeliveryInfo>();
                }

                foreach (TankGauge tankInfo in objtgcTankeDel)
                {
                    var tank = new TankDeliveryInfo
                    {
                        Id = Convert.ToInt32(tankInfo.Id),
                        DataCollection = new List<TankDataCollection>(tankInfo.DataCollection.Count)
                    };
                    ddcDelivery = tankInfo.GetDeliveryData(bytPosID, out byte byNroReport);

                    foreach (DeliveryData tgdData in ddcDelivery)
                    {
                        tank.DeliveriesDataCollection.Add(new DeliveriesData
                        {
                            DeliveryDataId = (TgDeliveryDataItemIds)tgdData.DeliveryDataId,
                            Data = tgdData.Data,
                            DataDescription = tgdData.DataDescription,
                            IsTicketedDelivery = tgdData.IsTicketedDelivery,
                            TicketedDeliveryDataId = (TgTicketedDeliveryDataItemIds)tgdData.TicketedDeliveryDataId
                        });
                    }
                    ret.Add(tank);
                }
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _performDisconnect();
                _DOMSSemaphore.Release();
            }
        }

        public IList<FuellingPointData> ObtenerFuellingPoinsData(string host, string posId, string maquina)
        {
            try
            {
                _DOMSSemaphore.Wait();
                var ret = new List<FuellingPointData>();
                // BLOQUE CRITICO
                ConnectToDOMS();

                GradeCollection gcGrade = null;
                FuellingPointTotals fptPunto = null;
                Forecourt.EventsDisabled = false;
                gcGrade = IFCConfig.Grades;

                // MX- Se coloca la Interfaz de la invocacion del Fuelling para el Objeto.
                foreach (FuellingPoint fuellingPoint in IFCConfig.FuellingPoints)
                {
                    fptPunto = fuellingPoint.Totals[FpTotalTypes.GT_FUELLING_POINT_TOTAL];

                    foreach (GradeTotal gradeTotal in fptPunto.GradeTotals)
                    {
                        ret.Add(new FuellingPointData
                        {
                           FuellingPointID = fuellingPoint.Id,
                           GrandVolTotal = Convert.ToDecimal(fptPunto.GrandVolTotal),
                           GrandMoneyTotal = Convert.ToDecimal(fptPunto.GrandMoneyTotal),
                           GradeID = gradeTotal.GradeId,
                           GradeTotal = gcGrade.Item[gradeTotal.GradeId].Text,
                           GradeVolTotal = Convert.ToDecimal(gradeTotal.GradeVolTotal)
                    });
                    }
                }
                return ret;
            }
            catch (Exception)
            {
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

        /// <summary>
        /// Condicion: Debe llamarse dentro de un bloque critico
        /// Realiza la conexion a DOMS
        /// </summary>
        /// <param name="strHost"></param>
        /// <param name="bytPosID"></param>
        /// <param name="idmaquina"></param>
        /// <exception cref="InvalidOperationException">Si no ha podido realizar conexion</exception>
        private void ConnectToDOMS(string strHost, string bytPosID, string idmaquina)
        {
            string logon = "POS,UNSO_FPSTA_2,APPL_ID=" + idmaquina;

            Forecourt.PosId = Convert.ToByte(bytPosID);
            Forecourt.HostName = strHost;
            string strEstadoSonda = "Initialize";
            Forecourt.Initialize();
            strEstadoSonda = "Logon";

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
                    Forecourt.FcLogon2(logon, flpParametro);
                    auxLogon = true;

                }
                catch
                {
                    if (cnt == 3)
                        throw new InvalidOperationException("Intento de Conexión 3 veces fallida" + strEstadoSonda);
                    Thread.Sleep(1000); // Pausa de 1 segundo para reintentar el Logon
                }
            } while ((cnt < 3) && !auxLogon);

        }
    }
}
