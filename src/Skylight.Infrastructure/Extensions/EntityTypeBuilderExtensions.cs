using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skylight.Infrastructure.Extensions;

internal static class EntityTypeBuilderExtensions
{
	internal static void AddCheckConstraint<T>(this PropertyBuilder<T> builder, Func<string, string> constraint)
		where T : class
	{
		IMutableTypeBase type = builder.Metadata.DeclaringType;

		string columnName = builder.Metadata.GetColumnName();

		type.ContainingEntityType.AddCheckConstraint($"ck_{type.GetTableName()}_{columnName}_", constraint($"\"{columnName}\""));
	}
}
