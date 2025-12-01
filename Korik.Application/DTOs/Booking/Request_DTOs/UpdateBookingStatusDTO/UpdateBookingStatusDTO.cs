using Korik.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class UpdateBookingStatusDTO
    {
        public int Id { get; set; }
        public BookingStatus Status { get; set; }

        [JsonIgnore]
        public string? ApplicationUserId { get; set; }
    }
}