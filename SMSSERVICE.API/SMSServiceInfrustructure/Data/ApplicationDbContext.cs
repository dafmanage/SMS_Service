using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegratedInfrustructure.Model.Configuration;
using IntegratedInfrustructure.Model.HRM;
using SMSServiceInfrustructure.Model.Message;
using SMSServiceInfrustructure.Model.Report;

namespace IntegratedInfrustructure.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        #region configuration

        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<GeneralCodes> GeneralCodes { get; set; }

        #endregion

        #region HRM
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<MessageGroup> MessageGroups { get; set; }
        public DbSet<MessageGroupPhone> MessageGroupPhones { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PersonalMessages> PersonalMessages { get; set; }

        public DbSet<Report> Reports { get; set; }
       

        #endregion



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            modelBuilder.Entity<GeneralCodes>()
               .HasIndex(b => b.GeneralCodeType).IsUnique();


            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.HasKey(r => new { r.UserId, r.RoleId });
            });
            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            });

            modelBuilder.Entity<MessageGroupPhone>()
      .HasIndex(e => new { e.PhoneNumber, e.MessageGroupId })
      .IsUnique();


            modelBuilder.Entity<MessageGroupPhone>()
      .Property(e => e.PhoneNumber)
      .HasMaxLength(10)
      ;
            modelBuilder.Entity<MessageGroup>()
 .HasIndex(e => e.GroupCode)
 .IsUnique();


            

        }
    }
}

