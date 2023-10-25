using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using project1_webapp.Models;

namespace project1_webapp.Data
{
    public class project1_UserContext : DbContext
    {
        public project1_UserContext (DbContextOptions<project1_UserContext> options)
            : base(options)
        {
        }

        public DbSet<project1_webapp.Models.User> Users { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>().ToTable("NewTable");
        }
    }
}
