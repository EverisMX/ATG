using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServicioTPVAgenteLocal.BE.TankGauge
{
    public class TankGaugeFuellingPointHistoryRequest
    {
        public List<TankGaugeFuellingPointHistory> tankGaugeFuellingPoingList { get; set; }
    }
}