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
    public record UpdateBookingStatusRequest(UpdateBookingStatusDTO Model) : IRequest<ServiceResult<BookingDTO>> { }

    public class UpdateBookingStatusRequestHandler : IRequestHandler<UpdateBookingStatusRequest, ServiceResult<BookingDTO>>
    {
        #region Dependency Injection

        private readonly IBookingService _bookingService;
        private readonly INotificationService _notificationService;
        private readonly ICarService _carService;
        private readonly IValidator<UpdateBookingStatusDTO> _validator;
        private readonly IMapper _mapper;

        public UpdateBookingStatusRequestHandler
            (
                IBookingService bookingService,
                INotificationService notificationService,
                ICarService carService,
                IValidator<UpdateBookingStatusDTO> validator,
                IMapper mapper
            )
        {
            _bookingService = bookingService;
            _notificationService = notificationService;
            _carService = carService;
            _validator = validator;
            _mapper = mapper;
        }

        #endregion Dependency Injection

        public async Task<ServiceResult<BookingDTO>> Handle(UpdateBookingStatusRequest request, CancellationToken cancellationToken)
        {
            #region Not Valid

            var validationResult = await _validator.ValidateAsync(request.Model, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<BookingDTO>.Fail(errors);
            }

            #endregion Not Valid

            var existingBooking = await _bookingService.GetByIdAsync(request.Model.Id);

            if (!existingBooking.Success || existingBooking.Data == null)
                return ServiceResult<BookingDTO>.Fail("Booking not found.");

            _mapper.Map(request.Model, existingBooking.Data);

            var updatedBooking = await _bookingService.UpdateAsync(existingBooking.Data);

            if (!updatedBooking.Success)
                return ServiceResult<BookingDTO>.Fail("Failed to update booking.");

            var (message, type) = existingBooking.Data.Status switch
            {
                BookingStatus.Pending => (
                    "Your booking request is pending approval.",
                    NotificationType.BookingCreated
                ),

                BookingStatus.Confirmed => (
                    "Your booking has been confirmed.",
                    NotificationType.BookingAccepted
                ),

                BookingStatus.InProgress => (
                    "Your booking is currently in progress.",
                    NotificationType.BookingAccepted
                ),

                BookingStatus.Cancelled => (
                    "Your booking has been cancelled.",
                    NotificationType.BookingCancelled
                ),

                BookingStatus.Completed => (
                    "Your booking has been completed. Thank you!",
                    NotificationType.BookingCompleted
                ),

                BookingStatus.Rejected => (
                    "Your booking request has been rejected.",
                    NotificationType.BookingRejected
                ),

                BookingStatus.NoShow => (
                    "You were marked as a no-show for this booking.",
                    NotificationType.BookingCreated
                ),

                _ => (
                    "Unknown booking status.",
                    NotificationType.BookingCreated
                )
            };

            try
            {
                var carResult = await _carService.GetByIdWithIncludeAsync(updatedBooking.Data!.CarId, c => c.CarOwnerProfile);

                if (carResult.Success && carResult.Data != null)
                {
                    var carOwnerUserId = carResult.Data.CarOwnerProfile.ApplicationUserId;

                    await _notificationService.SendNotificationAsync
                        (
                        senderId: request.Model.ApplicationUserId!,
                        receiverId: carOwnerUserId,
                        message: message,
                        type: type,
                        bookingId: updatedBooking.Data!.Id
                        );
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the booking creation
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            var bookingDto = _mapper.Map<BookingDTO>(updatedBooking.Data);
            return ServiceResult<BookingDTO>.Ok(bookingDto, "Booking updated successfully.");
        }
    }
}