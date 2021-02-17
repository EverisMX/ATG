using System;

namespace DOMSLibrary
{
    /// <summary>
    /// Punto de Abastecimiento de Combustible
    /// </summary>
    public class TankGaugeFuellingBE
    {

        #region Fields
        public int ID { get; set; }
        public string Ncompany { get; set; }
        public string StoreID { get; set; }
        public string Date { get; set; }
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
