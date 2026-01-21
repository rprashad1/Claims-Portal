using System;
using System.Threading.Tasks;
using ClaimsPortal.Models;

namespace ClaimsPortal.Services
{
    public class HospitalSearchCoordinator
    {
        private Func<HospitalInfo, Task>? _callback;

        public void RegisterCallback(Func<HospitalInfo, Task> callback)
        {
            _callback = callback;
            try { Console.WriteLine($"HospitalSearchCoordinator: RegisterCallback set"); } catch { }
        }

        public void UnregisterCallback()
        {
            _callback = null;
            try { Console.WriteLine($"HospitalSearchCoordinator: UnregisterCallback"); } catch { }
        }

        public async Task NotifySelection(HospitalInfo info)
        {
            var cb = _callback;
            // Unregister before invoking to avoid reentrancy issues
            _callback = null;
            try { Console.WriteLine($"HospitalSearchCoordinator: NotifySelection for EntityId={info?.EntityId}"); } catch { }
            if (cb != null)
            {
                try { await cb(info); } catch (Exception ex) { try { Console.WriteLine($"HospitalSearchCoordinator: callback error {ex.Message}"); } catch { } }
            }
        }
    }
}
