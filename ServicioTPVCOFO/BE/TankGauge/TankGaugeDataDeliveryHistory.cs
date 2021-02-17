using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServicioTPVAgenteLocal.BE.TankGauge
{
    public class TankGaugeDataDeliveryHistory
    {

        #region Fields

        public long ID { get; set; }

        public string Ncompany { get; set; }

        public string StoreID { get; set; }

        public string UserID { get; set; }

        public string Deliveries { get; set; }

        public int TgID { get; set; }

        public int TgProductCode { get; set; }

        public int TankDeliverySeqNo { get; set; }

        public string TankDeliveryStartDateAndTime { get; set; }

        public decimal TankDeliveryStartProdVol { get; set; }

        public decimal TankDeliveryStartWaterVol { get; set; }

        public decimal TankDeliveryStartTemp { get; set; }

        public string TankDeliveryStopDateAndTime { get; set; }

        public decimal TankDeliveryStopProdVol { get; set; }

        public decimal TankDeliveryStopWaterVol { get; set; }

        public decimal TankDeliveryStopTemp { get; set; }

        public decimal TankDeliveredVol { get; set; }

        public int DeliveryReportSeqNo { get; set; }

        public decimal TankDeliveryStartProductDensity { get; set; }

        public decimal TankDeliveryStopProductDensity { get; set; }

        public decimal TankDeliveryStartProductMass { get; set; }

        public decimal TankDeliveryStopProductMass { get; set; }

        public decimal TankDeliveryStartProdTcVol { get; set; }

        public decimal TankDeliveryStopProdTcVol { get; set; }

        public decimal TankDeliveredTcVol { get; set; }

        public decimal TankAdjustedVolume { get; set; }

        public decimal TankAdjustedTcVolume { get; set; }

        public decimal TankDeliverySaleVolDuringDelivey { get; set; }

        public decimal TankDeliveryStartProductTcDensity { get; set; }

        public decimal TankDeliveryStopProductTcDensity { get; set; }
        #endregion

    }
}