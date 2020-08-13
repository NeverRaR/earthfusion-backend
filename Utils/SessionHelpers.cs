using System;

namespace Utils
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
    }
}