namespace ModularMonolith.Modules.FirstModule.Domain.Entities;

public class Category
{
    public Guid Id { get; init; }
    
    public Guid? ParentId { get; set; }
    
    public string Name { get; set; }
}
