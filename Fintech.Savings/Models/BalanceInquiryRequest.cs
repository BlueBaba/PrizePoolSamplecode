using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class BalanceInquiryRequest
    {
        public string accountNumber { get; set; }
        public string uniqueKey { get; set; }
    }
}
