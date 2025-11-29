using Korik.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public interface IWorkshopServiceRepository : IGenericRepository<WorkshopService>
    {
        Task<PagedResult<WorkshopService>> SearchWorkshopsAsync(
            int serviceId,
            CarOrigin? origin,
            string? city,
            decimal? latitude,
            decimal? longitude,
            DateTime appointmentDate,
            int pageNumber,
            int pageSize);
    }
}