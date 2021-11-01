using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class BalanceInquiryResponse
    {
        public string transactionReference { get; set; }
        public string transactionId { get; set; }
        public string balance { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string transactionType { get; set; }
        public string terminalType { get; set; }
        public string availableBalance { get; set; }
        public string ledgerBalance { get; set; }
        public string iso { get; set; }
    }
}
