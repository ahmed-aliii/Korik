using Korik.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Application
{
    public class WorkshopServiceOfferingDTO
    {
        // Workshop Information
        public int WorkshopId { get; set; }

        public string WorkshopName { get; set; }
        public string WorkshopDescription { get; set; }
        public WorkShopType WorkshopType { get; set; }
        public string Country { get; set; }
        public string Governorate { get; set; }
        public string City { get; set; }
        public double Rating { get; set; }
        public string LogoImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int NumbersOfTechnicians { get; set; }

        // Service Offering Details (from WorkshopService junction table)
        public int Duration { get; set; }

        public int WorkshopServiceID { get; set; }
        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }
        public CarOrigin Origin { get; set; }

        // Service Details (from Service table)
        public string ServiceName { get; set; }

        public string ServiceDescription { get; set; }

        public bool IsClosed { get; set; }
    }
}