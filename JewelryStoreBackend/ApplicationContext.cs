﻿using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Models.DB.User;
using Microsoft.EntityFrameworkCore;

namespace JewelryStoreBackend;

public class ApplicationContext: DbContext
{
    public DbSet<Tokens> Tokens { get; set; }
    public DbSet<ProductsSlider> ProductsSlider { get; set; }
    public DbSet<Person> Users { get; set; }
    public DbSet<Basket> Basket { get; set; }
    public DbSet<UsersLike> UsersLike { get; set; }
    
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsersLike>()
            .HasOne<Person>()
            .WithMany(p => p.UsersLike)
            .HasPrincipalKey(p => p.PersonId)
            .HasForeignKey(ul => ul.PersonId);
        
        modelBuilder.Entity<Person>()
            .Property(p => p.Role)
            .HasConversion(v => v.ToString(), v => (Roles)Enum.Parse(typeof(Roles), v));

        base.OnModelCreating(modelBuilder);
    }
}