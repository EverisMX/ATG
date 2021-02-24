using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Script.Serialization;


namespace DOMSLibrary
{
    public class DOMSTankGauge
    {
        /// <summary>
        /// Controller de acceso a DOMS
        /// </summary>
        private readonly DOMSController Controller = DOMSController.GetInstance;

        public DOMSTankGauge()
        {
            Logger.Log("Instancia creada");
        }

        public string fnObtenerTankGaugeData(string pstrHost, string pbytPosId, string pscompany, string pstoreID, string psUserID, string pstrMaquina, string strTanksID)
        {
            TankGaugeDataHistoryBE objtgSonda = null;
            List<TankGaugeDataHistoryBE> LstObjtgSonda = null;
            string json = "";
            string strFechaIso = "";

            try
            {
                Logger.Log("Entrada", new { pstrHost, pbytPosId, pscompany, pstoreID, psUserID, pstrMaquina, strTanksID });
                /*
                * MX- Se utiliza para obtener los parametros del TankID con el TankGaudeID de la configuracio.
                */
                string[] objectItemTanks = strTanksID.Split('|');
                Thread.Sleep(5000); // Es necesario parar el hilo?

                // obtencion de datos de DOMS
                IList<TankGauge> tankInfoList = Controller.GetTanksGaugeDataDOMS(pstrHost, pbytPosId, pstrMaquina);

                if (tankInfoList.Count > 0)
                {
                    LstObjtgSonda = new List<TankGaugeDataHistoryBE>();
                    strFechaIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    foreach (TankGauge tgesondaa2 in tankInfoList)
                    {

                        objtgSonda = new TankGaugeDataHistoryBE();
                        objtgSonda.Ncompany = pscompany;
                        objtgSonda.StoreID = pstoreID;
                        objtgSonda.Date = strFechaIso;
                        objtgSonda.UserID = psUserID;

                        for (int i = 0; i < objectItemTanks.Length; i++)
                        {
                            string[] objectItemTankID = objectItemTanks[i].Split('-');
                            if (Convert.ToInt32(tgesondaa2.Id) == Convert.ToInt32(objectItemTankID[0]))
                                objtgSonda.TgID = Convert.ToInt32(objectItemTankID[1]);

                            objectItemTankID = null;
                        }

                        foreach (TankGaugeData tgdData in tgesondaa2.DataCollection)
                        {
                            if (tgdData.TankDataId == TankDataId.TGDID_TANK_PRODUCT_LEVEL)
                                objtgSonda.TankProductLevel = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_WATER_LEVEL)
                                objtgSonda.TankWaterLevel = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TOTAL_OBSERVED_VOL)
                                objtgSonda.TankTotalObservedVol = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_WATER_VOL)
                                objtgSonda.TankWaterVol = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_GROSS_OBSERVED_VOL)
                                objtgSonda.TankGrossObservedVol = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_GROSS_STD_VOL)
                                objtgSonda.TankGrossStdVol = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_AVAILABLE_ROOM)
                                objtgSonda.TankAvailableRoom = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_AVERAGE_TEMP)
                                objtgSonda.TankAverageTemp = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_DATA_LAST_UPDATE)
                                objtgSonda.TankDataLastUpdateDateAndTime = tgdData.Data.Year + "-" + tgdData.Data.Month + "-" + tgdData.Data.Day + " " + tgdData.Data.Hour + ":" + tgdData.Data.Minute + ":" + tgdData.Data.Second;
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_MAX_SAFE_FILL_CAPACITY)
                                objtgSonda.TankMaxSafeFillCapacity = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_SHELL_CAPACITY)
                                objtgSonda.TankShellCapacity = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_PROD_MASS)
                                objtgSonda.TankProductMass = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_PROD_DENSITY)
                                objtgSonda.TankProductDensity = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_PROD_TC_DENSITY)
                                objtgSonda.TankProductTcDensity = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_DENSITY_PROBE_TEMP)
                                objtgSonda.TankDensityProbeTemp = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_PROD_MASS)//no se encuentra enum por defecto TGDID_TANK_PROD_MASS
                                objtgSonda.TankSludGeLevel = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_PROD_MASS)//no se encuentra enum por defecto TGDID_TANK_PROD_MASS
                                objtgSonda.TankOilSepOilThickness = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_PROD_MASS)//no se encuentra enum por defecto TGDID_TANK_PROD_MASS
                                objtgSonda.TankOilSepOilVolume = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_TEMP_SENSOR1)
                                objtgSonda.TankTempSensor1 = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_TEMP_SENSOR2)
                                objtgSonda.TankTempSensor2 = Convert.ToDecimal(tgdData.Data);
                            else if (tgdData.TankDataId == TankDataId.TGDID_TANK_TEMP_SENSOR3)
                                objtgSonda.TankTempSensor3 = Convert.ToDecimal(tgdData.Data);
                        }
                        LstObjtgSonda.Add(objtgSonda);
                    }
                    json = TransformJson(LstObjtgSonda);
                }
                else
                {
                    json = "";
                }
                Logger.Log("salida", json);
                return json;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw new Exception(ex.Message, ex.InnerException);
            }

        }

        public string fnObtenerTankGaugeDelivery(string pstrHost, string pbytPosId, string pscompany, string pstoreID, string psUserID, string pstrMaquina, string strTanksID)
        {
            TankGaugeDeliveryHistoryBE objTank = null;
            List<TankGaugeDeliveryHistoryBE> LstobjTank = null;
            string json = "";
            string strFechaIso = "";

            try
            {
                Logger.Log("Entrada", new { pstrHost, pbytPosId, pscompany, pstoreID, psUserID, pstrMaquina, strTanksID });
                string[] objectItemTanksDev = strTanksID.Split('|');

                Thread.Sleep(5000); // Es necesario parar el hilo?

                // obtencion de datos de DOMS
                IList<TankDeliveryInfo> tankDeliveryInfoList = Controller.GetDeliveriesTanksGaugeDOMS(pstrHost, pbytPosId, pstrMaquina);

                if (tankDeliveryInfoList.Count > 0)
                {
                    LstobjTank = new List<TankGaugeDeliveryHistoryBE>();
                    strFechaIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    foreach (TankDeliveryInfo tankDeliveryInfo in tankDeliveryInfoList)
                    {
                        if (tankDeliveryInfo.DeliveriesDataCollection.Count > 0)
                        {
                            objTank = new TankGaugeDeliveryHistoryBE
                            {
                                Ncompany = pscompany,
                                StoreID = pstoreID,
                                Date = strFechaIso,
                                UserID = psUserID,
                                TgID = tankDeliveryInfo.Id
                            };

                            for (int i = 0; i < objectItemTanksDev.Length; i++)
                            {
                                string[] objectItemTankDevID = objectItemTanksDev[i].Split('-');
                                if (Convert.ToInt32(tankDeliveryInfo.Id) == Convert.ToInt32(objectItemTankDevID[0]))
                                    objTank.TgID = Convert.ToInt32(objectItemTankDevID[1]);

                                objectItemTankDevID = null;
                            }

                            foreach (DeliveriesData ddData in tankDeliveryInfo.DeliveriesDataCollection)
                            {
                                objTank.Deliveries = ddData.DataDescription;
                                if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_PRODUCT_CODE)
                                    objTank.TgProductCode = ddData.Data;
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_DATE_AND_TIME)
                                    objTank.TankDeliveryStartDateAndTime = ddData.Data.Year + "-" + ddData.Data.Month + "-" + ddData.Data.Day + " " + ddData.Data.Hour + ":" + ddData.Data.Minute + ":" + ddData.Data.Second;
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_PROD_VOL)
                                    objTank.TankDeliveryStartProdVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_DELIVERY_SEQ_NO)
                                    objTank.TankDeliverySeqNo = ddData.Data;
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_WATER_VOL)
                                    objTank.TankDeliveryStartWaterVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_TEMP)
                                    objTank.TankDeliveryStartTemp = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_DATE_AND_TIME)
                                    objTank.TankDeliveryStopDateAndTime = ddData.Data.Year + "-" + ddData.Data.Month + "-" + ddData.Data.Day + " " + ddData.Data.Hour + ":" + ddData.Data.Minute + ":" + ddData.Data.Second;
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_PROD_VOL)
                                    objTank.TankDeliveryStopProdVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_WATER_VOL)
                                    objTank.TankDeliveryStopWaterVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_TEMP)
                                    objTank.TankDeliveryStopTemp = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_DELIVERED_VOL)
                                    objTank.TankDeliveredVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_PROD_DENSITY)
                                    objTank.TankDeliveryStartProductDensity = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_DELIVERY_REPORT_SEQ_NO)
                                    objTank.DeliveryReportSeqNo = ddData.Data;
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_PROD_DENSITY)
                                    objTank.TankDeliveryStopProductDensity = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_PROD_MASS)
                                    objTank.TankDeliveryStartProductMass = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_PROD_MASS)
                                    objTank.TankDeliveryStopProductMass = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_PROD_TC_VOL)
                                    objTank.TankDeliveryStartProdTcVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_PROD_TC_VOL)
                                    objTank.TankDeliveryStopProdTcVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_DELIVERED_TC_VOL)
                                    objTank.TankDeliveredTcVol = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_ADJUSTED_VOL)
                                    objTank.TankAdjustedVolume = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_ADJUSTED_TC_VOL)
                                    objTank.TankAdjustedTcVolume = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_START_PROD_TC_DENSITY)
                                    objTank.TankDeliveryStartProductTcDensity = Convert.ToDecimal(ddData.Data);
                                else if (ddData.DeliveryDataId == TgDeliveryDataItemIds.TGDDI_STOP_PROD_TC_DENSITY)
                                    objTank.TankDeliveryStopProductTcDensity = Convert.ToDecimal(ddData.Data);

                            }

                            LstobjTank.Add(objTank);

                        }
                    }

                    json = TransformJson(LstobjTank);


                }
                else
                {
                    json = "";
                }
                Logger.Log("salida", json);

                return json;

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw new Exception(ex.Message);
            }

        }

        public string fnObtenerPuntoCombustible(string pstrHost, string pbytPosId, string pscompany, string pstoreID, string psUserID, string pstrMaquina)
        {
            TankGaugeFuellingBE objFuelling = null;
            List<TankGaugeFuellingBE> lsttgfFuelling = new List<TankGaugeFuellingBE>();
            string json = "";
            string strFechaIso = "";

            try
            {
                Logger.Log("Entrada", new { pstrHost, pbytPosId, pscompany, pstoreID, psUserID, pstrMaquina });
                Thread.Sleep(5000); // es necesario parar el hilo?

                strFechaIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // obtencion de datos de DOMS
                var dataFuellingPoint = Controller.GetFuellingPoinsDataDOMS(pstrHost, pbytPosId, pstrMaquina);

                foreach (FuellingPointData grtGrado in dataFuellingPoint)
                {
                    objFuelling = new TankGaugeFuellingBE()
                    {
                        Ncompany = pscompany,
                        StoreID = pstoreID,
                        Date = strFechaIso,
                        UserID = psUserID,
                        FuellingPointID = grtGrado.FuellingPointID,
                        GrandVolTotal = grtGrado.GrandVolTotal,
                        GrandMoneyTotal = grtGrado.GrandMoneyTotal,
                        GradeID = grtGrado.GradeID,
                        GradeTotal = grtGrado.GradeTotal,
                        GradeVolTotal = grtGrado.GradeVolTotal
                    };
                    lsttgfFuelling.Add(objFuelling);
                }
                json = TransformJson(lsttgfFuelling);
                Logger.Log("salida", json);
                return json;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw new Exception(ex.Message);
            }

        }

        private string TransformJson(object ObjectTransform)
        {
            return new JavaScriptSerializer().Serialize(ObjectTransform);
        }

    }
}
