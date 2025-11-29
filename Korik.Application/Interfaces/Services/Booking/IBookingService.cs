using Korik.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public interface IBookingService : IGenericService<Booking>
    {
        Task<ServiceResult<IEnumerable<Booking>>> GetBookingsByCarIdAsync(int carId);

        Task<ServiceResult<IEnumerable<Booking>>> GetBookingsByWorkshopProfileIdAsync(int workshopProfileId);

        Task<ServiceResult<Booking>> CreateBookingWithPhotosAsync(Booking booking, List<IFormFile>? photos);
    }
}