using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RS.Models
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> // Используем IdentityUser
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<OrderService> OrderServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Связь между Order и IdentityUser (AspNetUsers)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Employee) // Один заказ может быть связан с одним пользователем
                .WithMany() // Один пользователь может быть связан с множеством заказов
                .HasForeignKey(o => o.EmployeeId) // Связь с IdentityUser по EmployeeId
                .OnDelete(DeleteBehavior.Restrict); // При удалении пользователя заказ не удаляется
        }
    }
}
