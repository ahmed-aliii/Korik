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

        public CreateBookingRequestHandler
            (
            IBookingService bookingService,
            IValidator<CreateBookingDTO> validator,
            IMapper mapper
            )
        {
            _bookingService = bookingService;
            _validator = validator;
            _mapper = mapper;
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

            var bookingDto = _mapper.Map<BookingDTO>(createdBooking.Data);
            return ServiceResult<BookingDTO>.Created(bookingDto, "Booking created successfully.");
        }
    }
}