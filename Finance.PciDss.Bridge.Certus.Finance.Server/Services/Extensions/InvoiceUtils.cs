using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Finance.PciDss.Abstractions;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses;
using Newtonsoft.Json.Linq;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Extensions
{
    public static class InvoiceUtils
    {
        public static string GetRedirectUrlForInvoice(this IPciDssInvoiceModel invoice,
            string mappingString, string defaultRedirectUrl)
        {
            var mapping =
                mappingString
                    .Split("|")
                    .Select(item => item.Split("@"))
                    .Select(item => RedirectUrlSettings.Create(item[0], item[1], item[2]));

            foreach (var redirectUrlSettings in mapping)
            {
                if (invoice.BrandName.Equals(redirectUrlSettings.Brand, StringComparison.OrdinalIgnoreCase) && invoice.AccountId.Contains(redirectUrlSettings.AccountPrefix))
                {
                    return redirectUrlSettings.Link;
                }
            }

            return defaultRedirectUrl;
        }

        private class RedirectUrlSettings
        {
            public string Brand { get; private set; }
            public string AccountPrefix { get; private set; }
            public string Link { get; private set; }

            public static RedirectUrlSettings Create(string brand, string accountPrefix, string link)
            {
                return new RedirectUrlSettings {Brand = brand,AccountPrefix = accountPrefix, Link = link };
            }
        }
        public static string GenerateSha1(this string str)
        {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        //public static string GenerateKeyValue(this JObject obj)
        //{
        //    var buider = new StringBuilder();

        //    foreach (var props in obj.Properties())
        //    {
        //        buider.Append(props.Name);
        //        buider.Append('=');
        //        buider.Append(props.Value);
        //        buider.Append('&');
        //    }

        //    return buider.ToString();
        //}

        //public static string GenerateJsonStringFromKeyValue(this string keyValueString)
        //{
        //    var jObj = new JObject();

        //    foreach (var props in keyValueString.Split("&"))
        //    {
        //        var keyValue = props.Split("=");
        //        var key = keyValue[0];
        //        var value = keyValue[1].Replace("\n", "");

        //        jObj.Add(new JProperty(key, value));
        //    }

        //    return jObj.ToString();
        //}
        public static string EncryptDataInBase64(this string value, string data, string key)
        {
            return EncryptDataInBase64(data, key);
        }

        public static string EncryptDataInBase64(string data, string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(data);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0,
                toEncryptArray.Length);
            string encryptDataInBase64 = Convert.ToBase64String(resultArray);
            return encryptDataInBase64;
        }

        public static string HashDataInBase64(this string value)
        {
            return HashDataInBase64(Encoding.ASCII.GetBytes(value));
        }

        public static string HashDataInBase64(byte[] data)
        {
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(data);
            string hashDataInBase64 = Convert.ToBase64String(hash);
            return hashDataInBase64;
        }
        public static string CreateSignature(string data, string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(data);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0,
                toEncryptArray.Length);
            //Sha256 digest
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(resultArray);
            //base64
            string signature = Convert.ToBase64String(hash);
            return signature;
        }
    }
}
