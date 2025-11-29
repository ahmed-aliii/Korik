using Korik.Application;
using Korik.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace Korik.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region App Builder

            var builder = WebApplication.CreateBuilder(args);

            #region Built-In Services: Already dclared -> Need To Register

            #region Layers Registerations

            // Configuration
            var configuration = builder.Configuration;

            // Register layers
            builder.Services.AddApplicationService(configuration);
            builder.Services.AddInfrastructureService(configuration);

            #endregion Layers Registerations

            #region JWT Authentication Service

            // Add Authentication with JWT instead of Cookies
            builder.Services.AddAuthentication(options =>
            {
                // Set the default scheme that ASP.NET Core will use to authenticate users
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                // The scheme to use when the system needs to challenge (e.g., when a user is not authenticated)
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                // The general default scheme (usually same as above)
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Add JWT Bearer authentication handler
            .AddJwtBearer(options =>
            {
                // Require HTTPS for metadata (default = true)
                options.RequireHttpsMetadata = false;

                // store the bearer token in the AuthenticationProperties after a successful sign-in
                options.SaveToken = true;

                // Configure how the token will be validated
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Ensure the token has a valid Issuer (the authority who issued it)
                    ValidateIssuer = true,

                    // Ensure the token has a valid Audience (the intended receiver of the token)
                    ValidateAudience = true,

                    // Ensure the token has not expired
                    ValidateLifetime = true,

                    // Ensure the token signature is valid (signed with our secret key)
                    ValidateIssuerSigningKey = true,

                    // The expected Issuer value (from appsettings.json → "jwt:issuer")
                    ValidIssuer = builder.Configuration["jwt:issuer"],

                    // The expected Audience value (from appsettings.json → "jwt:audience")
                    ValidAudience = builder.Configuration["jwt:audience"],

                    // The symmetric security key used to sign the token (must match the one used when generating token)
                    IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            //Google External Login

            #endregion JWT Authentication Service

            #region Controllers Service

            //SuppressModelStateInvalidFilter
            // -> If Json is Not valid Suppress Model Validation To Reach Contorller and return Custom Error
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            //.ConfigureApiBehaviorOptions(options =>
            //{
            //    options.SuppressModelStateInvalidFilter = true;
            //});

            #endregion Controllers Service

            #region Swagger Service

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });
                c.EnableAnnotations();
                c.IgnoreObsoleteProperties();

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid JWT token."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            #endregion Swagger Service

            #region CORS Policy

            ///// Browsers enforce CORS, not servers.
            ///// Server must explicitly allow cross - origin requests.
            ///// Simple requests go directly, complex ones require preflight(OPTIONS).
            ///// In .NET Core, always configure UseCors.

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins(
                                builder.Configuration["consumers:AllowFrontend"], // frontend origin from configuration
                                "http://localhost:4200" // explicitly added localhost origin
                            )
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
            });

            #endregion CORS Policy

            #region SignalR

            builder.Services.AddSignalR();

            #endregion SignalR

            #endregion Built-In Services: Already dclared -> Need To Register

            var app = builder.Build();

            #endregion App Builder

            #region Middlewares

            /////Browsers enforce CORS, not servers.
            /////Server must explicitly allow cross - origin requests.
            /////Simple requests go directly, complex ones require preflight(OPTIONS).
            ///// In.NET Core, always configure UseCors MiddleWare.
            ///// UseCors MiddleWare Must be in First
            app.UseCors("AllowFrontend");

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //show detailed error in development only
                app.UseDeveloperExceptionPage();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseAuthentication(); // Who are you? (Identity) Before Authorization
            app.UseAuthorization(); // Do you have permission? (roles, policies, claims) After Authentication

            app.MapControllers(); //Send request to the right controller action.
            
            app.MapHub<NotificationHub>("/chathub");

            app.Run(); //Start the app & stop pipeline here.

            #endregion Middlewares
        }
    }
}