using System.Collections.Generic;

namespace OneSms.Contracts.V1.Responses
{
    public class FileUploadFailedResponse
    {
        public IEnumerable<string> Errors { get; set; }
    }
}
