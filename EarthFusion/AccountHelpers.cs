using System;
using System.Collections.Generic;
using Utils;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace EarthFusion
{
    class AccountHelpers
    {
        public static AltAccountResult AltAccountStatus(string sessionId, int userId, string operation)
        {
            AltAccountResult result = new AltAccountResult();
            string altString;
            if (operation == "DisableAccount")
            {
                altString = "disabled";
            }
            else if (operation == "EnableAccount")
            {
                altString = "enabled";
            }
            else
            {
                result.BoolResult = false;
                result.Operation = "AltAccountStatus";
                result.Result = "Nothing done.";
                result.Reason = "You provided an unimplemented operation.";
                result.Message = "Check your input.";
                return result;
            }
            result.Operation = operation;
            string oracleUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_USERNAME"];
            string oraclePassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleUsername, oraclePassword, false);
            UserInformation currentUser = SessionHelpers.ValidateSession(conn, sessionId);
            if (currentUser.role != "administrator")
            {
                result.BoolResult = false;
                result.Result = "Account not altered";
                result.Reason = "You are not the administrator.";
                result.Message = "Log into the administrator's account and try again.";
            }
            else if (!OracleHelpers.IsRowExistInColumnInTableName(conn, userId.ToString(), "SPATIAL_ADMIN.EARTHFUSION_USERS", "user_id"))
            {
                result.BoolResult = false;
                result.Result = "Account not altered";
                result.Reason = "Provided userId not found";
                result.Message = "Check your userId and try again.";
            }
            else
            {
                string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
                string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
                OracleConnection spatialAdminConn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
                spatialAdminConn.Open();

                OracleTransaction transaction;
                // update EARTHFUSION_USERS SET USER_STATUS = 'disabled' where USER_ID = 74725
                OracleCommand command = spatialAdminConn.CreateCommand();
                string updateString = "update SPATIAL_ADMIN.EARTHFUSION_USERS SET USER_STATUS = '" + altString + "' where USER_ID =" + userId;
                Logging.Info("SessionHelpers.AltAccountStatus", "Constructed update: " + updateString);
                transaction = spatialAdminConn.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Transaction = transaction;
                try
                {
                    command.CommandText = updateString;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    result.BoolResult = true;
                    result.Result = "Account " + altString;
                    result.Reason = "";
                    result.Message = "Yeah.";
                    spatialAdminConn.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logging.Warning("SessionHelpers.AltAccountStatus", ex.ToString());
                    result.BoolResult = false;
                    result.Result = "Account not altered";
                    result.Reason = "Exception occured. Check server log.";
                    result.Message = "If you are reading this message, consult your system admin immediately.";
                }
            }
            return result;
        }

        public static AltAccountResult AltAccountPassword(string sessionId, int userId, string password)
        {
            AltAccountResult result = new AltAccountResult();
            string oracleUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_USERNAME"];
            string oraclePassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleUsername, oraclePassword, false);
            UserInformation currentUser = SessionHelpers.ValidateSession(conn, sessionId);
            if ((currentUser.role != "administrator") && (currentUser.userId != userId))
            {
                result.BoolResult = false;
                result.Result = "Account not altered";
                result.Reason = "You are not the administrator, or you are trying to alter an account which is not yourself.";
                result.Message = "Log into the administrator's account or your own account and try again.";
            }
            else if (!OracleHelpers.IsRowExistInColumnInTableName(conn, userId.ToString(), "SPATIAL_ADMIN.EARTHFUSION_USERS", "user_id"))
            {
                result.BoolResult = false;
                result.Result = "Account not altered";
                result.Reason = "Provided userId not found";
                result.Message = "Check your userId and try again.";
            }
            else
            {
                string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
                string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
                OracleConnection spatialAdminConn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
                spatialAdminConn.Open();

                OracleTransaction transaction;
                // update EARTHFUSION_USERS SET USER_STATUS = 'disabled' where USER_ID = 74725
                OracleCommand command = spatialAdminConn.CreateCommand();
                string hashedUserPassword = GenericHelpers.ComputeSha256Hash(password);
                string updateString = "update SPATIAL_ADMIN.EARTHFUSION_USERS SET USER_PASSWORD_HASHED = '" + hashedUserPassword + "' where USER_ID =" + userId;
                Logging.Info("SessionHelpers.AltAccountStatus", "Constructed update: " + updateString);
                transaction = spatialAdminConn.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Transaction = transaction;
                try
                {
                    command.CommandText = updateString;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    result.BoolResult = true;
                    result.Result = "Account password changed";
                    result.Reason = "";
                    result.Message = "Yeah.";
                    spatialAdminConn.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logging.Warning("SessionHelpers.AltAccountPassword", ex.ToString());
                    result.BoolResult = false;
                    result.Result = "Account not altered";
                    result.Reason = "Exception occured. Check server log.";
                    result.Message = "If you are reading this message, consult your system admin immediately.";
                }
            }
            return result;
        }
    }
}