using ModularMonolith.FirstModule.Domain.Entities;

namespace ModularMonolith.FirstModule.Api.IntegrationTests.Categories;

public class CategoryFixture : IClassFixture<CategoryFixture>
{
    public Category ACategory() => new()
    {
        Id = Guid.Parse("3E4CFE37-A22A-4E9B-BB72-2EBC0F6788FF"), Name = "Category-1"
    };
}
