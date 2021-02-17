using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServicioTPVAgenteLocal.BE.TankGauge
{
    public class TankGaugeFuellingPointHistory
    {

        #region Fields
        public long ID { get; set; }

        public string Ncompany { get; set; }

        public string Date { get; set; }

        public string StoreID { get; set; }

        public string UserID { get; set; }

        public int FuellingPointID { get; set; }

        public decimal GrandVolTotal { get; set; }

        public decimal GrandMoneyTotal { get; set; }

        public int GradeID { get; set; }

        public string GradeTotal { get; set; }

        public decimal GradeVolTotal { get; set; }
        #endregion

    }
}