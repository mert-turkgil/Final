using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Final.Models;

namespace Final.Services
{
    public interface IMqttLogService
    {
        void AddLog(Guid companyId, string companyName, string message);
        List<MqttLog> GetLogs(Guid companyId);
        List<MqttLog> GetAllLogs();
    }
}