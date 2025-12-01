using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Domain
{
    public class Notification : BaseEntity
    {
        [ForeignKey("Sender")]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey("Receiver")]
        public string ReceiverId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #region Notification M---1 Booking (Optional)

        [ForeignKey("Booking")]
        public int? BookingId { get; set; }

        public virtual Booking? Booking { get; set; }

        #endregion Notification M---1 Booking (Optional)

        #region Notification M---1 Sender (ApplicationUser)

        public virtual ApplicationUser Sender { get; set; } = null!;

        #endregion Notification M---1 Sender (ApplicationUser)

        #region Notification M---1 Receiver (ApplicationUser)

        public virtual ApplicationUser Receiver { get; set; } = null!;

        #endregion Notification M---1 Receiver (ApplicationUser)
    }
}
