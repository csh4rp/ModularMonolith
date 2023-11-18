using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public interface IEventLogContext<out T> : IContext<T> where T : DbContext
{
    
}
