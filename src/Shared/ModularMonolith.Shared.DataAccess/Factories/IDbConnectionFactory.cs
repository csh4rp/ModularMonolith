using System.Data.Common;

namespace ModularMonolith.Shared.DataAccess.Factories;

public interface IDbConnectionFactory
{
    DbConnection Create();
}
