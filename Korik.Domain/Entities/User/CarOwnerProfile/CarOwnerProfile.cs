using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Domain
{
    public enum PreferredLanguage
    {
        English,
        Arabic
    }

    public class CarOwnerProfile : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public PreferredLanguage PreferredLanguage { get; set; } = PreferredLanguage.English;

        #region Relationships

        [ForeignKey(nameof(ApplicationUser))]
        public string ApplicationUserId { get; set; } = string.Empty;

        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        #endregion Relationships

        #region CarOwnerProfile 1---M Car

        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

        #endregion CarOwnerProfile 1---M Car

        #region CarOwnerProfile 1---M Review
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        #endregion

        #region CarOwnerProfile 1---M Notification

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        #endregion
    }
}