using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal class TankDeliveryInfo
    {
        public IList<TankDataCollection> DataCollection { get; set; } 
        public int Id { get; set; }
        public IList<DeliveriesData> DeliveriesDataCollection { get; set; } 

    }
}
