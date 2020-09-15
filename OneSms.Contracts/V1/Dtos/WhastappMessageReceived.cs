using System.Collections.Generic;

namespace OneSms.Contracts.V1.Dtos
{
    public class WhastappMessageReceived:BaseMessageReceived
    {
       public List<string> ImageLinks { get; set; }
    }
}
