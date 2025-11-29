using Korik.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Infrastructure
{
    public class Korik : IdentityDbContext<ApplicationUser>
    {
        #region CTORs

        public Korik() : base()
        {
        }

        public Korik(DbContextOptions<Korik> options) : base(options)
        {
        }

        #endregion CTORs

        #region DbSets

        public virtual DbSet<CarOwnerProfile> CarOwnerProfiles { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<WorkshopService> WorkshopServices { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }

        #region Booking

        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<BookingPhoto> BookingPhotos { get; set; }

        #endregion Booking

        #region WorkShop

        public virtual DbSet<WorkShopProfile> WorkShopProfiles { get; set; }
        public virtual DbSet<WorkShopPhoto> WorkShopPhotos { get; set; }
        public virtual DbSet<WorkingHours> WorkingHours { get; set; }

        #endregion WorkShop

        #region MyRegion

        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<CarExpenses> CarExpenses { get; set; }
        public virtual DbSet<CarIndicator> CarIndicators { get; set; }

        #endregion MyRegion

        #region Service

        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Subcategory> Subcategories { get; set; }

        #endregion Service

        #endregion DbSets

        #region Fluent API

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(CarConfiguration).Assembly);

            base.OnModelCreating(builder);
        }

        #endregion Fluent API
    }
}