using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace App.Entities
{
    public class IntraHeader
    {
        public string httpVerb; 
        public string percentEncodedUrl;
        public string clientId;
        public string clientSecret;
        public string reference;
        public string signatureConValue;
        public string auth;
        public string HBAuthorization;
        public string hash;
        public string signature;
        public string baseurl;
        public string baseurltoken;
        public string uniqueKey;

        public IntraHeader(IConfiguration config)
        {
            clientId = config.GetSection("intraclientId").Value;
            clientSecret = config.GetSection("intraclientSecret").Value;
            auth = Base64Encode(clientId);
            HBAuthorization = "HeritageAuth " + auth;
            baseurl = config.GetSection("intrabankbaseurl").Value;
            baseurltoken = config.GetSection("intrabanktokenbaseurl").Value;
            uniqueKey = config.GetSection("intrabankTranUniqueKey").Value;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string TextToHash(string text)
        {
            var sh = SHA1.Create();
            var hash = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] b = sh.ComputeHash(bytes);
            foreach (byte a in b)
            {
                var h = a.ToString("x2");
                hash.Append(h);
            }
            return hash.ToString();
        }


    }
}
