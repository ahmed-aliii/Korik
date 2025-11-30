using Korik.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class CreateBookingDTO
    {
        public DateTime AppointmentDate { get; set; }
        public string IssueDescription { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public int CarId { get; set; }
        public int WorkShopProfileId { get; set; }
        public int WorkshopServiceId { get; set; }

        public List<IFormFile>? Photos { get; set; }
    }
}