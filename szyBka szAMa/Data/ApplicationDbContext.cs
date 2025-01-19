using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using szyBka_szAMa.Models;

namespace szyBka_szAMa.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Address> Adresses { get; set; } = null!;
        public DbSet<Basket> Baskets { get; set; } = null!;
        public DbSet<Dish> Dishes { get; set; } = null!;
       //public DbSet<DishInBasket> DishesInBaskets { get; set; } = null!;
        public DbSet<DishInOrder> DishesInOrders { get; set; } = null!;
        public DbSet<DishReview> DishReviews { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Restaurant> Restaurants { get; set; } = null!;
        public DbSet<RestaurantReview> RestaurantsReviews { get; set; } = null!;
        public DbSet<DishReview> Reviews { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserMessage> UsersMessages { get; set; } = null!;
        public DbSet<WorkHour> WorkHours { get; set; } = null!;



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Dish>()
                .HasMany(e => e.Baskets)
                .WithMany(e => e.Dishes)
                .UsingEntity<DishInBasket>(
                    l => l.HasOne<Basket>().WithMany().HasForeignKey(e => e.BasketId).OnDelete(DeleteBehavior.Restrict),
                    r => r.HasOne<Dish>().WithMany().HasForeignKey(e => e.DishId).OnDelete(DeleteBehavior.Restrict));

            builder.Entity<Dish>()
                .HasMany(e => e.Orders)
                .WithMany(e => e.Dishes)
                .UsingEntity<DishInOrder>(
                    l => l.HasOne<Order>().WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict),
                    r => r.HasOne<Dish>().WithMany().HasForeignKey(e => e.DishId).OnDelete(DeleteBehavior.Restrict));

            builder.Entity<UserMessage>()
                .HasKey(d => new { d.UserId, d.MessageId });
            
            
            // Konfiguracja relacji User -> Address (jeden do jednego)
            builder.Entity<User>()
                .HasOne(u => u.Address)
                .WithOne()
                .HasForeignKey<User>(u => u.AddressId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracja relacji User -> Restaurant (jeden do wielu)
            builder.Entity<User>()
                .HasOne(u => u.Restaurant)
                .WithMany(r => r.Employees)
                .HasForeignKey(u => u.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracja relacji Restaurant -> Address (jeden do jednego)
            builder.Entity<Restaurant>()
                .HasOne(r => r.Address)
                .WithOne()
                .HasForeignKey<Restaurant>(r => r.AddressId)
                .OnDelete(DeleteBehavior.Cascade);

            // Opcjonalne: Dodanie indeksów i ograniczeń dla niektórych kolumn
            builder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            builder.Entity<Address>()
                .HasIndex(a => a.Email)
                .IsUnique();
            
            builder.Entity<WorkHour>()
                .HasOne(w => w.Restaurant)
                .WithMany(r => r.WorkHours)
                .HasForeignKey(w => w.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);



        }

        public DbSet<szyBka_szAMa.Models.DishInBasket>? DishInBasket { get; set; }
    }
}
