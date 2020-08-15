using System;
using System.Collections.Generic;
using Utils;
using Oracle.ManagedDataAccess.Client;

namespace EarthFusion
{
    class SessionHelpers
    {
        public static bool RequestVerificationCode(string emailAddress)
        {
            string verificationCodeSentString = RedisHelpers.GetString("earth_fusion_" + emailAddress + "_verification_code_sent");
            if (verificationCodeSentString != null)
            {
                // already has an code.
                return false;
            }
            Random random = new System.Random();
            // 6 digit
            int verificationCode = random.Next(100000, 999999);
            string verificationCodeString = verificationCode.ToString();
            SendgridHelpers.SendVerificationCodeTask(emailAddress, "User", verificationCodeString);
            // set redis
            RedisHelpers.SetString("earth_fusion_" + emailAddress + "_verification_code", verificationCodeString);
            RedisHelpers.SetString("earth_fusion_" + emailAddress + "_verification_code_sent", "1");
            // set verification code timeout: 600s (10 min)
            RedisHelpers.SetKeyExpireTime("earth_fusion_" + emailAddress + "_verification_code", 600);
            // set timeout for when can user request another verification code: 60 (1 min)
            RedisHelpers.SetKeyExpireTime("earth_fusion_" + emailAddress + "_verification_code_sent", 60);
            return true;
        }

        public static bool CompareVerificationCode(string emailAddress, int verificationCodeToCheck)
        {
            string verificationCodeString = RedisHelpers.GetString("earth_fusion_" + emailAddress + "_verification_code");
            if (verificationCodeString == verificationCodeToCheck.ToString())
            {
                // make verification code related keys expire
                RedisHelpers.SetKeyExpireTime("earth_fusion_" + emailAddress + "_verification_code", 1);
                RedisHelpers.SetKeyExpireTime("earth_fusion_" + emailAddress + "_verification_code_sent", 1);
                return true;
            }
            return false;
        }

        public static bool Register(string emailAddress, string nickname, string password)
        {
            // CREATE TABLE earthfusion_users(
            // user_id NUMBER,
            // user_name VARCHAR2(50),
            // user_email VARCHAR2(50),
            // user_password_hashed VARCHAR2(66),
            // user_status VARCHAR2(50),
            // user_role VARCHAR2(50),
            // PRIMARY KEY(user_id)
            // );

            // table structure:
            // user_id: uuid. This is the unique identification of an user.
            // user_name: username/nickname
            // user_email: user's email
            // user_password_hashed: hashed password. SHA256 only.
            // user_status: is the user enabled? "enabled"/"disabled"
            // user_role: administrator/user

            // TODO: generate uuid

            // TODO: insert into database
            // insert into earthfusion_users
            // (user_id, user_name, user_email, USER_PASSWORD_HASHED, USER_STATUS, USER_ROLE)
            // values
            // (1, 'marshmallow', 'marshmallow@anzupop.com', '5a9fee2cb0e686d7d9022dfc72ccb160d533c668059d1acfcf5da53d517f2d46', 'enabled', 'administrator');

            return true;
        }

        public static UserInformation Login(string username, string password)
        {
            string oracleUsername = Environment.GetEnvironmentVariable("EARTH_FUSION_SPATIAL_DB_USERNAME");
            string oraclePassword = Environment.GetEnvironmentVariable("EARTH_FUSION_SPATIAL_DB_PASSWORD");
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleUsername, oraclePassword, false);
            // Get user information
            List<UserInformation> selectedResult = GetUserInformation(conn, username);
            if (selectedResult.Count < 1)
            {
                Logging.Warning("EarthFusion.SessionHelpers.Login", "No matching username in raw data");
                return null;
            }
            string userPasswordHashed = GenericHelpers.ComputeSha256Hash(password);
            Logging.Info("EarthFusion.SessionHelpers.Login", "request has a hashed password of " + userPasswordHashed);
            foreach (UserInformation userInformation in selectedResult)
            {
                Logging.Info("EarthFusion.SessionHelpers.Login", "comparing user with uuid " + userInformation.userId.ToString());
                Logging.Info("EarthFusion.SessionHelpers.Login", "This user has a hashed password of " + userInformation.userPasswordHashed);
                if (userPasswordHashed == userInformation.userPasswordHashed)
                {
                    Logging.Info("EarthFusion.SessionHelpers.Login", "uuid " + userInformation.userId.ToString() + " seems good!");
                    return userInformation;
                }
            }
            return null;
        }

        public static List<UserInformation> GetUserInformation(OracleConnection conn, string username)
        {
            List<UserInformation> result = new List<UserInformation>();
            string queryString = "select * from spatial_admin.earthfusion_users where USER_NAME = '" + username + "'";
            Logging.Info("EarthFusion.SessionHelpers.GetUserInformation", "Constructed query: " + queryString);
            OracleCommand command = new OracleCommand(queryString, conn);
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    UserInformation temp = new UserInformation();
                    temp.userId = reader.GetInt32(0);
                    temp.userName = reader.GetString(1);
                    temp.emailAddress = reader.GetString(2);
                    temp.userPasswordHashed = reader.GetString(3);
                    temp.accountStatus = reader.GetString(4);
                    temp.role = reader.GetString(5);
                    result.Add(temp);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return result;
        }

        public static UserInformation ValidateSession(OracleConnection conn, string sessionId)
        {
            List<UserInformation> result = new List<UserInformation>();
            string userId = RedisHelpers.GetString("EARTH_FUSION_SESSION_" + sessionId.ToUpper());
            if (userId == null)
            {
                return null;
            }
            string queryString = "select * from spatial_admin.earthfusion_users where USER_ID = '" + userId + "'";
            Logging.Info("EarthFusion.SessionHelpers.GetUserInformation", "Constructed query: " + queryString);
            OracleCommand command = new OracleCommand(queryString, conn);
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    UserInformation temp = new UserInformation();
                    temp.userId = reader.GetInt32(0);
                    temp.userName = reader.GetString(1);
                    temp.emailAddress = reader.GetString(2);
                    temp.userPasswordHashed = reader.GetString(3);
                    temp.accountStatus = reader.GetString(4);
                    temp.role = reader.GetString(5);
                    result.Add(temp);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            if (result.Count < 1)
            {
                return null;
            }
            else
            {
                return result[0];
            }
        }
    }
}