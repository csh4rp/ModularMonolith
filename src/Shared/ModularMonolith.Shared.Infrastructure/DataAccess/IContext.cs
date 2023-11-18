using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public interface IContext<out T> where T : DbContext
{
    T Context { get; }
}
