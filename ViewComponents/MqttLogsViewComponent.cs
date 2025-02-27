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
            var logs = (companyId.HasValue && companyId.Value != Guid.Empty)
                ? _mqttLogService.GetLogs(companyId.Value)
                : _mqttLogService.GetAllLogs();

            return Task.FromResult<IViewComponentResult>(View(logs));
        }
    }
}
