using PSS_Forecourt_Lib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
namespace DOMSLibrary
{
    public class DOMSAlarmsProbes
    {
        private string TransformJson(object ObjectTransform)
        {
            return new JavaScriptSerializer().Serialize(ObjectTransform);
        }

        public string fnGetAlarm(string pstrHost, string pbytPosId, string pscompany, string pstoreID, string psUserID, string pstrMaquina, Forecourt fc0, IFCConfig ifc0)
        {

            AlarmTank objAlarmTank = null;
            List<AlarmTank> lstAlarmTank = null;
            TankGaugeCollection tgcSondaa = null;
            string json = "";
            string strFechaIso = "";
            TgMainStates tmsMainState = 0;
            byte bytStatus = 0;
            int intAlarmStatus = 0;

            try
            {
                    Thread.Sleep(5000);
                    fc0.EventsDisabled = false;
                    tgcSondaa = (TankGaugeCollection)ifc0.TankGauges;

                    if (tgcSondaa.Count > 0)
                    {
                        lstAlarmTank = new List<AlarmTank>();
                        strFechaIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        foreach (TankGauge tgesondaa2 in tgcSondaa)
                        {

                            tgesondaa2.GetStatus(out tmsMainState, out bytStatus, out intAlarmStatus);

                            if (Convert.ToByte(TgStatusBits.TGS_ALARM) == bytStatus)
                            {
                                objAlarmTank = new AlarmTank();
                                objAlarmTank.Ncompany = pscompany;
                                objAlarmTank.StoreID = pstoreID;
                                objAlarmTank.Date = strFechaIso;
                                objAlarmTank.UserID = psUserID;
                                objAlarmTank.TgID = tgesondaa2.Id;
                                objAlarmTank.bitAlarm = bytStatus;
                                objAlarmTank.AlarmStatus = intAlarmStatus;
                                lstAlarmTank.Add(objAlarmTank);
                            }
                        }
                        json = TransformJson(lstAlarmTank);
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

    }
}
