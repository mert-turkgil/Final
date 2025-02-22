using Final.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Final.ViewComponents
{
    public class MqttLogsViewComponent : ViewComponent
    {
        private readonly IMqttLogService _mqttLogService;

        public MqttLogsViewComponent(IMqttLogService mqttLogService)
        {
            _mqttLogService = mqttLogService;
        }

        public Task<IViewComponentResult> InvokeAsync(Guid? companyId = null)
        {
            var id = companyId ?? Guid.Empty;
            var logs = _mqttLogService.GetLogs(id);
            ViewData["CompanyId"] = id; 
            return Task.FromResult<IViewComponentResult>(View(logs));
        }
    }
}
