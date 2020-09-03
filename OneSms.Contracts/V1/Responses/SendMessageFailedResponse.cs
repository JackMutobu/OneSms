using System.Collections.Generic;

namespace OneSms.Contracts.V1.Responses
{
    public class SendMessageFailedResponse
    {
        public SendMessageFailedResponse()
        {
            Errors = new List<string>();
        }
        public IEnumerable<string> Errors { get; set; }
    }
}
