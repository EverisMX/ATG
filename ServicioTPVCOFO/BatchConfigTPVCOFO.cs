using System;

namespace ServicioTPVAgenteLocal
{
    public class BatchConfigTPVCOFO
    {
        public Batch BatchParams { get; set; }

        public BatchConfigTPVCOFO()
        {
            this.BatchParams = new Batch();
        }
     
        public class Batch
        {
            public string sExecutionHour { get; set; }
            public string sExecutionDate { get; set; }
            public int iExecutionState { get; set; }
            public string sLastExecutionDate { get; set; }


        
            public Batch()
            {
                this.iExecutionState = 0;
                this.sExecutionDate = DateTime.Now.ToShortDateString();
                this.sExecutionHour = "18:35";
                this.sLastExecutionDate = "";
            }
        }
    }
}
