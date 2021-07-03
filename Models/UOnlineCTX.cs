using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace pagadito_updater_service.Models
{
    public class UOnlineCTX:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.Configuration.GetConnectionString("UOnlineCTX"), options => options.EnableRetryOnFailure());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public virtual DbSet<pel_ern_pagadito> pel_ern_pagadito {get; set;}
    }
}