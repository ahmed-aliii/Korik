using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Domain
{
    public enum WorkShopType
    {
        Independent,
        MaintainanceCenter,
        Specialized,
        Mobile
    }

    public enum VerificationStatus
    {
        Pending,
        Verified,
        Rejected
    }

    public class WorkShopProfile : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public WorkShopType WorkShopType { get; set; }

        public string Country { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double Rating { get; set; } = 0;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public string LicenceImageUrl { get; set; }
        public string? LogoImageUrl { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int NumbersOfTechnicians { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        #region WorkShop 1---M WorkshopPhoto

        public virtual ICollection<WorkShopPhoto> WorkShopPhotos { get; set; } = new List<WorkShopPhoto>();

        #endregion WorkShop 1---M WorkshopPhoto

        #region WorkShopProfile 1---M WorkingHours

        public virtual ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();

        #endregion WorkShopProfile 1---M WorkingHours

        #region WorkShopProfile 1---1 ApplicationUser

        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        #endregion WorkShopProfile 1---1 ApplicationUser

        #region WorkShopProfile 1----M Booking

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        #endregion WorkShopProfile 1----M Booking

        #region WorkShopProfile M----M WorkshopService

        public virtual ICollection<WorkshopService> WorkshopServices { get; set; } = new List<WorkshopService>();

        #endregion WorkShopProfile M----M WorkshopService

        #region WorkShopProfile 1---M Review
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        #endregion

        #region WorkShopProfile 1---M Notification

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        #endregion
    }
}