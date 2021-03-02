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
        private readonly static SemaphoreSlim _DOMSSemaphore = new SemaphoreSlim(1, 1);

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

                TankGaugeCollection tgcSondaa = GetTankGaugeCollection();
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

                        TankGaugeDataCollection tankGaugeDataCollection = GetTankGaugeDataCollection(tankInfo);
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

                GetSiteDeliveryStatusResponse siteDeliveryStatus = GetSiteDeliveryStatus();
                Logger.Log("Llamada Forecourt.GetSiteDeliveryStatus con exito", new { siteDeliveryStatus });

                if (siteDeliveryStatus.BitEstadoFlag == 0) // indica que no se obtuvo informe report deilverys.
                {
                    Logger.Log("Salida con exito (BitEstadoFlag = 0, no se obtuvo infore report)");
                    return new List<TankDeliveryInfo>();
                }

                foreach (PSS_Forecourt_Lib.TankGauge tankInfo in siteDeliveryStatus.TankGaugeCollection)
                {
                    var tank = new TankDeliveryInfo
                    {
                        Id = Convert.ToInt32(tankInfo.Id)
                    };

                    GetDeliveryDataResponse getDeliveryDataResponse = GetDeliveryData(tankInfo, bytPosID);
                    Logger.Log("Llamada TankGauge.GetDeliveryData con exito.", new { getDeliveryDataResponse });

                    foreach (DeliveryData tgdData in getDeliveryDataResponse.DeliveryDataCollection)
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
                    DeliveryReportSeqNo = siteDeliveryStatus.BitDeliverySeq,
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

                FuellingPointCollection fpCollection = GetFuellingPointCollection();
                Logger.Log("Llamada IFCConfig.FuellingPoints con exito.", fpCollection);

                GradeCollection gcGrade = GetGradeCollection();
                Logger.Log("Llamada IFCConfig.Grades con exito", gcGrade);

                // MX- Se coloca la Interfaz de la invocacion del Fuelling para el Objeto.
                foreach (FuellingPoint fuellingPoint in fpCollection)
                {
                    FuellingPointTotals fptPunto = fuellingPoint.Totals[FpTotalTypes.GT_FUELLING_POINT_TOTAL];

                    GradeTotalCollection gradeTotals = GetGradeTotalCollection(fptPunto);
                    Logger.Log("Llamada GradeTotals con exito.", gradeTotals);
                    foreach (GradeTotal gradeTotal in gradeTotals)
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
       
        #region Function Privates

        /// <summary>
        /// PRECONDITION: LLAMAR DENTRO DE BLOQUE CRITICO
        /// </summary>
        private void _performDisconnect()
        {
            Logger.Log("Llamada  Forecourt.Disconnect");
            Forecourt.Disconnect();
            Logger.Log("Llamada  Forecourt.Disconnect con exito");
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

        /// <summary>
        /// Obtiene los TankGauges desde IFC.
        /// Se debe llamar desde un bloque critico.
        /// Realizara una serie de intentos para obtener la lectura, si no lo consigue fallará
        /// </summary>
        /// <returns></returns>
        private TankGaugeCollection GetTankGaugeCollection()
        {
            for (int i = 0; i < Configuration.MaxAttemptsToReadTankGauges; i++)
            {
                Logger.Log($"Intento {i + 1} de lectura TankGauge");
                TankGaugeCollection ret = IFCConfig.TankGauges;
                if (ret.Count > 0)
                {
                    return ret;
                }
                Logger.Log($"Intento {i + 1} de lectura TankGauge con resultado vacio. Paramos hilo {Configuration.SleepingMillisecondsBetweenAttemptsToReadTankGauges} ms");

                System.Threading.Thread.CurrentThread.Join(Configuration.SleepingMillisecondsBetweenAttemptsToReadTankGauges);
            }
            throw new InvalidOperationException($"No se ha recuperado TankGauges tras {Configuration.MaxAttemptsToReadTankGauges} intentos");
        }

        /// <summary>
        /// Obtiene TankGaugeDataCollection desde informacion de tankGauge
        /// Debe llamarse desde bloque critico
        /// Realizara una serie de intentos para obtener la lectura, si no lo consigue fallará
        /// </summary>
        /// <param name="tankInfo"></param>
        /// <returns></returns>
        private TankGaugeDataCollection GetTankGaugeDataCollection(PSS_Forecourt_Lib.TankGauge tankInfo)
        {
            for (int i = 0; i < Configuration.MaxAttemptsToReadTankGaugeData; i++)
            {
                Logger.Log($"Intento {i + 1} de lectura TankGaugeDataCollection");
                TankGaugeDataCollection ret = tankInfo.DataCollection;
                if (ret.Count > 0)
                {
                    return ret;
                }
                Logger.Log($"Intento {i + 1} de lectura TankGaugeDataCollection con resultado vacio. Paramos hilo {Configuration.SleepingMillisecondsBetweenAttemptsToReadTankGaugeData} ms");

                System.Threading.Thread.CurrentThread.Join(Configuration.SleepingMillisecondsBetweenAttemptsToReadTankGaugeData);
            }
            throw new InvalidOperationException($"No se ha recuperado TankGaugeDataCollection tras {Configuration.MaxAttemptsToReadTankGaugeData} intentos");
        }

        /// <summary>
        /// Obtiene los FuellingPointCollection desde IFC.
        /// Se debe llamar desde un bloque critico.
        /// Realizara una serie de intentos para obtener la lectura, si no lo consigue fallará
        /// </summary>
        /// <returns></returns>
        private FuellingPointCollection GetFuellingPointCollection()
        {
            for (int i = 0; i < Configuration.MaxAttemptsToReadFuellingPoints; i++)
            {
                Logger.Log($"Intento {i + 1} de lectura FuellingPointCollection");
                FuellingPointCollection ret = IFCConfig.FuellingPoints;
                if (ret.Count > 0)
                {
                    return ret;
                }
                Logger.Log($"Intento {i + 1} de lectura FuellingPointCollection con resultado vacio. Paramos hilo {Configuration.SleepingMillisecondsBetweenAttempsToReadFuellingPoints} ms");

                System.Threading.Thread.CurrentThread.Join(Configuration.SleepingMillisecondsBetweenAttempsToReadFuellingPoints);
            }
            throw new InvalidOperationException($"No se ha recuperado FuellingPointCollection tras {Configuration.MaxAttemptsToReadFuellingPoints} intentos");
        }

        /// <summary>
        /// Obtiene los GradeCollection desde IFC.
        /// Se debe llamar desde un bloque critico.
        /// Realizara una serie de intentos para obtener la lectura, si no lo consigue fallará
        /// </summary>
        /// <returns></returns>
        private GradeCollection GetGradeCollection()
        {
            for (int i = 0; i < Configuration.MaxAttemptsToReadGrades; i++)
            {
                Logger.Log($"Intento {i + 1} de lectura GradeCollection");
                GradeCollection ret = IFCConfig.Grades;
                if (ret.Count > 0)
                {
                    return ret;
                }
                Logger.Log($"Intento {i + 1} de lectura GradeCollection con resultado vacio. Paramos hilo {Configuration.SleepingMillisecondsBetweenAttempsToReadGrades} ms");

                System.Threading.Thread.CurrentThread.Join(Configuration.SleepingMillisecondsBetweenAttempsToReadGrades);
            }
            throw new InvalidOperationException($"No se ha recuperado GradeCollection tras {Configuration.MaxAttemptsToReadGrades} intentos");
        }

        /// <summary>
        /// Obtiene los GradeCollection desde IFC.
        /// Se debe llamar desde un bloque critico.
        /// Realizara una serie de intentos para obtener la lectura, si no lo consigue fallará
        /// </summary>
        /// <returns></returns>
        private GradeTotalCollection GetGradeTotalCollection(FuellingPointTotals fptPunto)
        {
            for (int i = 0; i < Configuration.MaxAttemptsToReadGradeTotals; i++)
            {
                Logger.Log($"Intento {i + 1} de lectura GradeTotals");
                GradeTotalCollection ret = fptPunto.GradeTotals;
                if (ret.Count > 0)
                {
                    return ret;
                }
                Logger.Log($"Intento {i + 1} de lectura GradeTotals con resultado vacio. Paramos hilo {Configuration.SleepingMillisecondsBetweenAttempsToReadGradeTotals} ms");

                System.Threading.Thread.CurrentThread.Join(Configuration.SleepingMillisecondsBetweenAttempsToReadGradeTotals);
            }
            throw new InvalidOperationException($"No se ha recuperado GradeTotals tras {Configuration.MaxAttemptsToReadGradeTotals} intentos");
        }

        /// <summary>
        /// Obtiene GetSiteDeliveryStatus desde Forecourt.
        /// Se debe llamar desde un bloque critico.
        /// Realizara una serie de intentos para obtener la lectura, si no lo consigue fallará
        /// </summary>
        /// <returns></returns>
        private GetSiteDeliveryStatusResponse GetSiteDeliveryStatus()
        {
            for (int i = 0; i < Configuration.MaxAttemptsToReadSiteDeliveryStatus; i++)
            {
                Logger.Log($"Intento {i + 1} de lectura SiteDeliveryStatus");
                Forecourt.GetSiteDeliveryStatus(out byte bitEstadoFlag, out byte bitDeliverySeq, out TankGaugeCollection objtgcTankeDel);
                if (objtgcTankeDel != null && objtgcTankeDel.Count > 0)
                {
                    return new GetSiteDeliveryStatusResponse
                    {
                        BitDeliverySeq = bitDeliverySeq,
                        BitEstadoFlag = bitEstadoFlag,
                        TankGaugeCollection = objtgcTankeDel
                    };
                }
                Logger.Log($"Intento {i + 1} de lectura SiteDeliveryStatus con resultado vacio. Paramos hilo {Configuration.SleepingMillisecondsBetweenAttempsToReadSiteDeliveryStatus} ms");

                System.Threading.Thread.CurrentThread.Join(Configuration.SleepingMillisecondsBetweenAttempsToReadSiteDeliveryStatus);
            }
            throw new InvalidOperationException($"No se ha recuperado SiteDeliveryStatus tras {Configuration.MaxAttemptsToReadSiteDeliveryStatus} intentos");
        }

        /// <summary>
        /// Obtiene GetDeliveryData desde TankGauge.
        /// Se debe llamar desde un bloque critico.
        /// Realizara una serie de intentos para obtener la lectura, si no lo consigue fallará
        /// </summary>
        /// <returns></returns>
        private GetDeliveryDataResponse GetDeliveryData(PSS_Forecourt_Lib.TankGauge tankInfo, byte bytPosId)
        {
            for (int i = 0; i < Configuration.MaxAttemptsToReadDeliveryData; i++)
            {
                Logger.Log($"Intento {i + 1} de lectura DeliveryData");
                DeliveryDataCollection ddcDelivery = tankInfo.GetDeliveryData(bytPosId, out byte byNroReport);
                if (ddcDelivery != null && ddcDelivery.Count > 0)
                {
                    return new GetDeliveryDataResponse
                    {
                        ByNroReport = byNroReport,
                        DeliveryDataCollection = ddcDelivery
                    };
                }
                Logger.Log($"Intento {i + 1} de lectura DeliveryData con resultado vacio. Paramos hilo {Configuration.SleepingMillisecondsBetweenAttempsToReadDeliveryData} ms");

                System.Threading.Thread.CurrentThread.Join(Configuration.SleepingMillisecondsBetweenAttempsToReadDeliveryData);
            }
            throw new InvalidOperationException($"No se ha recuperado DeliveryData tras {Configuration.MaxAttemptsToReadDeliveryData} intentos");
        }
        #endregion
    }
}
