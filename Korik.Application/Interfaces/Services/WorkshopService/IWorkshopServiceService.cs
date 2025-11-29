using Korik.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public interface IWorkshopServiceService : IGenericService<WorkshopService>
    {
        Task<ServiceResult<bool>> IsOriginUniqueForUpdateAsync(int id, CarOrigin? newOrigin);

        Task<ServiceResult<PagedResult<WorkshopService>>> SearchWorkshopsAsync(
                SearchWorkshopsByServiceAndOriginDTO searchDto);
    }
}