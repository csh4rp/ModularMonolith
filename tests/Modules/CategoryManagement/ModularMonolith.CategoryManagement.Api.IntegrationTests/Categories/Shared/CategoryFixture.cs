using Bogus;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Shared;

public sealed class CategoryFixture : Faker<Category>, IClassFixture<CategoryFixture>
{
    public CategoryFixture()
    {
        CustomInstantiator(f => Category.From(CategoryId.From(f.Random.Guid()), f.Name.JobTitle(), null));
    }

    public Category ACategory(string name = "Sample-Category") => Category.From(
        CategoryId.From(Guid.Parse("3E4CFE37-A22A-4E9B-BB72-2EBC0F6788FF")),
        name, null);
}
