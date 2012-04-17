using System;
using System.Data.SQLite;
using System.Collections.Generic;
using MBBetaAPI.AgentAPI;

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

        #region "Backup table operations"
        /// <summary>
        /// Record the start of a backup
        /// </summary>
        public static bool StartBackup(DateTime startPeriod, DateTime endPeriod,
            out DateTime currentBackupStart, out DateTime currentBackupEnd,
            out int BackupNo,
            out DateTime currentPeriodStart, out DateTime currentPeriodEnd,
            out bool isIncremental, out int currentState)
        {
            //bool inTransaction = false;

            BackupNo = 0;
            isIncremental = false;
            // allows for detection, if not set, they are equal
            currentPeriodStart = DateTime.Today;
            currentPeriodEnd = currentPeriodStart;
            currentBackupStart = startPeriod;
            currentBackupEnd = endPeriod;
            currentState = AsyncReqQueue.BACKUPFRIENDSINFO;

            lock (DBLayer.obj)
            {
                try
                {
                    DBLayer.DatabaseInUse = true;
                    DBLayer.GetConn();
                    // check first if there is an active Backup
                    string SQL = "select ID, CurrentStartTime, CurrentEndTime, PeriodStartTime, PeriodEndTime, CurrentBackupState from Backups where Active = 1";
                    SQLiteCommand CheckCmd = new SQLiteCommand(SQL, DBLayer.conn);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        BackupNo = reader.GetInt32(0);
                        currentPeriodStart = reader.GetDateTime(1);
                        currentPeriodEnd = reader.GetDateTime(2);
                        DateTime tempPeriodStart = reader.GetDateTime(3);
                        currentBackupStart = tempPeriodStart;
                        DateTime tempPeriodEnd = reader.GetDateTime(4);
                        currentBackupEnd = tempPeriodEnd;
                        currentState = reader.GetInt32(5);
                        reader.Close();
                    }
                    else
                    {
                        reader.Close();
                        // check if it will be an incremental or full backup
                        SQL = "select min(PeriodStartTime), max(PeriodEndTime), max(StartTime) from Backups where Active = 0";
                        CheckCmd = new SQLiteCommand(SQL, DBLayer.conn);
                        reader = CheckCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0) && !reader.IsDBNull(1) && !reader.IsDBNull(2))
                            {
                                DateTime tempPeriodStart = reader.GetDateTime(0);
                                DateTime tempPeriodEnd = reader.GetDateTime(1);
                                DateTime tempLastBackupDone = reader.GetDateTime(2);
                                // Verifying logic
                                //tempLastBackupDone = tempLastBackupDone.Date;

                                // Logic for incremental: if period start is similar to existing, complete backup, only go forward; else, full backup needed
                                // if existing backup covers the past, go only forward; else cover the period back first
                                if (tempPeriodStart <= currentBackupStart)
                                {
                                    //currentBackupStart = (tempPeriodEnd >= tempLastBackupDone.AddDays(-1)) ? tempLastBackupDone.AddDays(-1) : tempPeriodEnd;
                                    currentBackupStart = (tempPeriodEnd >= tempLastBackupDone) ? tempLastBackupDone : tempPeriodEnd;
                                    // TEST
                                    currentBackupStart = tempPeriodStart;
                                    currentBackupEnd = (tempPeriodEnd > endPeriod) ? tempPeriodEnd : endPeriod;
                                    //if (DateTime.Today.AddMonths(1) > endPeriod)
                                    if (DateTime.Today.AddMonths(1) > currentBackupEnd)
                                    {
                                        currentBackupEnd = DateTime.Today.AddMonths(1);
                                    }
                                    if (currentBackupEnd < currentBackupStart.AddMonths(1))
                                    {
                                        currentBackupEnd = currentBackupStart.AddMonths(1);
                                    }
                                    isIncremental = true;
                                }
                                else
                                {
                                    // initially, backup only the past
                                    currentBackupEnd = tempPeriodStart;
                                }
                            }
                        }
                        reader.Close();

                        // No active backup, insert it
                        SQL = "insert into Backups (StartTime, CurrentBackupState, PeriodStartTime, PeriodEndTime, CurrentStartTime, CurrentEndTime, Active) values (?,?,?,?,?,?,1)";
                        SQLiteCommand InsertCmd = new SQLiteCommand(SQL, DBLayer.conn);
                        // Calculation
                        currentPeriodEnd = endPeriod;
                        // Now go back by a month unless it is the one pointing to the future from now and it is an incremental backup
                        DateTime temp1 = DateTime.Today;
                        DateTime temp2 = endPeriod.AddMonths(-1);
                        if (!isIncremental)
                        {
                            temp2 = temp2.AddMonths(-1);
                        }
                        currentPeriodStart = (temp1 < temp2) ? temp1 : temp2;
                        SQLiteParameter pStart = new SQLiteParameter();
                        pStart.Value = DateTime.Now;
                        InsertCmd.Parameters.Add(pStart);
                        SQLiteParameter pState = new SQLiteParameter();
                        pState.Value = 0;
                        InsertCmd.Parameters.Add(pState);
                        SQLiteParameter pStartP = new SQLiteParameter();
                        pStartP.Value = currentBackupStart;
                        InsertCmd.Parameters.Add(pStartP);
                        SQLiteParameter pendP = new SQLiteParameter();
                        pendP.Value = currentBackupEnd;
                        InsertCmd.Parameters.Add(pendP);
                        SQLiteParameter pstartC = new SQLiteParameter();
                        pstartC.Value = currentPeriodStart;
                        InsertCmd.Parameters.Add(pstartC);
                        SQLiteParameter pendC = new SQLiteParameter();
                        pendC.Value = currentPeriodEnd;
                        InsertCmd.Parameters.Add(pendC);
                        InsertCmd.ExecuteNonQuery();
                        SQL = "select ID from Backups where StartTime=?";
                        CheckCmd = new SQLiteCommand(SQL, DBLayer.conn);
                        CheckCmd.Parameters.Add(pStart);
                        reader = CheckCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            BackupNo = reader.GetInt32(0);
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    string ErrorMessage = "Error creating backup \n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            } // lock
            return true;
        }

        public static bool GetLastBackup(out string backupState, out string backupDate)
        {
            // TODO: Localization
            backupState = "No Backup available";
            backupDate = "Never updated";
            lock (DBLayer.obj)
            {
                try
                {
                    DBLayer.DatabaseInUse = true;
                    DBLayer.GetConn();
                    // check first if there is an active Backup
                    string SQL = "select StartTime, EndTime, Active from Backups";
                    SQLiteCommand CheckCmd = new SQLiteCommand(SQL, DBLayer.conn);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        DateTime currentPeriodStart = reader.GetDateTime(0);
                        DateTime? currentPeriodEnd = null;
                        if (!reader.IsDBNull(1))
                        {
                            currentPeriodEnd = (DateTime?)reader.GetDateTime(1);
                        }
                        bool active = reader.GetBoolean(2);
                        if (active)
                        {
                            backupState = "Backup in progress";
                            backupDate = currentPeriodStart.ToShortDateString() + " " + currentPeriodStart.ToShortTimeString();
                        }
                        else
                        {
                            backupState = "Backup ready";
                            if (currentPeriodEnd == null)
                            {
                                backupDate = currentPeriodStart.ToShortDateString() + " " + currentPeriodStart.ToShortTimeString();
                            }
                            else
                            {
                                backupDate = currentPeriodEnd.Value.ToShortDateString() + " " + currentPeriodEnd.Value.ToShortTimeString();
                            }
                        }
                        reader.Close();
                    }
                    else
                    {
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    string ErrorMessage = "Error looking for backup \n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            } // lock
            return true;
        }

        /// <summary>
        /// Update progress of a backup
        /// </summary>
        public static bool UpdateBackup(int backupNo, DateTime currentStart, DateTime currentEnd, int currentBackupState)
        {
            lock (DBLayer.obj)
            {
                try
                {
                    DBLayer.DatabaseInUse = true;
                    DBLayer.GetConn();
                    string SQL = "update Backups set currentStartTime=?, currentEndTime=?, currentBackupState=? where ID=?";
                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, DBLayer.conn);
                    SQLiteParameter pStart = new SQLiteParameter();
                    pStart.Value = currentStart;
                    UpdateCmd.Parameters.Add(pStart);
                    SQLiteParameter pEnd = new SQLiteParameter();
                    pEnd.Value = currentEnd;
                    UpdateCmd.Parameters.Add(pEnd);
                    SQLiteParameter pBackupState = new SQLiteParameter();
                    pBackupState.Value = currentBackupState;
                    UpdateCmd.Parameters.Add(pBackupState);
                    SQLiteParameter pID = new SQLiteParameter();
                    pID.Value = backupNo;
                    UpdateCmd.Parameters.Add(pID);
                    UpdateCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string ErrorMessage = "Error updating backup \n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            } // lock
            return true;
        }

        /// <summary>
        /// Record the end of a backup
        /// </summary>
        public static bool EndBackup()
        {
            lock (DBLayer.obj)
            {
                try
                {
                    DBLayer.DatabaseInUse = true;
                    DBLayer.GetConn();
                    // TODO: Algorithm to check if time is completed
                    string SQL = "update Backups set EndTime = ?, Active = 0 where Active = 1";
                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, DBLayer.conn);
                    SQLiteParameter pEnd = new SQLiteParameter();
                    pEnd.Value = DateTime.Now;
                    UpdateCmd.Parameters.Add(pEnd);
                    UpdateCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string ErrorMessage = "Error updating backup \n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            } // lock
            return true;
        }

        public static bool GetBackupPeriods(out DateTime PeriodStart, out DateTime PeriodEnd)
        {

            PeriodEnd = DateTime.Today;
            PeriodStart = DateTime.Today;
            lock (DBLayer.obj)
            {
                try
                {
                    DBLayer.DatabaseInUse = true;
                    DBLayer.GetConn();
                    string SQL = "select min(PeriodStartTime), max(PeriodEndTime) from Backups where Active = 0";
                    SQLiteCommand CheckCmd = new SQLiteCommand(SQL, DBLayer.conn);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                        {
                            DateTime tempPeriodStart = reader.GetDateTime(0);
                            DateTime tempPeriodEnd = reader.GetDateTime(1);
                            PeriodStart = tempPeriodStart;
                            PeriodEnd = tempPeriodEnd;
                        }
                    }

                }
                catch (Exception ex)
                {
                    string ErrorMessage = "Error reading Periods \n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            } // lock
            return true;
        }
        #endregion
    }
}
