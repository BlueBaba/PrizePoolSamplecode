using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class TopupDepositAcctOpeningResp
    {
        public string responseCode { get; set; }
        public string accountNumber { get; set; }
        public string description { get; set; }
        public string errorResponse { get; set; }

    }

    public class DepositAccountTopupResp
    {
        public string ResponseCode { get; set; }
        public string Description { get; set; }
        public string ErrorResponse { get; set; }
        public string AccountNumber { get; set; }
        public string Oldbalance { get; set; }
        public string Newbalance { get; set; }
    }

    public class TermDepositAccountClosureResp
    {
        public string ResponseCode { get; set; }
        public string Description { get; set; }
        public string ErrorResponse { get; set; }
        public string AccoutNumber { get; set; }
        public string Oldbalance { get; set; }
        public string Newbalance { get; set; }
    }

}