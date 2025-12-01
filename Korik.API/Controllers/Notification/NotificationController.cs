using Korik.Application;
using Korik.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace Korik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  [Authorize]
    public class NotificationController : ControllerBase
  {
        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly IGenericRepository<CarOwnerProfile> _carOwnerRepository;
        private readonly IGenericRepository<WorkShopProfile> _workshopRepository;

        public NotificationController(
            IGenericRepository<Notification> notificationRepository,
            IGenericRepository<CarOwnerProfile> carOwnerRepository,
  IGenericRepository<WorkShopProfile> workshopRepository)
        {
     _notificationRepository = notificationRepository;
   _carOwnerRepository = carOwnerRepository;
_workshopRepository = workshopRepository;
        }

        [HttpGet("CarOwner")]
        [SwaggerOperation(
         Summary = "Get all notifications for the current car owner",
            Description = "Retrieves all notifications for the authenticated car owner user."
        )]
        public async Task<IActionResult> GetCarOwnerNotifications()
        {
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         if (string.IsNullOrEmpty(userId))
 {
      return Unauthorized("User not authenticated.");
  }

            // Get car owner profile
     var carOwner = await _carOwnerRepository.GetAllAsync()
 .FirstOrDefaultAsync(co => co.ApplicationUserId == userId);

            if (carOwner == null)
   {
      return NotFound("Car owner profile not found.");
   }

  // Get notifications
            var notifications = await _notificationRepository.GetAllAsync()
 .Where(n => n.CarOwnerId == carOwner.Id)
 .OrderByDescending(n => n.CreatedAt)
        .Select(n => new
           {
      n.Id,
          n.Message,
      n.Type,
        n.Status,
     n.RelatedEntityId,
     n.CreatedAt,
         n.ReadAt
   })
   .ToListAsync();

   return Ok(new { success = true, data = notifications, message = "Notifications retrieved successfully." });
        }

        [HttpGet("Workshop")]
        [SwaggerOperation(
         Summary = "Get all notifications for the current workshop",
      Description = "Retrieves all notifications for the authenticated workshop user."
        )]
        public async Task<IActionResult> GetWorkshopNotifications()
        {
       var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
          if (string.IsNullOrEmpty(userId))
   {
       return Unauthorized("User not authenticated.");
   }

      // Get workshop profile
            var workshop = await _workshopRepository.GetAllAsync()
    .FirstOrDefaultAsync(ws => ws.ApplicationUserId == userId);

            if (workshop == null)
          {
                return NotFound("Workshop profile not found.");
         }

 // Get notifications
 var notifications = await _notificationRepository.GetAllAsync()
  .Where(n => n.WorkShopProfileId == workshop.Id)
    .OrderByDescending(n => n.CreatedAt)
          .Select(n => new
          {
           n.Id,
           n.Message,
     n.Type,
  n.Status,
    n.RelatedEntityId,
       n.CreatedAt,
           n.ReadAt
                })
     .ToListAsync();

            return Ok(new { success = true, data = notifications, message = "Notifications retrieved successfully." });
        }

        [HttpPut("{id}/MarkAsRead")]
   [SwaggerOperation(
    Summary = "Mark a notification as read",
            Description = "Updates a notification's status to 'Read' and sets the ReadAt timestamp."
        )]
 public async Task<IActionResult> MarkAsRead(int id)
        {
     var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
     {
    return NotFound("Notification not found.");
         }

  // Verify the user owns this notification
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
            {
 return Unauthorized("User not authenticated.");
        }

  var carOwner = await _carOwnerRepository.GetAllAsync()
       .FirstOrDefaultAsync(co => co.ApplicationUserId == userId);
     var workshop = await _workshopRepository.GetAllAsync()
    .FirstOrDefaultAsync(ws => ws.ApplicationUserId == userId);

            if ((carOwner == null || notification.CarOwnerId != carOwner.Id) &&
    (workshop == null || notification.WorkShopProfileId != workshop.Id))
          {
                return Forbid("You do not have permission to mark this notification as read.");
            }

            notification.Status = "Read";
notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);

return Ok(new { success = true, message = "Notification marked as read." });
        }

    [HttpDelete("{id}")]
        [SwaggerOperation(
    Summary = "Delete a notification",
    Description = "Deletes a notification by ID."
  )]
public async Task<IActionResult> DeleteNotification(int id)
        {
  var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
{
 return NotFound("Notification not found.");
     }

  // Verify the user owns this notification
   var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (string.IsNullOrEmpty(userId))
     {
     return Unauthorized("User not authenticated.");
  }

     var carOwner = await _carOwnerRepository.GetAllAsync()
       .FirstOrDefaultAsync(co => co.ApplicationUserId == userId);
  var workshop = await _workshopRepository.GetAllAsync()
      .FirstOrDefaultAsync(ws => ws.ApplicationUserId == userId);

    if ((carOwner == null || notification.CarOwnerId != carOwner.Id) &&
      (workshop == null || notification.WorkShopProfileId != workshop.Id))
       {
         return Forbid("You do not have permission to delete this notification.");
            }

        await _notificationRepository.DeleteAsync(id);

  return Ok(new { success = true, message = "Notification deleted successfully." });
}
    }
}
