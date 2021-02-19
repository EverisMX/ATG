using Microsoft.Win32;
using Newtonsoft.Json;
using ServicioTPVAgenteLocal.BE;
using ServicioTPVAgenteLocal.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ServicioTPVAgenteLocal
{
    public partial class ServiceTPVATG : ServiceBase
    {
        #region Vars
        //bool _paused=false;
        ServiceThreadTPVCOFO _serviceThread;
        //string strServidorPrincipal = "";
        #endregion //Vars

        public ServiceTPVATG()
        {
            InitializeComponent();
            SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
            //SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            //this..Current.Suspending += new SuspendingEventHandler(App_Suspending);
            this.AutoLog = false;
            this.CanPauseAndContinue = true;
            CanHandlePowerEvent = true;
            //_paused = false;
        }

        //private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        //{
        //    if (e.Mode == PowerModes.Suspend)
        //    {

        //        File.AppendAllLines(@"C:\prueba.txt", new string[] { "Supendido" }, Encoding.ASCII);
        //    }
        //    else
        //    {
        //        File.AppendAllLines(@"C:\prueba.txt", new string[] { "Reanudado" }, Encoding.ASCII);
        //    }

        //}

        protected override bool OnPowerEvent(PowerBroadcastStatus powerstatus)
        {
            _serviceThread = new ServiceThreadTPVCOFO();
            if (powerstatus == PowerBroadcastStatus.Suspend)
            {
                //Proceso p = Process.Start("shutdown.exe", "/ r");
                //p.WaitForExit();
                //File.AppendAllLines(@"C:\prueba.txt", new string[] { "Supendido - OnPowerEvent" }, Encoding.ASCII);
                EventLog.WriteEntry("Supensión de Equipo - Servicio de Windows");
                ServiceLogTPVCOFO.Instance.WriteLine("***********Supensión de Equipo - Servicio de Windows***********", true);
                _serviceThread.Stop("Stoped");
            }
            else if(powerstatus == PowerBroadcastStatus.ResumeSuspend)
            {
                //File.AppendAllLines(@"C:\prueba.txt", new string[] { "Reanudado - OnPowerEvent" }, Encoding.ASCII);
                EventLog.WriteEntry("Reanudado de Equipo - Servicio de Windows");
                ServiceLogTPVCOFO.Instance.WriteLine("***********Reanudado de Equipo - Servicio de Windows***********", true);
                _serviceThread.Start("Started");
            }
            return base.OnPowerEvent(powerstatus);
        }


        #region Methods Service Windows
        protected override void OnStart(string[] args)
        {
            String strError = "";
            //RegistryKey objRegistro = null;
            try
            {

                ServiceLogTPVCOFO.Instance.WriteLine("***********Inicio de Servicio Windows***********", true);
                //_paused = false;
                EventLog.WriteEntry("TPVCOFO System Monitor Started.");

                //if (args != null)
                //{
                //    strServidorPrincipal = args[0];
                //    ServiceLogTPVCOFO.Instance.WriteLine("Inicio Setear Valor Registro de Windows - Servidor: " + strServidorPrincipal, true);
                //    objRegistro = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ServiceTPVATG");
                //    objRegistro.SetValue("Servidor", strServidorPrincipal);
                //    objRegistro.Close();
                //    ServiceLogTPVCOFO.Instance.WriteLine("Fin Setear Valor Registro de Windows - Servidor: " + strServidorPrincipal, true);

                //}
                //else
                //{
                //    ServiceLogTPVCOFO.Instance.WriteLine("Inicio Obtener Valor Registro de Windows - Servidor", true);
                //    objRegistro = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ServiceTPVATG");
                //    strServidorPrincipal = objRegistro.GetValue("Servidor").ToString();
                //    objRegistro.Close();
                //    ServiceLogTPVCOFO.Instance.WriteLine("Inicio Obtener Valor Registro de Windows - Servidor", true);
                //}

                _serviceThread = new ServiceThreadTPVCOFO();
                _serviceThread.Start("Started");

                ServiceLogTPVCOFO.Instance.GrabarLog();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    strError = ex.InnerException.Message;
                else
                    strError = ex.Message;

                EventLog.WriteEntry("Started Error :" + strError);
                ServiceLogTPVCOFO.Instance.WriteLine("Error al Iniciar: " + ex.Message, true);
                ServiceLogTPVCOFO.Instance.GrabarLog();
            }
        }

        protected override void OnStop()
        {
            try
            {
                EventLog.WriteEntry("TPVCOFO System Monitor Stop.");
                ServiceLogTPVCOFO.Instance.WriteLine("Stop Servicio : ", true);
                _serviceThread.Stop("Stoped");
                ServiceLogTPVCOFO.Instance.GrabarLog();
                //_paused = true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    ServiceLogTPVCOFO.Instance.WriteLine("Stoped Error :" + ex.InnerException.Message,true);
                else
                    ServiceLogTPVCOFO.Instance.WriteLine("Stoped Error :" + ex.Message,true);
                ServiceLogTPVCOFO.Instance.GrabarLog();
            }
        }

        protected override void OnContinue()
        {
            try
            {
                EventLog.WriteEntry("TPVCOFO System Monitor Resumed.");
                //_paused = false;
                _serviceThread = new ServiceThreadTPVCOFO();
                _serviceThread.Start("Resumed");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    EventLog.WriteEntry("Resumed Error :" + ex.InnerException.Message);
                else
                    EventLog.WriteEntry("Resumed Error :" + ex.Message);
            }
        }

        protected override void OnPause()
        {
            try
            {
                EventLog.WriteEntry("TPVCOFO System Monitor Pause.");
                _serviceThread.Pause("Paused");
                //_paused = true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    EventLog.WriteEntry("Stoped Error :" + ex.InnerException.Message);
                else
                    EventLog.WriteEntry("Stoped Error :" + ex.Message);
            }
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            EventLog.WriteEntry("Apagado o Reinicio de Equipo");
            ServiceLogTPVCOFO.Instance.WriteLine("***********Apagado o Reinicio de Equipo - Servicio de Windows***********", true);
            _serviceThread.Stop("Stoped");
        }           
        #endregion //Methods
    }
}
