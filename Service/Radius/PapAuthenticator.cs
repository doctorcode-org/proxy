using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace DoctorProxy.Service.Radius
{
    public static class PapAuthenticator
    {
        public static bool Authenticate(byte[] sharedSecret, byte[] rqAuthenticator, byte[] requestPassword, string clearPassword)
        {
            // PAPAuthenticator will encrypt the dbPassword value and compare it to the rpPassword value

            int passwordBuffLength;
            var authData = new byte[16];
            var currentPasswordBlock = new byte[16];

            // reduce dbPassword to byte array, pad to 16 byte boundary
            int passwordByteLength = passwordBuffLength = Encoding.ASCII.GetByteCount(clearPassword);

            if ((passwordByteLength % 16) != 0)
                passwordBuffLength += 16 - (passwordByteLength % 16);

            var passwordBuffer = new byte[passwordBuffLength];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(clearPassword), 0, passwordBuffer, 0, passwordByteLength);
            int passwordBlocks = passwordBuffLength / 16;

            // for the first round, init authData to request authenticator value
            Buffer.BlockCopy(rqAuthenticator, 0, authData, 0, 16);

            for (int i = 0; i < passwordBlocks; i++)
            {
                // copy the password block to cipherData
                Buffer.BlockCopy(passwordBuffer, i * 16, currentPasswordBlock, 0, 16);
                // encrypt in place
                PAPEncryptBlock(sharedSecret, authData, currentPasswordBlock);


                // test encrypted password against the request password
                for (int j = 0; j < 16; j++)
                {
                    if (currentPasswordBlock[j] != requestPassword[(i * 16) + j])
                        return false;
                }

                // copy cipherData to authData for next round
                Buffer.BlockCopy(currentPasswordBlock, 0, authData, 0, 16);
            }

            return true;
        }

        public static void PAPEncryptBlock(byte[] sharedSecret, byte[] authData, byte[] cipherData)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var baTemp = new byte[sharedSecret.Length + 16];
            Buffer.BlockCopy(sharedSecret, 0, baTemp, 0, sharedSecret.Length);
            Buffer.BlockCopy(authData, 0, baTemp, sharedSecret.Length, 16);

            var biData = new BitArray(cipherData);
            biData.Xor(new BitArray(md5.ComputeHash(baTemp))).CopyTo(cipherData, 0);
        }

        public static string Reverse(byte[] sharedSecret, byte[] rqAuthenticator, byte[] requestPassword)
        {
            var passwordBuffLength = requestPassword.Length;
            var authData = new byte[16];
            var currentPasswordBlock = new byte[16];

            var outPasswordBuffer = new byte[requestPassword.Length];
            var passwordBlocks = passwordBuffLength / 16;

            Buffer.BlockCopy(rqAuthenticator, 0, authData, 0, 16);

            for (var i = 0; i < passwordBlocks; i++)
            {
                Buffer.BlockCopy(requestPassword, i * 16, currentPasswordBlock, 0, 16);

                PapAuthenticator.PAPEncryptBlock(sharedSecret, authData, currentPasswordBlock);

                Buffer.BlockCopy(currentPasswordBlock, i * 16, outPasswordBuffer, 0, 16);

                Buffer.BlockCopy(currentPasswordBlock, 0, authData, 0, 16);
            }

            var outPassword = Encoding.ASCII.GetString(outPasswordBuffer).TrimEnd('\0');
            return outPassword;
        }

        public static byte[] GeneratePAPPassword(string ClearTextPW, string SharedSecret, byte[] RequestAuthenticator)
        {
            /* Generates the Encrypted Password (c) for the data paket 
             * 1.) Split the userpassword (P) in 128 bit / 16 byte blocks (p1 .. pn)
             *     if the last block is not devidable by 16, pad it with "0"
             * 2.) XOR these Blocks with the MD5 of the SharedSecret (S) and the Request Authenticator (RA)
             *     c1 = p1 XOR MD5(S + RA)
             *     c2 = p2 XOR MD5(S + c1)
             *     cn = pn XOR MD5(S + cn-1)
             *     
             *     c  = c1 + c2 + ... + cn (Concat)
             * 
             */

            // Initially the MD5 is taken over the Shared Secret and the Request Authenticator
            string pKeyRA = SharedSecret + Encoding.Default.GetString(RequestAuthenticator);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] pMD5Sum = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(pKeyRA));
            string pMD5String = ToHexString(pMD5Sum);

            // Determine how many rounds are needed for authentication
            int pCrounds = ClearTextPW.Length / 16;
            if (ClearTextPW.Length % 16 != 0) { pCrounds++; }


            byte[] Result = new byte[pCrounds * 16];
            for (int j = 0; j < pCrounds; j++)
            {
                int pm;
                int pp;

                //Split the password in 16byte chunks
                string pRoundPW = "";
                if (ClearTextPW.Length < (j + 1) * 16) { pRoundPW = ClearTextPW.Substring(j * 16, ClearTextPW.Length - j * 16); }
                else { pRoundPW = ClearTextPW.Substring(j * 16, 16); }

                for (int i = 0; i <= 15; i++)
                {
                    if (2 * i > pMD5String.Length) { pm = 0; } else { pm = System.Convert.ToInt32(pMD5String.Substring(2 * i, 2), 16); }
                    if (i >= pRoundPW.Length) { pp = 0; } else { pp = (int)pRoundPW[i]; }
                    int pc = pm ^ pp;
                    Result[(j * 16) + i] = (byte)pc;
                } //for (int i = 0; i <= 15; i++)


                //Determine the next MD5 Sum MD5(S + cn-1)
                byte[] pCN1 = new byte[16];
                Array.Copy(Result, j * 16, pCN1, 0, 16);
                string pKeyCN1 = SharedSecret + Encoding.Default.GetString(pCN1);
                md5 = new MD5CryptoServiceProvider();
                pMD5Sum = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(pKeyCN1));
                pMD5String = ToHexString(pMD5Sum);
            }

            return Result;
        }

        public static string ToHexString(byte[] bytes)
        {

            char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7',
                                 '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};


            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

    }
}
