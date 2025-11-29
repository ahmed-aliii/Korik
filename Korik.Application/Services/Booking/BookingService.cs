using Korik.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class BookingService : GenericService<Booking>, IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingPhotoService _bookingPhotoService;
        private readonly IFileStorageService _fileStorageService;

        public BookingService
            (
            IBookingRepository bookingRepository,
            IBookingPhotoService bookingPhotoService,
            IFileStorageService fileStorageService
            ) : base(bookingRepository)
        {
            _bookingRepository = bookingRepository;
            _bookingPhotoService = bookingPhotoService;
            _fileStorageService = fileStorageService;
        }

        public async Task<ServiceResult<IEnumerable<Booking>>> GetBookingsByCarIdAsync(int carId)
        {
            try
            {
                var result = await _bookingRepository.GetBookingsByCarIdAsync(carId).ToListAsync();

                if (result == null || !result.Any())
                {
                    return ServiceResult<IEnumerable<Booking>>.Fail("No bookings found for the specified car ID.");
                }

                return ServiceResult<IEnumerable<Booking>>.Ok(result, "Bookings retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Booking>>.Fail($"An error occurred while retrieving bookings: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Booking>>> GetBookingsByWorkshopProfileIdAsync(int workshopProfileId)
        {
            try
            {
                var result = await _bookingRepository.GetBookingsByWorkshopProfileIdAsync(workshopProfileId).ToListAsync();
                if (result == null || !result.Any())
                {
                    return ServiceResult<IEnumerable<Booking>>.Fail("No bookings found for the specified workshop profile ID.");
                }

                return ServiceResult<IEnumerable<Booking>>.Ok(result, "Bookings retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Booking>>.Fail($"An error occurred while retrieving bookings: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Booking>> CreateBookingWithPhotosAsync(Booking booking, List<IFormFile>? photos)
        {
            try
            {
                //booking creation
                var bookingResult = await CreateAsync(booking);

                if (!bookingResult.Success || bookingResult.Data == null)
                {
                    return ServiceResult<Booking>.Fail(
                        bookingResult.Message ?? "Failed to create booking.");
                }

                var createdBooking = bookingResult.Data;

                //Upload photos (optional)
                if (photos != null && photos.Any())
                {
                    var uploadedPhotos = new List<BookingPhoto>();

                    foreach (var photo in photos)
                    {
                        var saveResult = await _fileStorageService.SaveFileAsync(photo, "Booking");

                        if (!saveResult.Success || string.IsNullOrEmpty(saveResult.Data))
                        {
                            // rollback (future)
                            continue;
                        }

                        var photoResult = await _bookingPhotoService.CreateAsync(new BookingPhoto
                        {
                            BookingId = createdBooking.Id,
                            PhotoUrl = saveResult.Data
                        });

                        if (photoResult.Success && photoResult.Data != null)
                        {
                            uploadedPhotos.Add(photoResult.Data);
                        }
                    }

                    createdBooking.BookingPhotos = uploadedPhotos;
                }

                return ServiceResult<Booking>.Created(
                    createdBooking,
                    "Booking created successfully with photos.");
            }
            catch (Exception ex)
            {
                return ServiceResult<Booking>.Fail(
                    $"An error occurred while creating booking: {ex.Message}");
            }
        }
    }
}