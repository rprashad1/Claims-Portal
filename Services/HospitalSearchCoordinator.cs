using System;
using System.Threading.Tasks;
using ClaimsPortal.Models;
using Microsoft.Extensions.Logging;

namespace ClaimsPortal.Services
{
    public class HospitalSearchCoordinator
    {
        private readonly ILogger<HospitalSearchCoordinator> _logger;
        private Func<HospitalInfo, Task>? _callback;

        public HospitalSearchCoordinator(ILogger<HospitalSearchCoordinator> logger)
        {
            _logger = logger;
        }

        public void RegisterCallback(Func<HospitalInfo, Task> callback)
        {
            _callback = callback;
            _logger.LogDebug("HospitalSearchCoordinator: RegisterCallback set");
        }

        public void UnregisterCallback()
        {
            _callback = null;
            _logger.LogDebug("HospitalSearchCoordinator: UnregisterCallback");
        }

        public async Task NotifySelection(HospitalInfo info)
        {
            var cb = _callback;
            // Unregister before invoking to avoid reentrancy issues
            _callback = null;
            _logger.LogDebug("HospitalSearchCoordinator: NotifySelection for EntityId={EntityId}", info?.EntityId);
            if (cb != null)
            {
                try
                {
                    await cb(info);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HospitalSearchCoordinator: callback error");
                }
            }
        }
    }
}
