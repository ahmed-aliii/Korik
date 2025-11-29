using Korik.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Korik.Application
{
    public class WorkshopServiceService : GenericService<WorkshopService>, IWorkshopServiceService
    {
        #region Dependence Injection

        private readonly IWorkshopServiceRepository _repository;

        public WorkshopServiceService(IWorkshopServiceRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #endregion Dependence Injection

        public async Task<ServiceResult<bool>> IsOriginUniqueForUpdateAsync(int id, CarOrigin? newOrigin)
        {
            try
            {
                // If no origin is being changed, it's valid
                if (!newOrigin.HasValue)
                    return ServiceResult<bool>.Ok(true);

                // Get the existing workshop service
                var existingResult = await GetByIdAsync(id);
                if (!existingResult.Success || existingResult.Data == null)
                    return ServiceResult<bool>.Fail("Workshop service not found.");

                var existing = existingResult.Data;

                // If Origin hasn't changed, skip uniqueness check
                if (existing.Origin == newOrigin.Value)
                    return ServiceResult<bool>.Ok(true);

                // Check if combination of ServiceId + WorkShopProfileId + New Origin already exists
                var query = _repository.GetAllAsync();

                if (query == null)
                    return ServiceResult<bool>.Fail("Unable to query workshop services.");

                var isDuplicate = await query
                    .Where(ws =>
                        ws.ServiceId == existing.ServiceId &&
                        ws.WorkShopProfileId == existing.WorkShopProfileId &&
                        ws.Origin == newOrigin.Value &&
                        ws.Id != id)
                    .AnyAsync();

                return ServiceResult<bool>.Ok(!isDuplicate);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail($"Error checking origin uniqueness: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<WorkshopService>>> SearchWorkshopsAsync(
                    SearchWorkshopsByServiceAndOriginDTO searchDto)
        {
            try
            {
                var result = await _repository.SearchWorkshopsAsync(
                    searchDto.ServiceId,
                    searchDto.Origin,
                    searchDto.City,
                    searchDto.Latitude,
                    searchDto.Longitude,
                    searchDto.AppointmentDate,
                    searchDto.PageNumber,
                    searchDto.PageSize
                );

                return ServiceResult<PagedResult<WorkshopService>>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorkshopService>>.Fail(
                    $"Error searching workshops: {ex.Message}");
            }
        }
    }
}