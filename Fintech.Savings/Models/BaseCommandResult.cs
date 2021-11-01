using System;
using System.Text.Json.Serialization;

namespace App.Models
{

    public class ResponseMessage
    {
        [JsonIgnore]
        public bool IsSuccessful { get; internal set; }
        public string Message { get; internal set; }
    }


    public class GenericMessage<T> : ResponseMessage
    {

        public T Result { get; set; }
        public string ResponseCode { get;  set; }
        public string StatusCode { get; internal set; }
    }


    public class BaseCommandResult
    {

        public bool IsSuccessful { get; internal set; }
        public string Message { get; internal set; }
        public string StatusCode { get; internal set; }
    }


    public class BaseReponse<T>
    {
        public bool IsSuccessful { get; set; }
        public T Result { get; set; }
        public string ProviderUsed { get; set; }
    }
    

}
