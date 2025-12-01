using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class UpdateBookingStatusDTOValidator : AbstractValidator<UpdateBookingStatusDTO>
    {
        private readonly ICarService _carService;
        private readonly IBookingService _bookingService;

        public UpdateBookingStatusDTOValidator
            (
              ICarService carService,
              IBookingService bookingService
            )
        {
            _carService = carService;
            _bookingService = bookingService;

            // ---- ID CHECK USING BOOKING SERVICE ----
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Booking Id must be greater than 0.")
                .MustAsync(async (id, _) =>
                {
                    var result = await _bookingService.IsExistAsync(id);
                    return result.Data;
                })
                .WithMessage("Booking does not exist.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid booking status.");

            RuleFor(x => x.ApplicationUserId)
                .NotEmpty().WithMessage("ApplicationUser is required.");
        }
    }
}