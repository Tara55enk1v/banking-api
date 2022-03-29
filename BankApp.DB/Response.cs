using System;

namespace BankApp.DB
{
    public class Response
    {
        public string ResponseCode { get; set; }
        public string RequestId => $"{Guid.NewGuid().ToString()}";
        public string ResponseMessage { get; set; }
        public object Data { get; set; }
    }
}
