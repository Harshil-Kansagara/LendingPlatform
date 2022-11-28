
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.PayPal
{
    public class InoviceResponsesInLoopAC
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<JObject> Items { get; set; }
    }
}
