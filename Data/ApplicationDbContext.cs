using Microsoft.EntityFrameworkCore;
using KYCIDGenerator.Models;
using System.Collections.Generic;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options) { }

    public DbSet<KYC_Information> KYC_Information { get; set; }
    public DbSet<IndividualSample> IndividualSamples { get; set; }
    public DbSet<CorporateSample> CorporateSamples { get; set; }
    public DbSet<ManualKyc> Manual_KYCs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IndividualSample>().HasNoKey();
        modelBuilder.Entity<CorporateSample>().HasNoKey();

    }


}
