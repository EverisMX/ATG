namespace DOMSLibrary
{
    internal class GetSiteDeliveryStatusResponse
    {
        public byte BitEstadoFlag { get; set; }
        public byte BitDeliverySeq { get; set; }
        public PSS_Forecourt_Lib.TankGaugeCollection TankGaugeCollection { get; set; }
    }
}
