using PSS_Forecourt_Lib;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DOMSLibrary
{
    /// <summary>
    /// Clase que controla el acceso a DOMS.
    /// Singleton, una unica instancia y un unico hilo conecta con DOMS.
    /// </summary>
    internal class DOMSController
    {
        #region Singleton
        private static DOMSController _instance = null;

        protected DOMSController()
        {
            Forecourt = new Forecourt();
            IFCConfig = (IFCConfig)Forecourt;
        }

        public static DOMSController GetInstance
        {
            get
            {
                _DOMSSemaphore.Wait();
                try
                {
                    if (_instance == null)
                    {
                        _instance = new DOMSController();
                    }
                }
                finally
                {
                    _DOMSSemaphore.Release();
                }
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// Semaforo  para acceso a doms
        /// </summary>
        private readonly static SemaphoreSlim _DOMSSemaphore = new SemaphoreSlim(0, 1);

        private readonly Forecourt Forecourt = null;
        private readonly IFCConfig IFCConfig = null;

        /// <summary>
        /// Metodo encargado para obtener la informacion de los tanques del varillaje.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="posId"></param>
        /// <param name="maquina"></param>
        /// <returns IEnumerable<TankInfo>></returns>
        public IList<TankGauge> GetTanksGaugeDataDOMS(string host, string posId, string maquina)
        {
            try
            {
                _DOMSSemaphore.Wait();
                Logger.Log("Entrada", new { host, posId, maquina });
                var ret = new List<TankGauge>();
                // BLOQUE CRITICO
                _connectToDOMS(host, posId, maquina);
                Logger.Log("Llamada Forecourt.EventsDisabled");
                Forecourt.EventsDisabled = false;
                Logger.Log("Llamada IFCConfig.TankGauges");
                TankGaugeCollection tgcSondaa = IFCConfig.TankGauges;
                Logger.Log("Llamada IFCConfig.TankGauges con exito", tgcSondaa);

                if (tgcSondaa.Count > 0)
                {
                    foreach (PSS_Forecourt_Lib.TankGauge tankInfo in tgcSondaa)
                    {
                        var tank = new TankGauge
                        {
                            Id = Convert.ToInt32(tankInfo.Id),
                            DataCollection = new List<TankGaugeData>(tankInfo.DataCollection.Count)
                        };
                        Logger.Log("Llamada TankGaugeDataCollection");
                        TankGaugeDataCollection tankGaugeDataCollection = tankInfo.DataCollection;
                        Logger.Log("Llamada TankGaugeDataCollection con exito.", tankGaugeDataCollection);

                        foreach (PSS_Forecourt_Lib.TankGaugeData tgdData in tankGaugeDataCollection)
                        {
                            tank.DataCollection.Add(new TankGaugeData
                            {
                                Data = tgdData.Data,
                                TankDataId = (TankDataId)tgdData.TankDataId
                            });
                        }
                        ret.Add(tank);
                    }
                }
                Logger.Log("Salida", ret);
                return ret;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw;
            }
            finally
            {
                _performDisconnect();
                _DOMSSemaphore.Release();
            }
        }

        /// <summary>
        /// Metodo encargado para obtener informacion de los deliveries o cargas iniciales(Albaranes) a los tanques
        /// </summary>
        /// <param name="host"></param>
        /// <param name="posId"></param>
        /// <param name="maquina"></param>
        /// <returns></returns>
        public IList<TankDeliveryInfo> GetDeliveriesTanksGaugeDOMS(string host, string posId, string maquina)
        {
            try
            {
                _DOMSSemaphore.Wait();
                Logger.Log("Entrada", new { host, posId, maquina });
                var ret = new List<TankDeliveryInfo>();
                _connectToDOMS(host, posId, maquina);
                byte bytPosID = Convert.ToByte(posId);

                Logger.Log("Llamada Forecourt.EventsDisabled");
                Forecourt.EventsDisabled = false;

                Logger.Log("Llamada Forecourt.GetSiteDeliveryStatus");
                Forecourt.GetSiteDeliveryStatus(out byte bitEstadoFlag, out byte bitDeliverySeq, out TankGaugeCollection objtgcTankeDel);
                Logger.Log("Llamada Forecourt.GetSiteDeliveryStatus con exito", new { bitEstadoFlag, bitDeliverySeq, objtgcTankeDel });

                if (bitEstadoFlag == 0) // indica que no se obtuvo informe report deilverys.
                {
                    Logger.Log("Salida con exito");
                    return new List<TankDeliveryInfo>();
                }

                foreach (PSS_Forecourt_Lib.TankGauge tankInfo in objtgcTankeDel)
                {
                    var tank = new TankDeliveryInfo
                    {
                        Id = Convert.ToInt32(tankInfo.Id)
                    };

                    Logger.Log("Llamada Forecourt.GetSiteDeliveryStatus");
                    DeliveryDataCollection ddcDelivery = tankInfo.GetDeliveryData(bytPosID, out byte byNroReport);
                    Logger.Log("Llamada Forecourt.GetSiteDeliveryStatus con exito.", new { ddcDelivery, byNroReport });

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
                ClrTankDeliveryDataParms ctdParametro = new ClrTankDeliveryDataParms
                {
                    DeliveryReportSeqNo = bitDeliverySeq,
                    PosId = bytPosID
                };

                Logger.Log("Llamada Forecourt.ClrTankDeliveryData", ctdParametro);
                Forecourt.ClrTankDeliveryData(ctdParametro);
                Logger.Log("Llamada Forecourt.ClrTankDeliveryData con exito");

                Logger.Log("Salida", ret);
                return ret;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw;
            }
            finally
            {
                _performDisconnect();
                _DOMSSemaphore.Release();
            }
        }

        /// <summary>
        /// Metodo encargado para extraer informacion de los surtidores calculados en el controlador,
        /// tratarlo en el controller y mandarlo para realizar un json final.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="posId"></param>
        /// <param name="maquina"></param>
        /// <returns></returns>
        public IList<FuellingPointData> GetFuellingPoinsDataDOMS(string host, string posId, string maquina)
        {
            try
            {
                _DOMSSemaphore.Wait();
                Logger.Log("Entrada", new { host, posId, maquina });
                var ret = new List<FuellingPointData>();
                // BLOQUE CRITICO
                _connectToDOMS(host, posId, maquina);

                Logger.Log("Llamada Forecourt.EventsDisabled");
                Forecourt.EventsDisabled = false;

                Logger.Log("Llamada IFCConfig.FuellingPoints");
                FuellingPointCollection fpCollection = IFCConfig.FuellingPoints;
                Logger.Log("Llamada IFCConfig.FuellingPoints con exito.", fpCollection);

                // MX- Se coloca la Interfaz de la invocacion del Fuelling para el Objeto.
                foreach (FuellingPoint fuellingPoint in fpCollection)
                {
                    FuellingPointTotals fptPunto = fuellingPoint.Totals[FpTotalTypes.GT_FUELLING_POINT_TOTAL];

                    Logger.Log("Llamada GradeTotals");
                    GradeTotalCollection gradeTotals = fptPunto.GradeTotals;
                    Logger.Log("Llamada GradeTotals con exito.", gradeTotals);
                    foreach (GradeTotal gradeTotal in gradeTotals)
                    {
                        ret.Add(new FuellingPointData
                        {
                            FuellingPointID = fuellingPoint.Id,
                            GrandVolTotal = Convert.ToDecimal(fptPunto.GrandVolTotal),
                            GrandMoneyTotal = Convert.ToDecimal(fptPunto.GrandMoneyTotal),
                            GradeID = gradeTotal.GradeId,
                            GradeTotal = ((GradeCollection)null).Item[gradeTotal.GradeId].Text,
                            GradeVolTotal = Convert.ToDecimal(gradeTotal.GradeVolTotal)
                        });
                    }
                }

                Logger.Log("Salida", ret);
                return ret;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw;
            }
            finally
            {
                _performDisconnect();
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
        private void _connectToDOMS(string strHost, string bytPosID, string idmaquina)
        {
            string logon = "POS,UNSO_FPSTA_2,APPL_ID=" + idmaquina;

            Forecourt.PosId = Convert.ToByte(bytPosID);
            Forecourt.HostName = strHost;
            string strEstadoSonda = "Initialize";
            Logger.Log("Llamada  Forecourt.Initialize");
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
                    Logger.Log("Llamada  Forecourt.FcLogon2", new { logon, flpParametro });
                    Forecourt.FcLogon2(logon, flpParametro);
                    Logger.Log("Llamada  Forecourt.FcLogon2 con exito");
                    auxLogon = true;

                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    if (cnt == 3)
                        throw new InvalidOperationException("Intento de Conexión 3 veces fallida" + strEstadoSonda);
                    Thread.Sleep(1000); // Pausa de 1 segundo para reintentar el Logon
                }
            } while ((cnt < 3) && !auxLogon);

        }
    }
}
