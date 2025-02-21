using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Final.ViewComponents
{
    public class MqttLogsViewComponent : ViewComponent
    {
        public MqttLogsViewComponent()
        {
        }

        // Accepts an optional companyId parameter.
        public Task<IViewComponentResult> InvokeAsync(Guid? companyId = null)
        {
            var id = companyId ?? Guid.Empty;
            
            // Optionally, provide an initial log message.
            var logs = new List<string>
            {
                $"[{DateTime.Now:HH:mm:ss}] [Info] Company {id}: Initial log - waiting for updates..."
            };

            // Pass the company ID (or group name, e.g., company base topic) to the view.
            ViewData["CompanyId"] = id; 
            return Task.FromResult<IViewComponentResult>(View(logs));
        }
    }
}
