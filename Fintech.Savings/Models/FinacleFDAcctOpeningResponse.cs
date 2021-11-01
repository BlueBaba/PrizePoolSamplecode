using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class FinacleFDAcctOpeningResponse
    {
        public string accoutNumber { get; set; }
        public string cifId { get; set; }
        public string responseCode { get; set; }
        public string message { get; set; }
        public string errorResponse { get; set; }
    }
}
