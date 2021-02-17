using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServicioTPVAgenteLocal.BE.TankGauge
{
    public class TankGaugeAlarmHistory
    {
        #region Fields
        public string NCOMPANY { get; set; }
        public string STOREID { get; set; }
        public string DATE { get; set; }
        public string USERID { get; set; }
        public string TGID { get; set; }
        public string CODESTATE { get; set; }
        public string TYPEALARM { get; set; }
        public string CODEALARM { get; set; }
        public string TEXTALARM { get; set; }

        #endregion Fields
    }
}