using PSS_Forecourt_Lib;
using System;
using System.Collections.Generic;
using System.Threading;
using DOMSLibrary.Utility;
using System.Web.Script.Serialization;


namespace DOMSLibrary
{
    public class DOMSTankGauge
    {
        public DOMSTankGauge()
        {

        }

        public string fnObtenerTankGaugeData(string pstrHost, string pbytPosId, string pscompany, string pstoreID, string psUserID, string pstrMaquina, string strTanksID, Forecourt fc0, IFCConfig ifc0) {

            TankGaugeDataHistoryBE objtgSonda = null;
            List<TankGaugeDataHistoryBE> LstObjtgSonda = null;
            TankGaugeDataCollection objddcTanke = null;
            TankGaugeCollection tgcSondaa = null;
            string json = "";
            string strFechaIso = "";

            try
            {
                    string[] objectItemTanks = strTanksID.Split('|');

                    Thread.Sleep(5000);
                    fc0.EventsDisabled = false;
                    tgcSondaa = (TankGaugeCollection)ifc0.TankGauges;

                    if (tgcSondaa.Count > 0)
                    {
                        LstObjtgSonda = new List<TankGaugeDataHistoryBE>();
                        strFechaIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        foreach (TankGauge tgesondaa2 in tgcSondaa)
                        {

                            objtgSonda = new TankGaugeDataHistoryBE();
                            objddcTanke = tgesondaa2.DataCollection;
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
                                if (tgdData.TankDataId == TankDataIds.TGDID_TANK_PRODUCT_LEVEL)
                                    objtgSonda.TankProductLevel = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_WATER_LEVEL)
                                    objtgSonda.TankWaterLevel = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TOTAL_OBSERVED_VOL)
                                    objtgSonda.TankTotalObservedVol = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_WATER_VOL)
                                    objtgSonda.TankWaterVol = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_GROSS_OBSERVED_VOL)
                                    objtgSonda.TankGrossObservedVol = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_GROSS_STD_VOL)
                                    objtgSonda.TankGrossStdVol = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_AVAILABLE_ROOM)
                                    objtgSonda.TankAvailableRoom = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_AVERAGE_TEMP)
                                    objtgSonda.TankAverageTemp = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_DATA_LAST_UPDATE)
                                   objtgSonda.TankDataLastUpdateDateAndTime = tgdData.Data.Year + "-" + tgdData.Data.Month + "-" + tgdData.Data.Day + " " + tgdData.Data.Hour+":"+ tgdData.Data.Minute +":"+ tgdData.Data.Second;
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_MAX_SAFE_FILL_CAPACITY)
                                    objtgSonda.TankMaxSafeFillCapacity = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_SHELL_CAPACITY)
                                    objtgSonda.TankShellCapacity = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_PROD_MASS)
                                    objtgSonda.TankProductMass = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_PROD_DENSITY)
                                    objtgSonda.TankProductDensity = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_PROD_TC_DENSITY)
                                    objtgSonda.TankProductTcDensity = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_DENSITY_PROBE_TEMP)
                                    objtgSonda.TankDensityProbeTemp = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_PROD_MASS)//no se encuentra enum por defecto TGDID_TANK_PROD_MASS
                                    objtgSonda.TankSludGeLevel = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_PROD_MASS)//no se encuentra enum por defecto TGDID_TANK_PROD_MASS
                                    objtgSonda.TankOilSepOilThickness = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_PROD_MASS)//no se encuentra enum por defecto TGDID_TANK_PROD_MASS
                                    objtgSonda.TankOilSepOilVolume = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_TEMP_SENSOR1)
                                    objtgSonda.TankTempSensor1 = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_TEMP_SENSOR2)
                                    objtgSonda.TankTempSensor2 = Convert.ToDecimal(tgdData.Data);
                                else if (tgdData.TankDataId == TankDataIds.TGDID_TANK_TEMP_SENSOR3)
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

                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException );
            }

        }
        
        public string fnObtenerTankGaugeDelivery(string pstrHost, string pbytPosId, string pscompany, string pstoreID, string psUserID, string pstrMaquina, string strTanksID, Forecourt fc1, IFCConfig ifc1)
        {
            TankGaugeDeliveryHistoryBE objTank = null;
            List<TankGaugeDeliveryHistoryBE> LstobjTank = null;
            byte byNroReport;
            byte bitDeliverySeq;
            byte bitEstadoFlag;
            TankGaugeCollection objtgcTankeDel;
            DeliveryDataCollection ddcDelivery;
            byte bytPosID;
            ClrTankDeliveryDataParms ctdParametro = null;
            string json = "";
            string strFechaIso = "";

            try
            {
                    string[] objectItemTanksDev = strTanksID.Split('|');

                    Thread.Sleep(5000);
                    bytPosID = Convert.ToByte(pbytPosId);
                    fc1.EventsDisabled = false;
                    fc1.GetSiteDeliveryStatus(out bitEstadoFlag, out bitDeliverySeq, out objtgcTankeDel);

                    if (bitEstadoFlag == 0) // indica que no se obtuvo informe report deilverys.
                    {
                        return "";
                    }
                    ctdParametro = new ClrTankDeliveryDataParms();

                    if (objtgcTankeDel.Count > 0)
                    {
                        LstobjTank = new List<TankGaugeDeliveryHistoryBE>();
                        strFechaIso= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        foreach (TankGauge tgeSonda1 in objtgcTankeDel)
                        {

                            ddcDelivery = tgeSonda1.GetDeliveryData(bytPosID, out byNroReport);

                            if (ddcDelivery.Count > 0)
                            {
                                objTank = new TankGaugeDeliveryHistoryBE();
                                objTank.Ncompany = pscompany;
                                objTank.StoreID = pstoreID;
                                objTank.Date = strFechaIso;
                                objTank.UserID = psUserID;
                                objTank.TgID = tgeSonda1.Id;

                                for (int i = 0; i < objectItemTanksDev.Length; i++)
                                {
                                    string[] objectItemTankDevID = objectItemTanksDev[i].Split('-');
                                    if (Convert.ToInt32(tgeSonda1.Id) == Convert.ToInt32(objectItemTankDevID[0]))
                                        objTank.TgID = Convert.ToInt32(objectItemTankDevID[1]);

                                    objectItemTankDevID = null;
                                }

                                foreach (DeliveryData ddData in ddcDelivery)
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

                        json=  TransformJson(LstobjTank);
                        ctdParametro.DeliveryReportSeqNo = bitDeliverySeq;
                        ctdParametro.PosId = bytPosID;
                        fc1.ClrTankDeliveryData(ctdParametro);

                    }
                    else
                    {
                        json = "";
                    }

                return json;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public string fnObtenerPuntoCombustible(string pstrHost, string pbytPosId, string pscompany, string pstoreID, string psUserID, string pstrMaquina, Forecourt fc2, IFCConfig ifc2)
        {

            TankGaugeFuellingBE objFuelling = null;
            List<TankGaugeFuellingBE> lsttgfFuelling = new List<TankGaugeFuellingBE>();
            string json = "";
            string strFechaIso = "";

            try
            {

                    Thread.Sleep(5000);
                    GradeCollection gcGrade = null;
                    FuellingPointTotals fptPunto = null;

                    fc2.EventsDisabled = false;
                    gcGrade = ifc2.Grades;
                    strFechaIso= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    foreach (FuellingPoint fp in ifc2.FuellingPoints)
                    {
                        fptPunto = fp.Totals[FpTotalTypes.GT_FUELLING_POINT_TOTAL];
                        foreach (GradeTotal grtGrado in fptPunto.GradeTotals)
                        {
                            objFuelling = new TankGaugeFuellingBE();
                            objFuelling.Ncompany = pscompany;
                            objFuelling.StoreID = pstoreID;
                            objFuelling.Date = strFechaIso;
                            objFuelling.UserID = psUserID;
                            objFuelling.FuellingPointID = fp.Id;
                            objFuelling.GrandVolTotal = Convert.ToDecimal(fptPunto.GrandVolTotal);
                            objFuelling.GrandMoneyTotal = Convert.ToDecimal(fptPunto.GrandMoneyTotal);
                            objFuelling.GradeID = grtGrado.GradeId;
                            objFuelling.GradeTotal = gcGrade.Item[grtGrado.GradeId].Text;
                            objFuelling.GradeVolTotal = Convert.ToDecimal(grtGrado.GradeVolTotal);
                            lsttgfFuelling.Add(objFuelling);
                        }
                    }
                    json = TransformJson(lsttgfFuelling);
               
                return json;

            }
            catch (Exception ex)
            {
                throw new Exception (ex.Message );
            }

        }

        private string TransformJson(object ObjectTransform)
        {
            return new JavaScriptSerializer().Serialize(ObjectTransform);
        }
        
    }
}
