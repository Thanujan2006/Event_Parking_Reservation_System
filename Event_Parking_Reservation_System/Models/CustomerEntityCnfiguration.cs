using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_Parking_Reservation_System.Models
{
    public class CustomerEntityCnfiguration
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(c => c.CustomerId);

            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
            builder.HasIndex(c => c.Email).IsUnique();
            builder.Property(c => c.PhoneNumber).HasMaxLength(10).IsRequired();
            builder.Property(c => c.PasswordHash).HasMaxLength(512).IsRequired();
            builder.Property(c => c.Status).HasConversion<byte>();

            builder.OwnsOne(c => c.Security, security => AuthEntityConfiguration.ConfigureSecurity(security));
        }
    }
}
