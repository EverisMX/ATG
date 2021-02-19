using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal class DeliveriesData
    {
        public TgDeliveryDataItemIds DeliveryDataId { get; set; }
        public dynamic Data { get; set; }
        public string DataDescription { get; set; }
        public bool IsTicketedDelivery { get; set; }
        public TgTicketedDeliveryDataItemIds TicketedDeliveryDataId { get; set; }
    }
}
