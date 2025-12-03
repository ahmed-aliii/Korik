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
    public record GetBookingsByCarIdRequest(GetBookingsByCarIdDTO model) : IRequest<ServiceResult<PagedResult<BookingDTO>>>{ }



    public class GetBookingsByCarIdRequestHandler : IRequestHandler<GetBookingsByCarIdRequest, ServiceResult<PagedResult<BookingDTO>>>
    {
        private readonly IBookingService _bookingService;
        private readonly IValidator<GetBookingsByCarIdDTO> _validator;
        private readonly IMapper _mapper;
        public GetBookingsByCarIdRequestHandler
            (
            IBookingService bookingService,
            IValidator<GetBookingsByCarIdDTO> validator,
            IMapper mapper
            )
        {
            _bookingService = bookingService;
            _validator = validator;
            _mapper = mapper;
        }
        public async Task<ServiceResult<PagedResult<BookingDTO>>> Handle(GetBookingsByCarIdRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request.model, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                var errorMessage = string.Join(", ", errors);
                return ServiceResult<PagedResult<BookingDTO>>.Fail(errorMessage);
            }

            var result = await _bookingService.GetAllPagedAsync
               (
              request.model.PageNumber,
               request.model.PageSize,
                  x => x.CarId == request.model.CarId && (x.Status == BookingStatus.Pending || x.Status == BookingStatus.Confirmed)
                  
               );

            //Not Valid
            if (!result.Success)
            {
                return ServiceResult<PagedResult<BookingDTO>>.Fail($"Bad Request - Error While getting Data: {result.Message}");
            }

         
            var bookingDTOs = _mapper.Map<PagedResult<BookingDTO>>(result.Data);

            return ServiceResult<PagedResult<BookingDTO>>.Ok(bookingDTOs, "Bookings retrieved successfully.");

        }
    }

}
