using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System_Part2.Models
{
    public class CMCSContext : DbContext
    {
        public CMCSContext(DbContextOptions<CMCSContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
    }
}
