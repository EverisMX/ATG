using ServicioTPVAgenteLocal.Utility;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Windows.Forms;

namespace ServicioTPVAgenteLocal
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        //public CustomParameters customParameters = null;
        public ProjectInstaller()
        {
            InitializeComponent();
        }

  //////      /// <summary>
		///////// To cause this method to be invoked, I added the primary project output to the 
		///////// setup project's custom actions, under the "Install" folder.
		///////// </summary>
		///////// <param name="stateSaver">A dictionary object that will be retrievable during the uninstall process.</param>
		//////public override void Install(System.Collections.IDictionary stateSaver)
  //////      {
  //////          // Get the custom parameters from the install context.
  //////          customParameters=new CustomParameters(this.Context);

  //////          SaveCustomParametersInStateSaverDictionary(stateSaver, customParameters);

  //////          PrintMessage("The application is being installed.", customParameters);

  //////          base.Install(stateSaver);
  //////      }

  //////      /// <summary>
		///////// Adds or updates the state dictionary so that custom parameter values can be retrieved when 
		///////// the application is uninstalled.
		///////// </summary>
		///////// <param name="stateSaver">An IDictionary object that will contain all the objects who's state is to be persisted across installations.</param>
		///////// <param name="customParameters">A strong typed object of custom parameters that will be saved.</param>
		//////private void SaveCustomParametersInStateSaverDictionary(System.Collections.IDictionary stateSaver, CustomParameters customParameters)
  //////      {
  //////          // Add/update the "MyCustomParameter" entry in the state saver so that it may be accessed on uninstall.
  //////          if (stateSaver.Contains(CustomParameters.Keys.MyCustomParameter) == true)
  //////              stateSaver[CustomParameters.Keys.MyCustomParameter] = customParameters.MyCustomParameter;
  //////          else
  //////              stateSaver.Add(CustomParameters.Keys.MyCustomParameter, customParameters.MyCustomParameter);

  //////      }

  //////      /// <summary>
  //////      /// A helper method that prints out the passed message, and a dumps the custom parameters object to a message box.
  //////      /// </summary>
  //////      /// <param name="message">The message header to place in the message box.</param>
  //////      /// <param name="customParameters">A strong typed object of valid command line parameters.</param>
  //////      private void PrintMessage(string message, CustomParameters customParameters)
  //////      {
  //////          string outputMessage = string.Format("{0}\r\nThe parameters that were recorded during install are:\r\n\r\n\t{1} = {2}",
  //////              message,
  //////              CustomParameters.Keys.MyCustomParameter, customParameters.MyCustomParameter);

  //////          MessageBox.Show(outputMessage, "Installer Custom Action Fired!", MessageBoxButtons.OK, MessageBoxIcon.Information);
  //////      }

    }
}
