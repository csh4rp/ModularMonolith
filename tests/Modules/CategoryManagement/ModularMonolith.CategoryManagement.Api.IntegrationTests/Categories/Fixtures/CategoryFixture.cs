using Bogus;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;

public sealed class CategoryFixture : Faker<Category>, IClassFixture<CategoryFixture>
{
    public CategoryFixture()
    {
        RuleFor(x => x.Id, f => new CategoryId(f.Random.Guid()));
        RuleFor(x => x.Name, f => f.Name.JobTitle());
    }

    public Category ACategory(string name = "Sample-Category") => Category.From(
        new CategoryId(Guid.Parse("3E4CFE37-A22A-4E9B-BB72-2EBC0F6788FF")),
        name, null);
}
