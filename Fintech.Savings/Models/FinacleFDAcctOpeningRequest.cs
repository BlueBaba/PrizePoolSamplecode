using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class FinacleFDAcctOpeningRequest
    {
        public string cifId { get; set; }
        public string bvn { get; set; }
        public string schemeType { get; set; }
        public string schemeCode { get; set; }
        public string glSubHeadCode { get; set; }
        public string depositAmount { get; set; }
        public string depositPeriodDays { get; set; }
        public string accountPrefinterest { get; set; }
        public string debitAccountNumber { get; set; }
        public string repaymentAccount { get; set; }
        public string introducerCode { get; set; }
        public string autoCloseOnMaturityFlg { get; set; }
        public string autoRenewalFlag { get; set; }
        public string uniqueKey { get; set; }
        public string appId { get; set; }
    }
}
