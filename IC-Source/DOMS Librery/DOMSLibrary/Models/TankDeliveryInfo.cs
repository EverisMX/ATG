using System.Collections.Generic;

namespace DOMSLibrary
{
    internal class TankDeliveryInfo
    {
        public IList<TankGaugeData> DataCollection { get; set; } = new List<TankGaugeData>();
        public int Id { get; set; }
        public IList<DeliveriesData> DeliveriesDataCollection { get; set; } = new List<DeliveriesData>();

    }
}
