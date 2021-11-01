using App.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Interface
{
    public interface IIntraBankDrCr
    {
        Task<string> APICall(IntraHeader intraHeader, string url, object data, string qsparam, string token);
        Task<string> APICallAuth(IntraHeader intraHeader, string url, List<KeyValuePair<string, string>> keyValues);
    }
}
