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
    public record CreateBookingRequest(CreateBookingDTO model) : IRequest<ServiceResult<BookingDTO>> { }

    public class CreateBookingRequestHandler : IRequestHandler<CreateBookingRequest, ServiceResult<BookingDTO>>
    {
        private readonly IBookingService _bookingService;
        private readonly IValidator<CreateBookingDTO> _validator;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<Car> _carRepository;
        private readonly IGenericRepository<WorkShopProfile> _workshopRepository;

        public CreateBookingRequestHandler
            (
            IBookingService bookingService,
            IValidator<CreateBookingDTO> validator,
            IMapper mapper,
            INotificationService notificationService,
            IGenericRepository<Car> carRepository,
            IGenericRepository<WorkShopProfile> workshopRepository
            )
        {
            _bookingService = bookingService;
            _validator = validator;
            _mapper = mapper;
            _notificationService = notificationService;
            _carRepository = carRepository;
            _workshopRepository = workshopRepository;
        }

        public async Task<ServiceResult<BookingDTO>> Handle(CreateBookingRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request.model, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<BookingDTO>.Fail(string.Join(", ", errors));
            }

            var bookingToCreate = _mapper.Map<Booking>(request.model);
            var createdBooking = await _bookingService.CreateBookingWithPhotosAsync
                (
                bookingToCreate,
                request.model.Photos
                );

            if (!createdBooking.Success)
            {
                return ServiceResult<BookingDTO>.Fail(createdBooking.Message ?? "Failed to create booking.");
            }

            // Get car owner ID from the car
            var car = await _carRepository.GetByIdAsync(createdBooking.Data.CarId);
            var workshop = await _workshopRepository.GetByIdAsync(createdBooking.Data.WorkShopProfileId);

            if (car != null && workshop != null)
            {
                // Send notification to workshop
                var notificationPayload = new
                {
                    BookingId = createdBooking.Data.Id,
                    CarOwnerId = car.CarOwnerProfileId,
                    AppointmentDate = createdBooking.Data.AppointmentDate,
                    IssueDescription = createdBooking.Data.IssueDescription,
                    WorkshopServiceId = createdBooking.Data.WorkshopServiceId,
                    CarId = createdBooking.Data.CarId
                };

                await _notificationService.NotifyWorkshopBookingRequestAsync(
                    createdBooking.Data.WorkShopProfileId,
                    notificationPayload
                );
            }

            var bookingDto = _mapper.Map<BookingDTO>(createdBooking.Data);
            return ServiceResult<BookingDTO>.Created(bookingDto, "Booking created successfully.");
        }
    }
}