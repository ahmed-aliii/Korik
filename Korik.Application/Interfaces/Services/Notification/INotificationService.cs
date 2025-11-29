using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public interface INotificationService
    {
        Task NotifyWorkshopBookingRequestAsync(int workshopId, object payload);

        Task NotifyCarOwnerBookingStatusAsync(int carOwnerId, object payload);
    }
}
