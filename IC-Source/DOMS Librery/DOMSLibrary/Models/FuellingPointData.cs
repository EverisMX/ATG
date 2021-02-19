using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal class FuellingPointData
    {
        public int FuellingPointID { get; set; }
        public decimal GrandVolTotal { get; set; }
        public decimal GrandMoneyTotal { get; set; }
        public int GradeID { get; set; }
        public string GradeTotal { get; set; }
        public decimal GradeVolTotal { get; set; }
    }
}
