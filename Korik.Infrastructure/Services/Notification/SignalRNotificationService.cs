using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Korik.Domain;
using Korik.Application;
using System.Linq;

namespace Korik.Infrastructure
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IServiceProvider _serviceProvider;
 private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly IGenericRepository<WorkShopProfile> _workshopRepository;
     private readonly IGenericRepository<CarOwnerProfile> _carOwnerRepository;

        public SignalRNotificationService(
            IServiceProvider serviceProvider,
   IGenericRepository<Notification> notificationRepository,
            IGenericRepository<WorkShopProfile> workshopRepository,
            IGenericRepository<CarOwnerProfile> carOwnerRepository)
        {
  _serviceProvider = serviceProvider;
            _notificationRepository = notificationRepository;
    _workshopRepository = workshopRepository;
      _carOwnerRepository = carOwnerRepository;
        }

        public async Task NotifyWorkshopBookingRequestAsync(int workshopId, object payload)
        {
            // Get workshop profile with ApplicationUserId
       var workshop = await _workshopRepository.GetByIdAsync(workshopId);
            if (workshop == null)
            {
    return;
            }

            // Extract booking details from payload
        var bookingId = (int)payload.GetType().GetProperty("BookingId")?.GetValue(payload);
       var carOwnerId = (int)payload.GetType().GetProperty("CarOwnerId")?.GetValue(payload);
            var appointmentDate = (DateTime)payload.GetType().GetProperty("AppointmentDate")?.GetValue(payload);
 var issueDescription = (string)payload.GetType().GetProperty("IssueDescription")?.GetValue(payload);

     // Create notification in database
       var notification = new Notification
            {
              CarOwnerId = carOwnerId,
            WorkShopProfileId = workshopId,
                Message = $"New booking request received for {appointmentDate:dd/MM/yyyy HH:mm}. Issue: {issueDescription}",
       Type = "BookingRequest",
  RelatedEntityId = bookingId,
         Status = "Unread",
     CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

         // Send real-time notification via SignalR to the workshop user
         try
  {
     await SendSignalRNotification(workshop.ApplicationUserId, "WorkshopBookingRequested", new
      {
            NotificationId = notification.Id,
           BookingId = bookingId,
           CarOwnerId = carOwnerId,
Message = notification.Message,
        AppointmentDate = appointmentDate,
   IssueDescription = issueDescription,
          CreatedAt = notification.CreatedAt
      });
     }
     catch
            {
        // If SignalR fails, notification is still saved in database
          }
  }

   public async Task NotifyCarOwnerBookingStatusAsync(int carOwnerId, object payload)
        {
        // Get car owner profile with ApplicationUserId
         var carOwner = await _carOwnerRepository.GetByIdAsync(carOwnerId);
     if (carOwner == null)
   {
         return;
            }

         // Extract booking details from payload
   var bookingId = (int)payload.GetType().GetProperty("BookingId")?.GetValue(payload);
            var workshopId = (int)payload.GetType().GetProperty("WorkshopId")?.GetValue(payload);
      var newStatus = (string)payload.GetType().GetProperty("NewStatus")?.GetValue(payload);
          var workshopName = (string)payload.GetType().GetProperty("WorkshopName")?.GetValue(payload);

       // Create notification in database
        string message = newStatus switch
          {
    "Confirmed" => $"Your booking with {workshopName} has been confirmed!",
           "Rejected" => $"Your booking with {workshopName} has been declined.",
    "InProgress" => $"Your booking with {workshopName} is now in progress.",
      "Completed" => $"Your booking with {workshopName} has been completed.",
                "Cancelled" => $"Your booking with {workshopName} has been cancelled.",
          _ => $"Your booking status has been updated to {newStatus}."
   };

            var notification = new Notification
       {
      CarOwnerId = carOwnerId,
                WorkShopProfileId = workshopId,
         Message = message,
                Type = "StatusUpdate",
        RelatedEntityId = bookingId,
                Status = "Unread",
 CreatedAt = DateTime.UtcNow
            };

  await _notificationRepository.AddAsync(notification);

     // Send real-time notification via SignalR to the car owner user
          try
 {
    await SendSignalRNotification(carOwner.ApplicationUserId, "CarOwnerBookingStatusChanged", new
     {
        NotificationId = notification.Id,
        BookingId = bookingId,
          WorkshopId = workshopId,
       WorkshopName = workshopName,
               NewStatus = newStatus,
     Message = message,
           CreatedAt = notification.CreatedAt
             });
            }
            catch
            {
     // If SignalR fails, notification is still saved in database
            }
        }

        private async Task SendSignalRNotification(string userId, string method, object data)
        {
   // Get the IHubContext service dynamically to avoid circular dependency
   var hubContextType = typeof(IHubContext<>).MakeGenericType(
        AppDomain.CurrentDomain.GetAssemblies()
  .SelectMany(a => a.GetTypes())
    .FirstOrDefault(t => t.Name == "NotificationHub" && t.Namespace == "Korik.API")
?? typeof(Hub)
          );

 var hubContext = _serviceProvider.GetService(hubContextType);
            if (hubContext != null)
         {
   var clientsProperty = hubContext.GetType().GetProperty("Clients");
       if (clientsProperty != null)
     {
              dynamic clients = clientsProperty.GetValue(hubContext);
  await clients.User(userId).SendAsync(method, data);
                }
            }
        }
  }
}
