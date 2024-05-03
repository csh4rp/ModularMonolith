﻿using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.DataAccess.Mongo.AuditLogs.Options;

public class AuditLogOptions
{
    [Required]
    public string CollectionName { get; set; } = "audit_logs";

    [Required]
    public TimeSpan ChangeDataCaptureDiff { get; set; } = TimeSpan.FromMinutes(15);

    [Required]
    public int ChangeDataCaptureBatchSize { get; set; } = 100;
}