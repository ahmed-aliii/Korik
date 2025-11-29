using AutoMapper;
using Korik.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class WorkshopServiceProfile : Profile
    {
        public WorkshopServiceProfile()
        {
            //Entity => DTO || DTO => Entity
            CreateMap<WorkshopService, WorkshopServiceDTO>().ReverseMap();

            //Request => WorkshopService || WorkshopService => Request
            CreateMap<CreateWorkshopServiceDTO, WorkshopService>();

            CreateMap<UpdateWorkshopServiceDTO, WorkshopService>()
                .ForMember(dest => dest.Duration, opt => opt.Condition(src => src.Duration.HasValue))
                .ForMember(dest => dest.MinPrice, opt => opt.Condition(src => src.MinPrice.HasValue))
                .ForMember(dest => dest.MaxPrice, opt => opt.Condition(src => src.MaxPrice.HasValue))
                .ForMember(dest => dest.Origin, opt => opt.Condition(src => src.Origin.HasValue));

            CreateMap<WorkshopService, WorkshopServiceOfferingDTO>()
                // Workshop Information (from navigation property WorkShopProfile)
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(src => src.WorkShopProfileId))
                .ForMember(dest => dest.WorkshopName, opt => opt.MapFrom(src => src.WorkShopProfile.Name))
                .ForMember(dest => dest.WorkshopDescription, opt => opt.MapFrom(src => src.WorkShopProfile.Description))
                .ForMember(dest => dest.WorkshopType, opt => opt.MapFrom(src => src.WorkShopProfile.WorkShopType))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.WorkShopProfile.Country))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.WorkShopProfile.Governorate))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.WorkShopProfile.City))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.WorkShopProfile.Rating))
                .ForMember(dest => dest.LogoImageUrl, opt => opt.MapFrom(src => src.WorkShopProfile.LogoImageUrl))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.WorkShopProfile.PhoneNumber))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.WorkShopProfile.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.WorkShopProfile.Longitude))
                .ForMember(dest => dest.NumbersOfTechnicians, opt => opt.MapFrom(src => src.WorkShopProfile.NumbersOfTechnicians))
                // Service Offering Details (from WorkshopService entity itself)
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src => src.MinPrice))
                .ForMember(dest => dest.MaxPrice, opt => opt.MapFrom(src => src.MaxPrice))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin))
                // Service Details (from Service table)
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.ServiceDescription, opt => opt.MapFrom(src => src.Service.Description))
                .ForMember(dest => dest.IsClosed, opt => opt.MapFrom((src, dest, destMember, context) =>
                 {
                     // Get appointment date from mapping context
                     if (context.Items.TryGetValue("AppointmentDate", out var appointmentDateObj)
                         && appointmentDateObj is DateTime appointmentDate)
                     {
                         var dayOfWeek = appointmentDate.DayOfWeek;
                         var appointmentTime = TimeOnly.FromDateTime(appointmentDate);

                         // Check if workshop is open at this time
                         var isOpen = src.WorkShopProfile.WorkingHours.Any(wh =>
                             wh.Day == dayOfWeek &&
                             !wh.IsClosed &&
                             wh.From <= appointmentTime &&
                             wh.To >= appointmentTime
                         );

                         return !isOpen; // IsClosed = true if NOT open
                     }

                     return false; // Default to not closed if no date provided
                 }));

            CreateMap<PagedResult<WorkshopService>, PagedResult<WorkshopServiceOfferingDTO>>()
                     .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<WorkshopService, WorkshopServiceWithServiceDTO>()
                //from WorkshopService entity itself
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src => src.MinPrice))
                .ForMember(dest => dest.MaxPrice, opt => opt.MapFrom(src => src.MaxPrice))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin))
                // Service Details (from Service table)
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.ServiceDescription, opt => opt.MapFrom(src => src.Service.Description));

            CreateMap<PagedResult<WorkshopService>, PagedResult<WorkshopServiceWithServiceDTO>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        }
    }
}