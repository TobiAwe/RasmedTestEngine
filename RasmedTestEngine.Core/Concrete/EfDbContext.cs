using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using RasmedTestEngine.Core.Entities;

namespace RasmedTestEngine.Core.Concrete
{

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class EfDbContext : IdentityDbContext<Member>
    {
        public EfDbContext() : base("EfDbContext", false)
        {
        }

        public DbSet<Examination> Examinations { get; set; }
        public DbSet<MemberProfile> MemberProfiles { get; set; }
        public DbSet<ExamAnswer> ExamAnswers { get; set; }
        public DbSet<ExamOption> ExamOptions { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }

        public DbSet<ExamCountManager> ExamCountManagers { get; set; }

        public DbSet<ResultLog> ResultLogs { get; set; } 

        public DbSet<ResultManager> ResultManagers {get; set; } 

        public DbSet<ExamAccessControl> ExamAccessControls { get; set; } 


        public static EfDbContext Create()
        {
            return new EfDbContext();
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>()
                .Property(c => c.Name)
                .HasMaxLength(128)
                .IsRequired();

            modelBuilder.Entity<Member>()
                .ToTable("AspNetUsers")
                .Property(c => c.UserName)
                .HasMaxLength(128)
                .IsRequired();
        }
       
    }
}