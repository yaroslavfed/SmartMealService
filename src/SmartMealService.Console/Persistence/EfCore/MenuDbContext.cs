using Microsoft.EntityFrameworkCore;

namespace SmartMealService.Console.Persistence.EfCore;

public class MenuDbContext(DbContextOptions<MenuDbContext> options) : DbContext(options)
{
    public DbSet<MenuItemEntity> MenuItems => Set<MenuItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItemEntity>(entity =>
        {
            entity.ToTable("menu_items");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).HasColumnName("id");
            entity.Property(i => i.Article).HasColumnName("article").IsRequired();
            entity.Property(i => i.Name).HasColumnName("name").IsRequired();
            entity.Property(i => i.Price).HasColumnName("price").HasColumnType("numeric(18,2)");
            entity.Property(i => i.IsWeighted).HasColumnName("is_weighted");
            entity.Property(i => i.FullPath).HasColumnName("full_path").IsRequired();
            entity.Property(i => i.BarcodesJson).HasColumnName("barcodes_json").IsRequired();
            entity.Ignore(i => i.Barcodes);
        });
    }
}
