namespace ServicioTPVAgenteLocal.BE
{
    public class ResponseApi
    {

        #region Fields
        /// <summary>
        /// Message of the return status
        /// </summary>
        private string message = string.Empty;

        /// <summary>
        /// Return status
        /// </summary>
        private int status = -1;

        /// <summary>
        /// Signature para Seguridad de servicios
        /// </summary>
        private string signature = string.Empty;

        /// <summary>
        /// Fecha Return
        /// </summary>
        private string timestamp = string.Empty;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        /// <exception cref="System.InvalidOperationException"></exception>
        public int Status
        {
            get { return status; }

            set
            {
                status = value;
                //try
                //{
                //    message = EnumHelper.GetStringValue((ApiResponseStatuses)value);
                //}
                //catch (Exception)
                //{
                //    throw new InvalidOperationException(value + " is an invalid ApiResponseStatus");
                //}
            }
        }

        /// <summary>
        /// Campo para Seguridad de servicios
        /// </summary>
        public string Signature
        {
            get { return signature; }
            set { signature = value; }
        }

        /// <summary>
        /// Fecha Creacion 
        /// </summary>
        public string Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        /// <summary>
        /// Campo Devuelto por el Request Solicitado.
        /// </summary>
        public object objResponse { get; set; }
        #endregion Properties

    }
}