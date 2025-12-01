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
        private readonly INotificationService _notificationService;
        private readonly ICarService _carService;
        private readonly IWorkShopProfileService _workShopProfileService;
        private readonly IValidator<CreateBookingDTO> _validator;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<Car> _carRepository;
        private readonly IGenericRepository<WorkShopProfile> _workshopRepository;

        public CreateBookingRequestHandler
            (
            IBookingService bookingService,
            INotificationService notificationService,
            ICarService carService,
            IWorkShopProfileService workShopProfileService,
            IValidator<CreateBookingDTO> validator,
            IMapper mapper,
            INotificationService notificationService,
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

            // ✅ Send notification to workshop owner
            try
            {
                // Get CarOwnerProfile to extract sender ApplicationUserId
                var carResult = await _carService.GetByIdWithIncludeAsync(request.model.CarId, c => c.CarOwnerProfile);
                
                // Get WorkShopProfile to extract receiver ApplicationUserId
                var workshopResult = await _workShopProfileService.GetByIdAsync(request.model.WorkShopProfileId);

                if (carResult.Success && carResult.Data != null && 
                    workshopResult.Success && workshopResult.Data != null)
                {
                    var carOwnerUserId = carResult.Data.CarOwnerProfile.ApplicationUserId;
                    var workshopUserId = workshopResult.Data.ApplicationUserId;
                    var carOwnerName = $"{carResult.Data.CarOwnerProfile.FirstName} {carResult.Data.CarOwnerProfile.LastName}";

                    // Send notification
                    await _notificationService.SendNotificationAsync(
                        senderId: carOwnerUserId,
                        receiverId: workshopUserId,
                        message: $"New booking request from {carOwnerName} for {request.model.IssueDescription}",
                        type: NotificationType.BookingCreated,
                        bookingId: createdBooking.Data!.Id
                    );
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the booking creation
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            return ServiceResult<BookingDTO>.Created(bookingDto, "Booking created successfully.");
        }
    }
}