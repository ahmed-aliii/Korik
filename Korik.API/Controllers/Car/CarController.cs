using Korik.Application;
using Korik.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Korik.API.Controllers
{
    [Authorize(Roles = "CAROWNER,WORKSHOP")]
    [Route("api/[controller]")]
    [ApiController]

    public class CarController : ControllerBase
    {
        #region Dependency Injection
        private readonly IMediator _mediator;
        private readonly ICarOwnerProfileService _carOwnerProfileService;

        public CarController(IMediator mediator, ICarOwnerProfileService carOwnerProfileService)
        {
            _mediator = mediator;
            _carOwnerProfileService = carOwnerProfileService;
        }
        #endregion

        #region Commands
        [HttpPost]
        [SwaggerOperation(Summary = "Create a new car")]
        public async Task<IActionResult> PostCar([FromBody] CreateCarDTO model)
        {
            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var carOwnerProfileResult = await _carOwnerProfileService.GetByApplicationUserIdAsync(applicationUserId);

            if (!carOwnerProfileResult.Success)
            {
                return ApiResponse.FromResult(this, ServiceResult<CarDTO>.Fail("Car owner profile not found for the current user."));
            }

            model.CarOwnerProfileId = carOwnerProfileResult.Data.Id;

            var result = await _mediator.Send(new CreateCarRequest(model));

            return ApiResponse.FromResult(this, result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Delete a car by Id")]
        public async Task<IActionResult> DeleteCar([FromRoute] int id)
        {
            var result = await _mediator.Send(new DeleteCarRequest(new DeleteCarDTO { Id = id }));
            return ApiResponse.FromResult(this, result);
        }




        [HttpPut]
        [SwaggerOperation(Summary = "Update a car")]
        public async Task<IActionResult> PutCar([FromBody] UpdateCarDTO model)
        {
            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var carOwnerProfileResult = await _carOwnerProfileService.GetByApplicationUserIdAsync(applicationUserId);

            if (!carOwnerProfileResult.Success)
            {
                return ApiResponse.FromResult(this, ServiceResult<CarDTO>.Fail("Car owner profile not found for the current user."));
            }

            model.CarOwnerProfileId = carOwnerProfileResult.Data.Id;

            var result = await _mediator.Send(new UpdateCarRequest(model));
            return ApiResponse.FromResult(this, result);
        }
        #endregion

        #region Queries
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get a car by Id")]
        public async Task<IActionResult> GetByIdCar([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetByIdCarRequest(new GetByIdCarDTO { Id = id }));
            return ApiResponse.FromResult(this, result);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all cars")]
        public async Task<IActionResult> GetAllCars()
        {
            var result = await _mediator.Send(new GetAllCarRequest());
            return ApiResponse.FromResult(this, result);
        }

        [HttpGet("owner/{carOwnerProfileId}")]
        [SwaggerOperation(Summary = "Get all cars by CarOwnerProfileId")]
        public async Task<IActionResult> GetCarsByOwnerProfileId([FromRoute] int carOwnerProfileId)
        {
            var result = await _mediator.Send(new GetAllCarsByCarOwnerProfileIdRequest(new GetCarsByCarOwnerProfileIdDTO
            {
                CarOwnerProfileId = carOwnerProfileId
            }));

            return ApiResponse.FromResult(this, result);
        }
        #endregion

    }
}
