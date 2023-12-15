﻿using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using Npgsql;

namespace ModularMonolith.Shared.Infrastructure.DataAccess.Factories;

public sealed class DbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public DbConnectionFactory(IOptionsSnapshot<DatabaseOptions> options) => _options = options.Value;

    public NpgsqlConnection Create() => new(_options.ConnectionString);
}
