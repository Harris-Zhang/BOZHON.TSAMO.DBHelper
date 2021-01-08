using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Utils
{
    public class ValidLicense
    {
        private readonly static string licenseKey = System.IO.File.ReadAllText("TSAMO.license");
        public static List<string> GetMacAddress(string separator = "-")
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            var macAddress = new List<string>();

            foreach (NetworkInterface adapter in nics.
                Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback
                && c.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                && c.Name.IndexOf("VM") < 0
                && c.Description.IndexOf("Virtual") < 0))
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                var unicastAddresses = properties.UnicastAddresses;
                if (unicastAddresses.Any(temp => temp.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    var address = adapter.GetPhysicalAddress();
                    if (string.IsNullOrEmpty(separator))
                    {
                        macAddress.Add(address.ToString());
                    }
                    else
                    {
                        string tmp = "";
                        byte[] bytes = address.GetAddressBytes();

                        for (int i = 0; i < bytes.Length; i++)
                        {
                            tmp += bytes[i].ToString("X2");

                            if (i != bytes.Length - 1)
                            {
                                tmp += separator;
                            }
                        }
                        macAddress.Add(tmp);
                    }
                }
            }
            return macAddress;
        }

        public static string AESDecrypt(string ciphertext, string password)
        {
            Aes aes = new AesCryptoServiceProvider();
            char[] inArray = ciphertext.ToCharArray();
            byte[] buffer = Convert.FromBase64CharArray(inArray, 0, inArray.Length);
            byte[] salt = Encoding.UTF8.GetBytes("TSAMO_MES");
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, salt);
            aes = new AesCryptoServiceProvider
            {
                KeySize = aes.LegalKeySizes[0].MaxSize,
                BlockSize = aes.LegalBlockSizes[0].MaxSize,
                Key = bytes.GetBytes(aes.KeySize / 8),
                IV = bytes.GetBytes(aes.BlockSize / 8)
            };
            byte[] buffer3 = new byte[buffer.Length];
            int count = 0;
            using (ICryptoTransform transform = aes.CreateDecryptor())
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    using (CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read))
                    {
                        count = stream2.Read(buffer3, 0, buffer3.Length);
                        stream2.Close();
                    }
                    stream.Close();
                }
            }
            return Encoding.UTF8.GetString(buffer3, 0, count);
        }

        public static string Decrypt(string ciphertext)
        {
            int length = CharToIntCipher(ciphertext[ciphertext.Length - 1]);
            ciphertext = ciphertext.Substring(0, ciphertext.Length - 1);
            length--;
            char c = ciphertext[length];
            ciphertext = ciphertext.Substring(0, length) + ciphertext.Substring(length + 1);
            for (int i = 0; i < 0x38; i++)
            {
                int num3 = CharToIntCipher(c);
                length = ((length - (num3 % ciphertext.Length)) > 0) ? (length - (num3 % ciphertext.Length)) : ((length + ciphertext.Length) - (num3 % ciphertext.Length));
                length--;
                c = ciphertext[length];
                ciphertext = ciphertext.Substring(0, length) + ciphertext.Substring(length + 1);
            }
            ciphertext = ciphertext.Substring(1, ciphertext.Length - 1);
            return ciphertext;
        }
        private static int CharToIntCipher(char c)
        {
            return (c - '$');
        }
        private static byte[] EncodingGetBytes(string source)
        {
            return Encoding.Unicode.GetBytes(source);
        }

        public static string ComputeHash(string source)
        {
            return ComputeHash(EncodingGetBytes(source));
        }
        public static string ComputeHash(byte[] bytes)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        public static bool IsValidLicense( out string message)
        { 
            message = "验证成功";
            bool result = false;
            try
            {
                
                if (licenseKey.Length < 284)
                {
                    message = "License 有误，请联系相关人员确认License 是否正确";
                    return result;
                }
                string type = licenseKey.Substring(0, 0x18);
                string machId = licenseKey.Substring(0x18, 0x80);
                string expiredDate = ValidLicense.AESDecrypt(licenseKey.Substring(0x104, 0x18), "TSAMO_MES");

                string macAdd = ValidLicense.Decrypt(ValidLicense.AESDecrypt(machId, "TSAMO_MES"));

                List<string> macIds = ValidLicense.GetMacAddress();
                string d = macIds.Where(p => ValidLicense.ComputeHash(p) == macAdd).FirstOrDefault();
                if (string.IsNullOrEmpty(d))
                { 
                    message = "License 有误，请联系相关人员确认License 是否正确";
                    return result;
                }
                if (VerifiExpiredDate(expiredDate) < DateTime.Now)
                { 
                    message = "License 已过期，请联系相关人员确认License是否正确";
                    return result;
                }
                result = true;
            }
            catch
            {
                result = false;
                message = "License 有误，请联系相关人员确认License 是否正确";
            }
            return result;

        }
        /// <summary>
        /// 验证日期
        /// </summary>
        /// <param name="str">待转换的object</param>
        /// <param name="defVal">缺省值(转换不成功)</param> 
        /// <returns>转换后的Int类型结果</returns>
        private static DateTime VerifiExpiredDate(object obj)
        {
            DateTime result = DateTime.Now.AddDays(-1);
            try
            {
                if (obj != null)
                    result = Convert.ToDateTime(obj);
            }
            catch
            {

            }
            return result;
        }
    }
}
