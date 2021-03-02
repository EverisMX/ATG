using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOMSLibrary
{
    internal static class Configuration
    {
        /// <summary>
        /// Intentos para acceder a TankGaugeCollection
        /// </summary>
        internal static int MaxAttemptsToReadTankGauges => 20;

        /// <summary>
        /// Tiempo de espera entre intentos de lectura TankGaugeCollection
        /// </summary>
        internal static int SleepingMillisecondsBetweenAttemptsToReadTankGauges => 500;

        /// <summary>
        /// Intentos para acceder a TankGaugeDataCollection
        /// </summary>
        internal static int MaxAttemptsToReadTankGaugeData => 20;

        /// <summary>
        /// Tiempo de espera entre intentos de lectura TankGaugeDataCollection
        /// </summary>
        internal static int SleepingMillisecondsBetweenAttemptsToReadTankGaugeData => 500;

        /// <summary>
        /// Intentos para acceder a FuellingPoints
        /// </summary>
        internal static int MaxAttemptsToReadFuellingPoints => 20;

        /// <summary>
        /// Tiempo de espera entre intentos de lectura FuellingPoints
        /// </summary>
        internal static int SleepingMillisecondsBetweenAttempsToReadFuellingPoints => 500;

        /// <summary>
        /// Intentos para acceder a Grades
        /// </summary>
        internal static int MaxAttemptsToReadGrades => 20;

        /// <summary>
        /// Tiempo de espera entre intentos de lectura Grades
        /// </summary>
        internal static int SleepingMillisecondsBetweenAttempsToReadGrades => 500;

        /// <summary>
        /// Intentos para acceder a GradeTotals
        /// </summary>
        internal static int MaxAttemptsToReadGradeTotals => 20;

        /// <summary>
        /// Tiempo de espera entre intentos de lectura GradeTotals
        /// </summary>
        internal static int SleepingMillisecondsBetweenAttempsToReadGradeTotals => 500;

        /// <summary>
        /// Intentos para acceder a SiteDeliveryStatus
        /// </summary>
        internal static int MaxAttemptsToReadSiteDeliveryStatus => 20;

        /// <summary>
        /// Tiempo de espera entre intentos de lectura SiteDeliveryStatus
        /// </summary>
        internal static int SleepingMillisecondsBetweenAttempsToReadSiteDeliveryStatus => 500;

        /// <summary>
        /// Intentos para acceder a DeliveryData
        /// </summary>
        internal static int MaxAttemptsToReadDeliveryData => 20;

        /// <summary>
        /// Tiempo de espera entre intentos de lectura DeliveryData
        /// </summary>
        internal static int SleepingMillisecondsBetweenAttempsToReadDeliveryData => 500;

    }
}
