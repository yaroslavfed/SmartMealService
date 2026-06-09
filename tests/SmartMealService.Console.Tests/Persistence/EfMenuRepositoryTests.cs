using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartMealService.Console.Persistence.EfCore;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Tests.Persistence;

public class EfMenuRepositoryTests
{
    [Fact]
    public async Task InitializeAsync_ShouldCreateDatabase()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfMenuRepository(dbContext);

        await repository.InitializeAsync();

        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task SaveMenuAsync_ShouldPersistMenuItems()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfMenuRepository(dbContext);
        var menuItems = new[]
        {
            new MenuItem
            {
                Id = "5979224",
                Article = "A1004292",
                Name = "Каша гречневая",
                Price = 50,
                IsWeighted = false,
                FullPath = @"ПРОИЗВОДСТВО\Гарниры",
                Barcodes = ["57890975627974236429"]
            }
        };

        await repository.InitializeAsync();
        await repository.SaveMenuAsync(menuItems);

        var entity = await dbContext.MenuItems.SingleAsync();
        entity.Id.Should().Be("5979224");
        entity.Article.Should().Be("A1004292");
        entity.Name.Should().Be("Каша гречневая");
        entity.Price.Should().Be(50m);
        entity.Barcodes.Should().ContainSingle().Which.Should().Be("57890975627974236429");
    }

    [Fact]
    public async Task SaveMenuAsync_ShouldReplaceExistingMenuItems()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfMenuRepository(dbContext);

        await repository.InitializeAsync();
        await repository.SaveMenuAsync(
        [
            new MenuItem { Id = "5979224", Article = "A1004292", Name = "Каша гречневая", Price = 50 }
        ]);
        await repository.SaveMenuAsync(
        [
            new MenuItem { Id = "9084246", Article = "A1004293", Name = "Конфеты Коровка", Price = 300 }
        ]);

        var storedIds = await dbContext.MenuItems.Select(i => i.Id).ToListAsync();
        storedIds.Should().BeEquivalentTo(["9084246"]);
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MenuDbContext(options);
    }
}
