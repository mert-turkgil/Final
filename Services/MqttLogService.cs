using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Final.Models;

namespace Final.Services
{
    public class MqttLogService : IMqttLogService
    {
        // Store logs per company.
        private readonly ConcurrentDictionary<Guid, List<MqttLog>> _logs = new ConcurrentDictionary<Guid, List<MqttLog>>();

        // Computes a color based on the company name.
        private string GetColorForCompany(string companyName)
        {
            int hash = companyName.GetHashCode();
            byte r = (byte)((hash >> 16) & 0xFF);
            byte g = (byte)((hash >> 8) & 0xFF);
            byte b = (byte)(hash & 0xFF);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// Adds a log entry for the given company.
        /// </summary>
        public void AddLog(Guid companyId, string companyName, string message)
        {
            var now = DateTime.Now;
            var color = GetColorForCompany(companyName);
            // Format the log message with HTML color markup.
            var formattedMessage = $"[>>{now:HH:mm:ss}] [Info] Company <span style=\"color: {color};\">{companyName}</span>: {message}";
            var newLog = new MqttLog { Timestamp = now, Message = formattedMessage };

            _logs.AddOrUpdate(companyId,
                key => new List<MqttLog> { newLog },
                (key, existingLogs) =>
                {
                    if (existingLogs.Any())
                    {
                        var lastLog = existingLogs.Last();
                        // Avoid duplicate messages within 1 minute.
                        if (lastLog.Message == formattedMessage && (now - lastLog.Timestamp).TotalMinutes < 1)
                        {
                            return existingLogs;
                        }
                    }
                    existingLogs.Add(newLog);
                    return existingLogs;
                });
        }

        public List<MqttLog> GetLogs(Guid companyId)
        {
            return _logs.TryGetValue(companyId, out var logs)
                ? logs
                : new List<MqttLog>();
        }

        public List<MqttLog> GetAllLogs()
        {
            return _logs.Values.SelectMany(logList => logList)
                      .OrderBy(log => log.Timestamp)
                      .ToList();
        }
    }
}
