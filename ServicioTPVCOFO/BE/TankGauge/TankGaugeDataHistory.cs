using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServicioTPVAgenteLocal.BE.TankGauge
{
    public class TankGaugeDataHistory
    {

        #region Fields

        public int ID { get; set; }

        public string Ncompany { get; set; }

        public string StoreID { get; set; }

        public string Date { get; set; }

        public string UserID { get; set; }

        public int TgID { get; set; }

        public decimal TankProductLevel { get; set; }

        public decimal TankWaterLevel { get; set; }

        public decimal TankTotalObservedVol { get; set; }

        public decimal TankWaterVol { get; set; }

        public decimal TankGrossObservedVol { get; set; }

        public decimal TankGrossStdVol { get; set; }

        public decimal TankAvailableRoom { get; set; }

        public decimal TankAverageTemp { get; set; }

        public string TankDataLastUpdateDateAndTime { get; set; }

        public decimal TankMaxSafeFillCapacity { get; set; }

        public decimal TankShellCapacity { get; set; }

        public decimal TankProductMass { get; set; }

        public decimal TankProductDensity { get; set; }

        public decimal TankProductTcDensity { get; set; }

        public decimal TankDensityProbeTemp { get; set; }

        public decimal TankSludGeLevel { get; set; }

        public decimal TankOilSepOilThickness { get; set; }

        public decimal TankOilSepOilVolume { get; set; }

        public decimal TankTempSensor1 { get; set; }

        public decimal TankTempSensor2 { get; set; }

        public decimal TankTempSensor3 { get; set; }
        #endregion

    }
}