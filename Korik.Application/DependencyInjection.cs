using FluentValidation;
using Korik.Application;
using Korik.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            #region AutoMapper

            var licenseKey = configuration["AutoMapper:LicenseKey"];
            services.AddAutoMapper(cfg =>
            {
                cfg.LicenseKey = licenseKey;
            },
                Assembly.GetExecutingAssembly()
            );

            #endregion AutoMapper

            #region FluentValidation

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            #endregion FluentValidation

            #region Mediator

            services.AddMediatR(config =>
                   config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            );

            #endregion Mediator

            #region Fluent Email

            // Configure FluentEmail with Gmail
            services
                .AddFluentEmail("ahmedmah1284@gmail.com") // sender email
                .AddSmtpSender(new SmtpClient("smtp.gmail.com")
                {
                    Port = 587, // TLS port
                    Credentials = new System.Net.NetworkCredential(
                        "ahmedmah1284@gmail.com",
                        "ekdx ewdr rzij bpfe" // your Gmail App Password
                    ),
                    EnableSsl = true
                });

            #endregion Fluent Email

            #region SignalR

            //services.AddSignalR(options =>
            //{
            //    options.EnableDetailedErrors = true;
            //    //options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            //    //options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            //});

            #endregion SignalR

            #region AuthService & AccountService

            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IAccountService, AccountService>();

            #endregion AuthService & AccountService

            #region Generic Service

            services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));

            #endregion Generic Service

            #region CarOwnerProfileService

            services.AddScoped<ICarOwnerProfileService, CarOwnerProfileService>();

            #endregion CarOwnerProfileService

            #region WorkShopProfile Service

            services.AddScoped<IWorkShopProfileService, WorkShopProfileService>();
            services.AddScoped<IWorkShopPhotoService, WorkShopPhotoService>();

            services.AddScoped<IWorkShopWorkingHoursService, WorkShopWorkingHoursService>();

            #endregion WorkShopProfile Service

            #region Car Service

            services.AddScoped<ICarService, CarService>();

            #endregion Car Service

            #region Car Expense Service

            services.AddScoped<ICarExpenseService, CarExpenseService>();

            #endregion Car Expense Service

            #region Car Indicator Service

            services.AddScoped<ICarIndicatorService, CarIndicatorService>();

            #endregion Car Indicator Service

            #region ICarIndicatorStatusService

            services.AddScoped<ICarIndicatorStatusService, CarIndicatorStatusService>();

            #endregion ICarIndicatorStatusService

            #region CarOwnerProfile Service

            services.AddScoped<ICarOwnerProfileService, CarOwnerProfileService>();

            #endregion CarOwnerProfile Service

            #region Category Service

            services.AddScoped<ICategoryService, CategoryService>();

            #endregion Category Service

            #region SubcategoryService

            services.AddScoped<ISubcategoryService, SubcategoryService>();

            #endregion SubcategoryService

            #region Service Service

            services.AddScoped<IServiceService, ServiceService>();

            #endregion Service Service

            #region WorkshopServiceService

            services.AddScoped<IWorkshopServiceService, WorkshopServiceService>();

            #endregion WorkshopServiceService

            #region BookingPhoto

            services.AddScoped<IBookingPhotoService, BookingPhotoService>();

            #endregion BookingPhoto

            #region Review Service
            services.AddScoped<IReviewService, ReviewService>();
            #endregion

            #region Booking Service
            services.AddScoped<IBookingService, BookingService>();
            #endregion Booking Service

            return services;
        }
    }
}