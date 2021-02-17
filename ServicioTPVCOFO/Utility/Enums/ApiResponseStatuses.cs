using System.ComponentModel;

namespace ServicioTPVAgenteLocal.Utility.Enums
{
    internal enum ApiResponseStatuses
    {
        /// <summary>
        /// The successful
        /// <para> 400 : ApiResponseStatuses.Successful</para>
        /// </summary>
        [Description("Conforme")]
        Successful = 200,

        /// <summary>
        /// The unknown error
        /// <para> 155 : ApiResponseStatuses.ApiSignature_NotMatch</para>
        /// </summary>
        [Description("No coincide la firma.")]
        Signature_NotMatch = 155,

        /// <summary>
        /// The invalid parameter
        /// <para> 400 : ApiResponseStatuses.Bad Request</para>
        /// </summary>
        [Description("Error Description: ")]
        BadRequest = 400,

   
    }
}
