using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal class DeliveryData
    {
        public TgDeliveryDataItemIds DeliveryDataId { get; }
        public dynamic Data { get; }
        public string DataDescription { get; }
        public bool IsTicketedDelivery { get; }
        public TgTicketedDeliveryDataItemIds TicketedDeliveryDataId { get; }
    }
}
