using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServicioTPVAgenteLocal.BE.TankGauge
{
    public class TankGaugeAlarmHistoryRequest
    {
        public List<TankGaugeAlarmHistory> listTankGaugeAlarmsHistory { get; set; }
    }
}