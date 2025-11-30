using Korik.Application;
using Korik.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Infrastructure
{
    public class WorkshopServiceRepository : GenericRepository<WorkshopService>, IWorkshopServiceRepository
    {
        private readonly Korik context;

        #region Dependence Injection

        public WorkshopServiceRepository(Korik context) : base(context)
        {
            this.context = context;
        }

        #endregion Dependence Injection

        public async Task<PagedResult<WorkshopService>> SearchWorkshopsAsync(
                int serviceId,
                CarOrigin? origin,
                string? city,
                decimal? latitude,
                decimal? longitude,
                DateTime appointmentDate,
                int pageNumber,
                int pageSize
            )
        {
            var dayOfWeek = appointmentDate.DayOfWeek;
            var appointmentTime = TimeOnly.FromDateTime(appointmentDate);

            // Base query with minimal includes (only what we need)
            var baseQuery = context.Set<WorkshopService>()
                .AsNoTracking()
                .Where(ws => ws.ServiceId == serviceId &&
                             ws.WorkShopProfile.VerificationStatus == VerificationStatus.Verified);

            // Apply filters
            if (origin.HasValue)
            {
                baseQuery = baseQuery.Where(ws => ws.Origin == origin.Value);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                baseQuery = baseQuery.Where(ws => ws.WorkShopProfile.City == city);
            }

            // Count before loading navigation properties
            var totalRecords = await baseQuery.CountAsync();

            if (totalRecords == 0)
            {
                return new PagedResult<WorkshopService>
                {
                    Items = new List<WorkshopService>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = 0
                };
            }

            // Build final query with ordering and pagination
            IQueryable<WorkshopService> finalQuery;

            if (latitude.HasValue && longitude.HasValue)
            {
                var lat1 = (double)latitude.Value;
                var lon1 = (double)longitude.Value;

                // Use Haversine formula for accurate distance
                finalQuery = baseQuery
                    .Select(ws => new
                    {
                        WorkshopService = ws,
                        // Haversine distance calculation (in kilometers)
                        Distance = 6371 * 2 * Math.Asin(Math.Sqrt(
                            Math.Pow(Math.Sin((((double)ws.WorkShopProfile.Latitude - lat1) * Math.PI / 180) / 2), 2) +
                            Math.Cos(lat1 * Math.PI / 180) *
                            Math.Cos((double)ws.WorkShopProfile.Latitude * Math.PI / 180) *
                            Math.Pow(Math.Sin((((double)ws.WorkShopProfile.Longitude - lon1) * Math.PI / 180) / 2), 2)
                        )),
                        IsClosed = !ws.WorkShopProfile.WorkingHours.Any(wh =>
                            wh.Day == dayOfWeek &&
                            !wh.IsClosed &&
                            wh.From <= appointmentTime &&
                            wh.To >= appointmentTime
                        )
                    })
                    .OrderBy(x => x.IsClosed)
                    .ThenBy(x => x.Distance)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.WorkshopService);
            }
            else
            {
                finalQuery = baseQuery
                    .Select(ws => new
                    {
                        WorkshopService = ws,
                        IsClosed = !ws.WorkShopProfile.WorkingHours.Any(wh =>
                            wh.Day == dayOfWeek &&
                            !wh.IsClosed &&
                            wh.From <= appointmentTime &&
                            wh.To >= appointmentTime
                        )
                    })
                    .OrderBy(x => x.IsClosed)
                    .ThenByDescending(x => x.WorkshopService.WorkShopProfile.Rating)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.WorkshopService);
            }

            // Load navigation properties only for paginated results
            var data = await finalQuery
                .Include(ws => ws.WorkShopProfile)
                    .ThenInclude(w => w.WorkingHours)
                .Include(ws => ws.Service)
                .ToListAsync();

            return new PagedResult<WorkshopService>
            {
                Items = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
    }
}