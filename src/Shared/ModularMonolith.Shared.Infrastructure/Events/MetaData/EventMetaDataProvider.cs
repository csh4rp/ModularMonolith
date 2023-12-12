using System.Globalization;
using EFCore.NamingConventions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;

namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public sealed class EventMetaDataProvider
{
    private static readonly EventLogMetaData EventLogMetaData = Initialize();
    
    private static EventLogMetaData Initialize()
    {
        var conventionSet = new ConventionSet();
        
        var rewritingConvention = new NameRewritingConvention(new SnakeCaseNameRewriter(CultureInfo.InvariantCulture));
        conventionSet.EntityTypeAddedConventions.Add(rewritingConvention);
        conventionSet.EntityTypeAnnotationChangedConventions.Add(rewritingConvention);
        conventionSet.PropertyAddedConventions.Add(rewritingConvention);
        conventionSet.ForeignKeyOwnershipChangedConventions.Add(rewritingConvention);
        conventionSet.KeyAddedConventions.Add(rewritingConvention);
        conventionSet.ForeignKeyAddedConventions.Add(rewritingConvention);
        conventionSet.IndexAddedConventions.Add(rewritingConvention);
        conventionSet.EntityTypeBaseTypeChangedConventions.Add(rewritingConvention);
        conventionSet.ModelFinalizingConventions.Add(rewritingConvention);
        
        var model = new ModelBuilder(conventionSet).ApplyConfiguration(new EventLogEntityTypeConfiguration()).FinalizeModel();

        var entity = model.FindEntityType(typeof(EventLog))!;

        return new EventLogMetaData
        {
            TableName = entity.GetTableName()!, 
            IdColumnName = entity.FindProperty(nameof(EventLog.Id))!.GetColumnName(),
            NameColumnName = entity.FindProperty(nameof(EventLog.Name))!.GetColumnName(),
            OperationNameColumnName = entity.FindProperty(nameof(EventLog.OperationName))!.GetColumnName(),
            PayloadColumnName = entity.FindProperty(nameof(EventLog.Payload))!.GetColumnName(),
            PublishedAtColumnName = entity.FindProperty(nameof(EventLog.PublishedAt))!.GetColumnName(),
            TypeColumnName = entity.FindProperty(nameof(EventLog.Type))!.GetColumnName(),
            ActivityIdColumnName = entity.FindProperty(nameof(EventLog.ActivityId))!.GetColumnName(),
            CorrelationIdColumnName = entity.FindProperty(nameof(EventLog.CorrelationId))!.GetColumnName(),
            CreatedAtColumnName = entity.FindProperty(nameof(EventLog.CreatedAt))!.GetColumnName(),
            UserIdColumnName = entity.FindProperty(nameof(EventLog.UserId))!.GetColumnName(),
        };
    }
    
    public EventLogMetaData GetEventLogMetaData() => EventLogMetaData;
    
    public EventLockMetaData GetEventLockMetaData() => null!;

    public EventCorrelationLockMetaData GetEventCorrelationLockMetaData() => null!;
}
