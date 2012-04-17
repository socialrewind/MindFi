using System;
using System.Collections.Generic;

namespace MBBetaAPI.SRAPI
{
    public class SRBackup
    {
        /// <summary>
        /// Initial date that is selected for backup
        /// </summary>
        public DateTime BackupPeriodSelectedStartDate { get; set; }
        /// <summary>
        /// End date that is selected for backup
        /// </summary>
        public DateTime BackupPeriodSelectedEndDate { get; set; }
        /// <summary>
        /// Initial date that is completed for backup
        /// </summary>
        public DateTime BackupCompletedStart { get; set; }
        /// <summary>
        /// End date date that is completed for backup
        /// </summary>
        public DateTime BackupCompletedEnd { get; set; }
        /// <summary>
        /// Initial date for the period in progress
        /// </summary>
        public DateTime CurrentPeriodStart { get; set; }
        /// <summary>
        /// End date for the period in progress
        /// </summary>
        public DateTime CurrentPeriodEnd { get; set; }

    }
}
