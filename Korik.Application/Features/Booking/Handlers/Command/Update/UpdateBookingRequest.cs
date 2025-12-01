using AutoMapper;
using FluentValidation;
using Korik.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public record UpdateBookingRequest(UpdateBookingDTO model) : IRequest<ServiceResult<BookingDTO>> { }


    public class UpdateBookingRequestHandler : IRequestHandler<UpdateBookingRequest, ServiceResult<BookingDTO>>
    {
        private readonly IBookingService _bookingService;
        private readonly INotificationService _notificationService;
        private readonly ICarService _carService;
        private readonly IWorkShopProfileService _workShopProfileService;
        private readonly IValidator<UpdateBookingDTO> _validator;
        private readonly IMapper _mapper;
      
        public UpdateBookingRequestHandler
            (
            IBookingService bookingService,
            INotificationService notificationService,
            ICarService carService,
            IWorkShopProfileService workShopProfileService,
            IValidator<UpdateBookingDTO> validator,
            IMapper mapper,
            INotificationService notificationService,
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<Car> carRepository,
            IGenericRepository<WorkShopProfile> workshopRepository
            )
        {
            _bookingService = bookingService;
            _notificationService = notificationService;
            _carService = carService;
            _workShopProfileService = workShopProfileService;
            _validator = validator;
            _mapper = mapper;
            _notificationService = notificationService;
            _bookingRepository = bookingRepository;
            _carRepository = carRepository;
            _workshopRepository = workshopRepository;
        }

        public async Task<ServiceResult<BookingDTO>> Handle(UpdateBookingRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request.model, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<BookingDTO>.Fail(errors);
            }

            // Get the existing booking to compare status changes
            var existingBookingResult = await _bookingService.GetByIdAsync(request.model.Id);
            if (!existingBookingResult.Success || existingBookingResult.Data == null)
            {
                return ServiceResult<BookingDTO>.Fail("Booking not found.");
            }

            var previousStatus = existingBookingResult.Data.Status;

            var bookingtoUpdate = _mapper.Map<Booking>(request.model);
            var updatedBooking = await _bookingService.UpdateAsync(bookingtoUpdate);


            if (!updatedBooking.Success)
            {
                return ServiceResult<BookingDTO>.Fail(updatedBooking.Message ?? "Failed to update booking.");
            }

            // ✅ Send notification if status changed to Confirmed
            if (request.model.Status == BookingStatus.Confirmed && previousStatus != BookingStatus.Confirmed)
            {
                try
                {
                    // Get CarOwnerProfile to extract receiver ApplicationUserId
                    var carResult = await _carService.GetByIdWithIncludeAsync(request.model.CarId ?? updatedBooking.Data!.CarId, c => c.CarOwnerProfile);
  
                    // Get WorkShopProfile to extract sender ApplicationUserId
                    var workshopResult = await _workShopProfileService.GetByIdAsync(request.model.WorkShopProfileId ?? updatedBooking.Data!.WorkShopProfileId);

                    if (carResult.Success && carResult.Data != null && 
                        workshopResult.Success && workshopResult.Data != null)
                    {
                        var carOwnerUserId = carResult.Data.CarOwnerProfile.ApplicationUserId;
                        var workshopUserId = workshopResult.Data.ApplicationUserId;
                        var workshopName = workshopResult.Data.Name;

                        // Send notification to car owner
                        await _notificationService.SendNotificationAsync(
                            senderId: workshopUserId,
                            receiverId: carOwnerUserId,
                            message: $"Your booking at {workshopName} has been accepted! Appointment: {updatedBooking.Data!.AppointmentDate:dd/MM/yyyy HH:mm}",
                            type: NotificationType.BookingAccepted,
                            bookingId: updatedBooking.Data.Id
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the booking update
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
            }

            // ✅ Send notification if status changed to Rejected
            else if (request.model.Status == BookingStatus.Rejected && previousStatus != BookingStatus.Rejected)
            {
                try
                {
                    var carResult = await _carService.GetByIdWithIncludeAsync(request.model.CarId ?? updatedBooking.Data!.CarId, c => c.CarOwnerProfile);
                    var workshopResult = await _workShopProfileService.GetByIdAsync(request.model.WorkShopProfileId ?? updatedBooking.Data!.WorkShopProfileId);

                    if (carResult.Success && carResult.Data != null && 
                        workshopResult.Success && workshopResult.Data != null)
                    {
                        var carOwnerUserId = carResult.Data.CarOwnerProfile.ApplicationUserId;
                        var workshopUserId = workshopResult.Data.ApplicationUserId;
                        var workshopName = workshopResult.Data.Name;

                        await _notificationService.SendNotificationAsync(
                            senderId: workshopUserId,
                            receiverId: carOwnerUserId,
                            message: $"Your booking request at {workshopName} has been declined.",
                            type: NotificationType.BookingRejected,
                            bookingId: updatedBooking.Data!.Id
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
            }

            // ✅ Send notification if status changed to Completed
            else if (request.model.Status == BookingStatus.Completed && previousStatus != BookingStatus.Completed)
            {
                try
                {
                    var carResult = await _carService.GetByIdWithIncludeAsync(request.model.CarId ?? updatedBooking.Data!.CarId, c => c.CarOwnerProfile);
                    var workshopResult = await _workShopProfileService.GetByIdAsync(request.model.WorkShopProfileId ?? updatedBooking.Data!.WorkShopProfileId);

                    if (carResult.Success && carResult.Data != null && 
                        workshopResult.Success && workshopResult.Data != null)
                    {
                        var carOwnerUserId = carResult.Data.CarOwnerProfile.ApplicationUserId;
                        var workshopUserId = workshopResult.Data.ApplicationUserId;
                        var workshopName = workshopResult.Data.Name;

                        await _notificationService.SendNotificationAsync(
                            senderId: workshopUserId,
                            receiverId: carOwnerUserId,
                            message: $"Your booking at {workshopName} has been completed. Please leave a review!",
                            type: NotificationType.BookingCompleted,
                            bookingId: updatedBooking.Data!.Id
                        );
                    }
 }
 catch (Exception ex)
     {
    Console.WriteLine($"Failed to send notification: {ex.Message}");
           }
       }

            // ✅ Send notification if status changed to Cancelled
            else if (request.model.Status == BookingStatus.Cancelled && previousStatus != BookingStatus.Cancelled)
            {
                try
                {
                    var carResult = await _carService.GetByIdWithIncludeAsync(request.model.CarId ?? updatedBooking.Data!.CarId, c => c.CarOwnerProfile);
                    var workshopResult = await _workShopProfileService.GetByIdAsync(request.model.WorkShopProfileId ?? updatedBooking.Data!.WorkShopProfileId);

                    if (carResult.Success && carResult.Data != null && 
                        workshopResult.Success && workshopResult.Data != null)
                    {
                        var carOwnerUserId = carResult.Data.CarOwnerProfile.ApplicationUserId;
                        var workshopUserId = workshopResult.Data.ApplicationUserId;

                        // Determine who cancelled (for proper notification)
                        // You might need to add a field in UpdateBookingDTO to track who initiated the cancellation
                        // For now, we'll notify the workshop when car owner cancels
                        await _notificationService.SendNotificationAsync(
                            senderId: carOwnerUserId,
                            receiverId: workshopUserId,
                            message: $"Booking has been cancelled by the customer.",
                            type: NotificationType.BookingCancelled,
                            bookingId: updatedBooking.Data!.Id
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
   }
            }

            var bookingDto = _mapper.Map<BookingDTO>(updatedBooking.Data);
            return ServiceResult<BookingDTO>.Ok(bookingDto, "Booking updated successfully.");
        }
    }
}
