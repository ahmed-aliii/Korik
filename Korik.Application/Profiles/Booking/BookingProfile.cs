using AutoMapper;
using Korik.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<CreateBookingDTO, Booking>()
              .ForMember(dest => dest.Status,
                         opt => opt.MapFrom(src => BookingStatus.Pending))
              .ForMember(dest => dest.IssueDescription,
                         opt => opt.MapFrom(src => src.IssueDescription ?? string.Empty))

              // PaidAmount always = 0
              .ForMember(dest => dest.PaidAmount,
                         opt => opt.MapFrom(src => 0m))

              // PaymentStatus always unpaid
              .ForMember(dest => dest.PaymentStatus,
                         opt => opt.MapFrom(src => PaymentStatus.Unpaid))

              .ForMember(dest => dest.CreatedAt,
                         opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateBookingStatusDTO, Booking>();

            CreateMap<UpdateBookingDTO, Booking>();

            CreateMap<Booking, BookingDTO>();
        }
    }
}