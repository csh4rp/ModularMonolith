namespace ModularMonolith.Shared.Domain.Abstractions;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : IEquatable<TId>;
