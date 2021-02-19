using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal class FuellingPoinsTotals
    {
        byte FpId { get; set; }
        double GrandVolTotal { get; set; }
        double GrandMoneyTotal { get; set; }
        FuellingPointsTotalTypes Type { get; set; }
        GradesFuellingTotals GradeTotals { get; set; }
    }
}
