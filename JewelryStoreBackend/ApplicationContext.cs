using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.Order;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Script;
using Microsoft.EntityFrameworkCore;

namespace JewelryStoreBackend;

public class ApplicationContext: DbContext
{
    public DbSet<Tokens> Tokens { get; set; }
    public DbSet<ProductsSlider> ProductsSlider { get; set; }
    public DbSet<Person> Users { get; set; }
    public DbSet<Address> Address { get; set; }
    public DbSet<Basket> Basket { get; set; }
    public DbSet<UsersLike> UsersLike { get; set; }
    public DbSet<Warehouses> Warehouses { get; set; }
    public DbSet<Coupon> Coupon { get; set; }
    
    
    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrderProducts> OrderProducts { get; set; }
    public DbSet<OrderPayments> OrderPayments { get; set; }
    public DbSet<OrderShippings> OrderShippings { get; set; }
    
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsersLike>()
            .HasOne<Person>()
            .WithMany(p => p.UsersLike)
            .HasPrincipalKey(p => p.PersonId)
            .HasForeignKey(ul => ul.PersonId);
        
        modelBuilder.Entity<Person>()
            .Property(p => p.Role)
            .HasConversion(v => v.ToString(), v => (Roles)Enum.Parse(typeof(Roles), v));
        
        modelBuilder.Entity<Coupon>()
            .Property(p => p.Action)
            .HasConversion(v => v.ToString(), v => (CouponAction)Enum.Parse(typeof(CouponAction), v));
        
        modelBuilder.Entity<OrderProducts>()
            .HasOne(op => op.Order)
            .WithMany(o => o.Products)
            .HasForeignKey(op => op.OrderId)
            .HasPrincipalKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Orders>()
            .HasOne(o => o.Users)
            .WithMany(o => o.Orders)
            .HasForeignKey(o => o.PersonId)
            .HasPrincipalKey(o => o.PersonId);
        
        modelBuilder.Entity<OrderPayments>()
            .HasOne(op => op.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<OrderPayments>(op => op.OrderId)
            .HasPrincipalKey<Orders>(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<OrderShippings>()
            .HasOne(os => os.Order)
            .WithOne(o => o.Shipping)
            .HasForeignKey<OrderShippings>(os => os.OrderId)
            .HasPrincipalKey<Orders>(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<OrderShippings>()
            .Property(p => p.DeliveryType)
            .HasConversion(v => v.ToString(), v => (DeliveryType)Enum.Parse(typeof(DeliveryType), v));
        
        modelBuilder.Entity<Orders>()
            .Property(p => p.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<OrderPayments>()
            .Property(p => p.PaymentStatus)
            .HasConversion(v => v.ToString(), v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v));
        
        modelBuilder.Entity<OrderPayments>()
            .Property(p => p.PaymentMethod)
            .HasConversion(v => v.ToString(), v => (PaymentType)Enum.Parse(typeof(PaymentType), v));
        
        base.OnModelCreating(modelBuilder);
    }
}