using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class NewClientResponse
    {
        public string ClientName { get; set; }
        public string Status { get; set; }
        public string ClientCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ClientKey { get; set; }
    }
}
