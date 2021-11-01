using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class FinacleTopupAcctOpeningRequest
    {
        public string debitAccountNumber { get; set; }
        public string depositAmount { get; set; }
        public string depositPeriodMonths { get; set; }
        public string depositPeriodDays { get; set; }
        public string customerId { get; set; }
        public string openEffectiveDate { get; set; }
        public string depositRate { get; set; }
        public string accountManagerId { get; set; }
        public string solId { get; set; }
        public string IntroducerId { get; set; }
        public string AppId { get; set; }
    }

    public class FinacleDepositAccountTopupRequest
    {
        public string debitAccountNumber { get; set; }
        public string transactionAmount { get; set; }
        public string depositAccount { get; set; }
    }

    public class FinacleTermDepositAccountClosureRequest
    {
        public string repaymentAccountNumber { get; set; }
        public string depositAccount { get; set; }
        public string withdrawalAmount { get; set; }
        public string closureType { get; set; }
        public string currency { get; set; }
    }

}