using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class CreateBookingDTOValidator : AbstractValidator<CreateBookingDTO>
    {
        private readonly ICarService _carService;
        private readonly IWorkShopProfileService _workShopProfileService;
        private readonly IWorkshopServiceService _workshopServiceService;

        public CreateBookingDTOValidator(
            ICarService carService,
            IWorkShopProfileService workShopProfileService,
            IWorkshopServiceService workshopServiceService
            )
        {
            _carService = carService;
            _workShopProfileService = workShopProfileService;
            _workshopServiceService = workshopServiceService;

            RuleFor(x => x.AppointmentDate)
                .NotEmpty().WithMessage("Appointment date is required.")
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Appointment date must be in the future.");

            RuleFor(x => x.IssueDescription)
                .NotNull().WithMessage("Issue description is required.")
                .MaximumLength(500).WithMessage("Issue description must not exceed 500 characters.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Invalid payment method.");

            // ----- CarId validation -----
            RuleFor(x => x.CarId)
                .GreaterThan(0).WithMessage("CarId must be greater than 0.")
                .MustAsync(async (carId, _) =>
                {
                    var result = await _carService.IsExistAsync(carId);
                    return result.Data; // ServiceResult<bool>.Data
                })
                .WithMessage("Car does not exist.");

            // ----- WorkShopProfileId validation -----
            RuleFor(x => x.WorkShopProfileId)
                .GreaterThan(0).WithMessage("WorkShopProfileId must be greater than 0.")
                .MustAsync(async (wsId, _) =>
                {
                    var result = await _workShopProfileService.IsExistAsync(wsId);
                    return result.Data;
                })
                .WithMessage("Workshop profile does not exist.");

            // ----- WorkshopServiceId validation -----
            RuleFor(x => x.WorkshopServiceId)
                .GreaterThan(0).WithMessage("WorkshopServiceId must be greater than 0.")
                .MustAsync(async (serviceId, _) =>
                {
                    var result = await _workshopServiceService.IsExistAsync(serviceId);
                    return result.Data;
                })
                .WithMessage("Workshop service does not exist.");

            // ----- Booking Photos validation (OPTIONAL) -----
            When(x => x.Photos != null && x.Photos.Any(), () =>
            {
                RuleFor(x => x.Photos)
                    .Must(photos => photos.Count <= 10)
                    .WithMessage("Maximum 10 photos allowed");

                RuleForEach(x => x.Photos)
                    .ChildRules(photo =>
                    {
                        photo.RuleFor(f => f.Length)
                            .LessThanOrEqualTo(5 * 1024 * 1024)
                            .WithMessage("Each photo must be less than 5MB");

                        photo.RuleFor(f => f.ContentType)
                            .Must(contentType =>
                                contentType != null &&
                                (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
                                 contentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) ||
                                 contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ||
                                 contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase)))
                            .WithMessage("Only JPEG, JPG, PNG, and WebP images are allowed");

                        photo.RuleFor(f => f.FileName)
                            .NotEmpty()
                            .WithMessage("File name is required");
                    });
            });
        }
    }
}