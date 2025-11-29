using Korik.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Korik.API.Controllers.Booking
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        #region Dependency Injection

        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #endregion Dependency Injection

        #region Commands

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new booking",
            Description = "Creates a new booking with the provided details."
        )]
        public async Task<IActionResult> PostBooking([FromForm] CreateBookingDTO model)
        {
            var result = await _mediator.Send(new CreateBookingRequest(model));

            return ApiResponse.FromResult(this, result);
        }

        [HttpPut]
        [SwaggerOperation(
            Summary = "Update an existing booking",
            Description = "Updates the details of an existing booking."
        )]
        public async Task<IActionResult> PutBooking([FromBody] UpdateBookingDTO model)
        {
            var result = await _mediator.Send(new UpdateBookingRequest(model));
            return ApiResponse.FromResult(this, result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Delete a booking by Id",
            Description = "Deletes the booking identified by the provided Id."
        )]
        public async Task<IActionResult> DeleteBooking([FromRoute] int id)
        {
            var result = await _mediator.Send(
                new DeleteBookingRequest(
                    new DeleteBookingDTO() { Id = id }
                    ));
            return ApiResponse.FromResult(this, result);
        }

        #endregion Commands

        #region Queries

        [HttpGet("ByCar/{carId:int}")]
        [SwaggerOperation(
            Summary = "Get bookings by Car Id",
            Description = "Retrieves all bookings associated with the specified Car Id."
        )]
        public async Task<IActionResult> GetBookingsByCarId([FromRoute] int carId)
        {
            var result = await _mediator.Send(
                new GetBookingsByCarIdRequest(
                    new GetBookingsByCarIdDTO() { CarId = carId }
                    ));
            return ApiResponse.FromResult(this, result);
        }

        [HttpGet("ByWorkshop/{workshopProfileId:int}")]
        [SwaggerOperation(
            Summary = "Get bookings by Workshop Id",
            Description = "Retrieves all bookings associated with the specified Workshop Id."
        )]
        public async Task<IActionResult> GetBookingsByWorkshopId([FromRoute] int workshopProfileId)
        {
            var result = await _mediator.Send(
                new GetBookingsByWorkshopProfileIdRequest(
                    new GetBookingsByWorkshopProfileIdDTO() { WorkshopProfileId = workshopProfileId }
                    ));
            return ApiResponse.FromResult(this, result);
        }

        #endregion Queries
    }
}