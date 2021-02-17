using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServicioTPVAgenteLocal;

public class StateObject
{
    // Client socket.  
    public Socket sWorkSocket = null;
    // Size of receive buffer.  
    public const int iBufferSize = 256;
    // Receive buffer.  
    public byte[] buffer = new byte[iBufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class SocketAsynchronousClient
{
    // The port number for the remote device.  
    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    private static ServiceBatchTPVCOFO Hilobatch;
    private static String sError = String.Empty;
    private static String sResponse = String.Empty;
    private static string strMsge = null;
    private static string sIP = null;
    private static string strNombreArchBatch = String.Empty;


    public string GetError()
    {
        return sError;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sPIP">IP Socket</param>
    /// <param name="iPport">Puerto Socket</param>
    /// <param name="ipIdxHilo"> ID Hilo asociado al socket </param>
    /// <param name="pstrMsge">Mensaje inicial procedente del inicio del Hoñp </param>
    /// <param name="ac">Variable AsyncCallback asociada al hilo</param>
    /// <param name="itimeOut">TimeOut para controlar la recepción del mensaje</param>
    /// <param name="pHilobatch">Instancia de clase ServiceBatchTPVCOFO asociada al Hilo de Ejecución</param>
    /// <param name="spstrNombreArchBatch">Nombre de archivo BATCH asociado al control de respuesta del Hilo, es vacion cuando no se controlan las respuestas</param>
    public void StartClient(string sPIP, int iPport, int ipIdxHilo, string pstrMsge, AsyncCallback ac, int itimeOut, ServiceBatchTPVCOFO pHilobatch, String spstrNombreArchBatch)
    {
        strNombreArchBatch = spstrNombreArchBatch;
        Hilobatch = pHilobatch;
        sIP = sPIP;
        strMsge = pstrMsge;

        try
        {
            // Establish the remote endpoint for the socket.  
            // The name of the   
            // remote device is "host.contoso.com".  
            //10.232.100.191 

            IPAddress ipAddress = IPAddress.Parse(sPIP);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, iPport);

            // Create a TCP/sIP socket.  
            Socket client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            client.ReceiveTimeout = itimeOut;

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP, ac, client);
            connectDone.WaitOne();

            // Send test data to the remote device.  
            Send(client, "Hilo " + ipIdxHilo + "<EOF>");
            sendDone.WaitOne();

            // Receive the sResponse from the remote device.  
            Receive(client);
            receiveDone.WaitOne();


            // Release the socket.  
            client.Shutdown(SocketShutdown.Both);
            client.Close();

        }
        catch (TimeoutException e)
        {
            ServiceLogTPVCOFO.Instance.WriteLine("\r\n" + e.InnerException.ToString() + "\r\n" + "---------------------------------------------------");

        }
        catch (Exception e)
        {
            ServiceLogTPVCOFO.Instance.WriteLine("\r\n" + e.InnerException.ToString() + "\r\n" + "---------------------------------------------------");

        }
    }

    /// <summary>
    /// Método que inicializa la varible AsyncCallback utilizado en el método ServiceWorkerTPVCOFO.TimerProcSocket
    /// </summary>
    /// <param name="ar">Variable AsyncCallback asociada al hilo</param> 
    public void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            sResponse += "Socket connected to " + client.RemoteEndPoint.ToString() + "\r\n";

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
            sError += e.ToString();
        }
    }

    /// <summary>
    /// Método delegado para que se ejecute al recibir una respuesta del Socket
    /// </summary>
    /// <param name="client">Instancia del cliente Socket</param>
    private static void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            StateObject state = new StateObject();
            state.sWorkSocket = client;
            state.sb.Append(strMsge);
            // Begin receiving the data from the remote device.  
            client.BeginReceive(state.buffer, 0, StateObject.iBufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            sError += e.ToString();
        }
    }

    /// <summary>
    /// Método delegado para que la respuesta se espera en modo Asincrono
    /// </summary>
    /// <param name="ar">Variable AsyncCallback asociada al hilo</param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the clientR socket   
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket clientR = state.sWorkSocket;
            int posRpta = 0;
            // Read data from the remote device.  
            int bytesRead = clientR.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));  
            }
            else
            {
                // All the data has arrived; put it in sResponse.  
                if (state.sb.Length > 1)
                {
                    sResponse = state.sb.ToString();
                }
                // Signal that all bytes have been received.  
                receiveDone.Set();
            } 
            if (strNombreArchBatch != "")
            {
                posRpta = state.sb.ToString().LastIndexOf("Check Socket ------");
                Hilobatch.WriteLineBatch(strNombreArchBatch, state.sb.ToString().Substring(posRpta + 1));
            }

            state.sb.Append("\r\n" + "Finaliza Proceso Timer Socket... ...");
            ServiceLogTPVCOFO.Instance.WriteLine(state.sb.ToString() + "\r\n" + "---------------------------------------------------");
            sResponse = "";


        }
        catch (Exception e)
        {
            sError += e.ToString();
            ServiceLogTPVCOFO.Instance.WriteLine("sError Socket " + sError + "\r\n" + "---------------------------------------------------");


        }
    }



    /// <summary>
    /// Método utilizado para el envío del mensaje al Socket
    /// </summary>
    /// <param name="client">Instancia Socket Cliente</param>
    /// <param name="sData">Mensaje enviado a Servidor Socket</param>
    private static void Send(Socket client, String sData)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(sData);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    /// <summary>
    /// Método utilizado para controlar de modo asíncronos el envío del mensaje al Socket
    /// </summary>
    /// <param name="ar">Variable AsyncCallback asociada al hilo</param>
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int iBytesSent = client.EndSend(ar);
            sResponse += "Sent" + iBytesSent + " bytes to server.";

            // Signal that all bytes have been sent.  
            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
