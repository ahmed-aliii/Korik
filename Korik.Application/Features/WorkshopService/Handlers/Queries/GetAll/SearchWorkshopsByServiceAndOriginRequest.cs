using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public record SearchWorkshopsByServiceAndOriginRequest(SearchWorkshopsByServiceAndOriginDTO Model)
        : IRequest<ServiceResult<PagedResult<WorkshopServiceOfferingDTO>>>
    {
    }

    public class SearchWorkshopsByServiceAndOriginRequestHandler : IRequestHandler<SearchWorkshopsByServiceAndOriginRequest, ServiceResult<PagedResult<WorkshopServiceOfferingDTO>>>
    {
        #region Dependency Injection

        private readonly IWorkshopServiceService _workshopServiceService;
        private readonly IValidator<SearchWorkshopsByServiceAndOriginDTO> _validator;
        private readonly IMapper _mapper;

        public SearchWorkshopsByServiceAndOriginRequestHandler
            (
            IWorkshopServiceService workshopServiceService,
            IValidator<SearchWorkshopsByServiceAndOriginDTO> validator,
            IMapper mapper
            )
        {
            _workshopServiceService = workshopServiceService;
            _validator = validator;
            _mapper = mapper;
        }

        #endregion Dependency Injection

        public async Task<ServiceResult<PagedResult<WorkshopServiceOfferingDTO>>> Handle(SearchWorkshopsByServiceAndOriginRequest request, CancellationToken cancellationToken)
        {
            #region Not Valid

            var validationResult = await _validator.ValidateAsync
                (
                request.Model,
                cancellationToken
                );

            if (!validationResult.IsValid)
            {
                var errors = string.Join(',', validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<PagedResult<WorkshopServiceOfferingDTO>>.Fail(errors);
            }

            #endregion Not Valid

            #region Valid

            var result = await _workshopServiceService.SearchWorkshopsAsync
                (
                 request.Model
                );

            //Not Valid
            if (!result.Success)
            {
                return ServiceResult<PagedResult<WorkshopServiceOfferingDTO>>.Fail($"Bad Request - Error While getting Data: {result.Message}");
            }

            //Valid
            //Map Entity(IEnumerable) => DTO (IEnumerable)
            var pagedResult = _mapper.Map<PagedResult<WorkshopServiceOfferingDTO>>(
                result.Data,
                    opt => opt.Items["AppointmentDate"] = request.Model.AppointmentDate

                );

            return ServiceResult<PagedResult<WorkshopServiceOfferingDTO>>.Ok(pagedResult);

            #endregion Valid
        }
    }
}