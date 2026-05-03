using Clean.Architecture.Core.ContributorAggregate;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Clean.Architecture.Infrastructure.Data.Config;

public sealed class ContributorIdValueGenerator : ValueGenerator<ContributorId>
{
  private int _current = 1;

  public override bool GeneratesTemporaryValues => true;

  public override ContributorId Next(EntityEntry entry)
  {
    return ContributorId.From(_current++);
  }
}
