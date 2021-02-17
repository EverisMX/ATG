using System.Reflection;
using System.Collections.Generic;
using System.ServiceProcess;

namespace ServicioTPVAgenteLocal
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.BeforeInstall += new
            System.Configuration.Install.InstallEventHandler(ProjectInstaller_BeforeInstall);

            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();

            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.Description = "Service Version: " + Assembly.GetExecutingAssembly().GetName().Version; 
            this.serviceInstaller1.DisplayName = "System Monitor TPV Agente Local MX";
            this.serviceInstaller1.ServiceName = "ServiceTPVATG";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
//            this.serviceInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.serviceInstaller1});

        }

        void ProjectInstaller_BeforeInstall(object sender, System.Configuration.Install.InstallEventArgs e)
        {
            List<ServiceController> services = new List<ServiceController>(ServiceController.GetServices());

            foreach (ServiceController s in services)
            {
                if (s.ServiceName == this.serviceInstaller1.ServiceName)
                {
                    ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
                    ServiceInstallerObj.Context = new System.Configuration.Install.InstallContext();
                    ServiceInstallerObj.Context = Context;
                    ServiceInstallerObj.ServiceName = "ServiceTPVATG";
                    ServiceInstallerObj.Uninstall(null);

                    break;
                }
            }
        }
        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
    }
}