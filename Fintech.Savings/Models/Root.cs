using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class CustProfileResp
    {
        public CustProfileProp custprof { get; set; }
        public bool isSuccess { get; set; }
        public string error { get; set; }
        public object timestamp { get; set; }
        public object responseCode { get; set; }
        public bool isFailure { get; set; }
    }

    public class CustProfileProp
    {
        public string salutation { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string bvn { get; set; }
        public object nickName { get; set; }
        public object address { get; set; }
        public string accountNumber { get; set; }
        public object signature { get; set; }
        public object passport { get; set; }
        public string customerUniqueRefNumber { get; set; }
        public string fullName { get; set; }
        public object rmCode { get; set; }
        public string customerCif { get; set; }
    }



    public class TermDepositRate
    {
        public string effectiveRate { get; set; }
        public string normalRate { get; set; }
        public string penalRate { get; set; }
        public string responseCode { get; set; }
        public string message { get; set; }
        public string errorResponse { get; set; }

    }

}
