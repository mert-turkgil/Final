using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Final.Models
{
    public class MqttLog
    {
        #nullable disable
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }

}