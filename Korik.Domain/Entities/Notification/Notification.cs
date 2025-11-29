using System;

namespace Korik.Domain
{
    public class Notification
    {
        public int Id { get; set; }
        public int CarOwnerId { get; set; } // Foreign key to CarOwnerProfile
        public int WorkShopProfileId { get; set; } // Foreign key to WorkShopProfile
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., BookingRequest, StatusUpdate
        public int? RelatedEntityId { get; set; } // Optional, e.g., BookingId
        public string Status { get; set; } = "Unread"; // Unread, Read
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        #region Relationships
        public virtual CarOwnerProfile CarOwner { get; set; } = null!;
        public virtual WorkShopProfile WorkShopProfile { get; set; } = null!;
        #endregion Relationships
    }
}
