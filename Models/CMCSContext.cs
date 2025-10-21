using Microsoft.EntityFrameworkCore;
using System;

namespace Contract_Monthly_Claim_System_Part2.Models
{
    public class CMCSContext : DbContext
    {
        public CMCSContext(DbContextOptions<CMCSContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Store ClaimStatus enum as string in the database
            modelBuilder.Entity<Claim>()
                .Property(c => c.Status)
                .HasConversion(
                    v => v.ToString(), 
                    v => (ClaimStatus)Enum.Parse(typeof(ClaimStatus), v) 
                );

            
        }
    }
}

