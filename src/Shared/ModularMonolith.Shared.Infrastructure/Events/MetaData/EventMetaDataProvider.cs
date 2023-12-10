using System.Globalization;
using EFCore.NamingConventions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;

namespace ModularMonolith.Shared.Infrastructure.Events.MetaData;

public class EventMetaDataProvider
{
    private static readonly EventLogMetaData EventLogMetaData = Initialize();
    
    private static EventLogMetaData Initialize()
    {
        new NamingConventionsOptionsExtension().WithSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
        
        var conventionSet = new ConventionSet();
        
        NameRewritingConvention rewritingConvention = new NameRewritingConvention(new SnakeCaseNameRewriter(CultureInfo.InvariantCulture));
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

        var idProperty = entity.FindProperty(nameof(EventLog.Id))!;
        
        return new EventLogMetaData
        {
            TableName = entity.GetTableName()!, 
            IdColumnName = idProperty.GetColumnName(),
        };
    }
    
    public EventLogMetaData GetEventLogMetaData() => EventLogMetaData;
    
    public EventLockMetaData GetEventLockMetaData() => null!;
}
