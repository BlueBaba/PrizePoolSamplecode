using System;
namespace App.Models
{
    public class ErrorResponseModel
    {
        public ErrorResponseModel()
        {
        }

        public object Message { get; internal set; }
        public string RequestId { get; internal set; }
        public string TraceMessages { get; internal set; }
    }
}
