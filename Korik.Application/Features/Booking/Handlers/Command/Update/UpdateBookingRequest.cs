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
        private readonly IValidator<UpdateBookingDTO> _validator;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<Car> _carRepository;
        private readonly IGenericRepository<WorkShopProfile> _workshopRepository;

        public UpdateBookingRequestHandler
            (
            IBookingService bookingService,
            IValidator<UpdateBookingDTO> validator,
            IMapper mapper,
            INotificationService notificationService,
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<Car> carRepository,
            IGenericRepository<WorkShopProfile> workshopRepository
            )
        {
            _bookingService = bookingService;
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

            // Get the existing booking to compare status
            var existingBooking = await _bookingRepository.GetByIdAsync(request.model.Id);
            if (existingBooking == null)
            {
                return ServiceResult<BookingDTO>.Fail("Booking not found.");
            }

            var previousStatus = existingBooking.Status;
            var bookingtoUpdate = _mapper.Map<Booking>(request.model);
            var updatedBooking = await _bookingService.UpdateAsync(bookingtoUpdate);


            if (!updatedBooking.Success)
            {
                return ServiceResult<BookingDTO>.Fail(updatedBooking.Message ?? "Failed to update booking.");
            }

            // Check if status has changed and send notification to car owner
            var statusChanged = previousStatus != updatedBooking.Data.Status;
            var shouldNotify = statusChanged && (
                updatedBooking.Data.Status == BookingStatus.Confirmed ||
                updatedBooking.Data.Status == BookingStatus.Rejected ||
                updatedBooking.Data.Status == BookingStatus.InProgress ||
                updatedBooking.Data.Status == BookingStatus.Completed ||
                updatedBooking.Data.Status == BookingStatus.Cancelled
            );

            if (shouldNotify)
            {
                // Get car owner ID from the car
                var car = await _carRepository.GetByIdAsync(updatedBooking.Data.CarId);
                var workshop = await _workshopRepository.GetByIdAsync(updatedBooking.Data.WorkShopProfileId);

                if (car != null && workshop != null)
                {
                    var notificationPayload = new
                    {
                        BookingId = updatedBooking.Data.Id,
                        WorkshopId = updatedBooking.Data.WorkShopProfileId,
                        WorkshopName = workshop.Name,
                        NewStatus = updatedBooking.Data.Status.ToString(),
                        AppointmentDate = updatedBooking.Data.AppointmentDate
                    };

                    await _notificationService.NotifyCarOwnerBookingStatusAsync(
                        car.CarOwnerProfileId,
                        notificationPayload
                    );
                }
            }


            var bookingDto = _mapper.Map<BookingDTO>(updatedBooking.Data);
            return ServiceResult<BookingDTO>.Ok(bookingDto, "Booking updated successfully.");
        }
    }
}
