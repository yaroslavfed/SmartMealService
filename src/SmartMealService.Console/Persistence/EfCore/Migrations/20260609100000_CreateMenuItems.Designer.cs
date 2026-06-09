using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMealService.Console.Persistence.EfCore.Migrations;

[DbContext(typeof(MenuDbContext))]
[Migration("20260609100000_CreateMenuItems")]
partial class CreateMenuItems
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.22");

        modelBuilder.Entity("SmartMealService.Console.Persistence.EfCore.MenuItemEntity", entity =>
        {
            entity.Property<string>("Id")
                .HasColumnType("text")
                .HasColumnName("id");

            entity.Property<string>("Article")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("article");

            entity.Property<string>("BarcodesJson")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("barcodes_json");

            entity.Property<string>("FullPath")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("full_path");

            entity.Property<bool>("IsWeighted")
                .HasColumnType("boolean")
                .HasColumnName("is_weighted");

            entity.Property<string>("Name")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("name");

            entity.Property<decimal>("Price")
                .HasColumnType("numeric(18,2)")
                .HasColumnName("price");

            entity.HasKey("Id");

            entity.ToTable("menu_items");
        });
    }
}
