using Backend.Models;
using Backend.Models.Accounts;
using Backend.Models.Customers;
using Backend.Models.Emails;
using Backend.Models.Identity;
using Backend.Models.Setup;
using Backend.Models.Subscription;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Backend.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<Employee>  Employees { get; set; }
        public DbSet<Agency>  Agencies { get; set; }
        public DbSet<Branch>  Branches { get; set; }
        public DbSet<Menu>     Menus { get; set; }
        public DbSet<MenuPermission> MenuPermissions { get; set; }
        public DbSet<Module>  Modules { get; set; }
        public DbSet<Permission>   Permissions { get; set; }
        public DbSet<RolePermission>    RolePermissions { get; set; }



        /// <summary>
        /// Accounts Models
        /// </summary>
        /// 

        public DbSet<Account>  Accounts { get; set; }
        public DbSet<Currency>   Currencies { get; set; }
        public DbSet<Deposit>  Deposits { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<ExchangeRate>  ExchangeRates { get; set; }
        public DbSet<Expense>    Expenses { get; set; }
        public DbSet<Loan>     Loans { get; set; }
        public DbSet<Transaction>  Transactions { get; set; }
        public DbSet<TransactionDetail>  TransactionDetails { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Withdraw>  Withdraws { get; set; }
        public DbSet<LoanPayment> LoanPayments { get; set; }
        public DbSet<Revenue>  Revenues { get; set; }




        /// <summary>
        /// Subscription Model 
        /// </summary>

        public DbSet<Plan>  Plans { get; set; }
        public DbSet<PlanPermission>  PlanPermissions { get; set; }
        public DbSet<Subscriptions> Subscriptions { get; set; }


        /// <summary>
        /// Customers 
        /// </summary>
        /// 

        public DbSet<Customer>  Customers { get; set; }



        /// <summary>
        /// Utilities  
        /// </summary>

        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<ContactRequest>  ContactRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Agency Code Unique
            modelBuilder.Entity<Agency>()
                .HasIndex(a => a.Code)
                .IsUnique();

            // Branch Code Unique
            modelBuilder.Entity<Branch>()
                .HasIndex(b => b.Code)
                .IsUnique();

            // Only one main branch per agency
            modelBuilder.Entity<Branch>()
                .HasIndex(b => new { b.AgencyId, b.IsMain })
                .IsUnique();

            modelBuilder.Entity<Currency>()
                    .HasIndex(c => c.Code)
                    .IsUnique();

            modelBuilder.Entity<Currency>()
               .HasIndex(c => c.IsBase)
               .IsUnique()
               .HasFilter("\"IsBase\" = true");

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.ReferenceNo)
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasIndex(x => new { x.RoleId, x.PermissionId })
                .IsUnique();



            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Agency)
                .WithMany(a => a.Users)
                .HasForeignKey(u => u.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);






            // Apply all configurations automatically from the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
     
        }
    }

}
