using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class SearchWorkshopsByServiceAndOriginDTOValidator : AbstractValidator<SearchWorkshopsByServiceAndOriginDTO>
    {
        private readonly IServiceService _serviceService;

        public SearchWorkshopsByServiceAndOriginDTOValidator(IServiceService serviceService)
        {
            _serviceService = serviceService;

            // ServiceId - Required and must exist
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage("ServiceId must be greater than 0.")
                .MustAsync(async (id, cancellationToken) =>
                {
                    var exists = await _serviceService.IsExistAsync(id);
                    return exists.Data;
                })
                .WithMessage("Service does not exist.");

            // Origin - Optional but must be valid enum if provided
            RuleFor(x => x.Origin)
                .IsInEnum()
                .When(x => x.Origin.HasValue)
                .WithMessage("Origin must be a valid CarOrigin value.");

            // Appointment Date - Must be in the future
            RuleFor(x => x.AppointmentDate)
                .NotEmpty()
                .WithMessage("Appointment date is required.")
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Appointment date must be in the future.");

            // City - Optional, validate only when provided
            RuleFor(x => x.City)
                .MaximumLength(100)
                .WithMessage("City name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.City));

            // Latitude & Longitude - Both must be provided together or not at all
            When(x => x.Latitude.HasValue || x.Longitude.HasValue, () =>
            {
                RuleFor(x => x.Latitude)
                    .NotNull()
                    .WithMessage("Latitude is required when Longitude is provided.")
                    .InclusiveBetween(-90m, 90m)
                    .WithMessage("Latitude must be between -90 and 90.");

                RuleFor(x => x.Longitude)
                    .NotNull()
                    .WithMessage("Longitude is required when Latitude is provided.")
                    .InclusiveBetween(-180m, 180m)
                    .WithMessage("Longitude must be between -180 and 180.");
            });

            // Pagination
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be greater than or equal to 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}