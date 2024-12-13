using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RS.Models
{
    public class ApplicationDbContext : IdentityDbContext<Employee>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

       // public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<OrderService> OrderServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Связь между OrderProduct и Order
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)
                .WithMany()
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление при удалении заказа

            // Связь между OrderProduct и Product
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)
                .WithMany()
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Каскадное удаление при удалении продукта запрет

            // Связь между OrderService и Order
            modelBuilder.Entity<OrderService>()
                .HasOne(os => os.Order)
                .WithMany()
                .HasForeignKey(os => os.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление при удалении заказа

            // Связь между OrderService и Service
            modelBuilder.Entity<OrderService>()
                .HasOne(os => os.Service)
                .WithMany()
                .HasForeignKey(os => os.ServiceId)
                .OnDelete(DeleteBehavior.Restrict); // Каскадное удаление при удалении услуги запрет

            // Связь между OrderProduct и Employee через executorId
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Executor)
                .WithMany()
                .HasForeignKey(op => op.ExecutorId)
                .OnDelete(DeleteBehavior.SetNull); // При удалении сотрудника обнуляем его задачи в OrderProduct

            // Связь между OrderService и Employee через executorId
            modelBuilder.Entity<OrderService>()
                .HasOne(os => os.Executor)
                .WithMany()
                .HasForeignKey(os => os.ExecutorId)
                .OnDelete(DeleteBehavior.SetNull); // При удалении сотрудника обнуляем его задачи в OrderService

            // Связь между Order и Employee через employeeId
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Employee) // Один заказ может быть связан с одним сотрудником
                .WithMany() // Один сотрудник может быть связан с множеством заказов
                .HasForeignKey(o => o.EmployeeId) // Связь с Employee по полю EmployeeId
                .OnDelete(DeleteBehavior.Restrict); // При удалении сотрудника заказ не удаляется (можно изменить на Cascade, если нужно)
        }
    }
}
