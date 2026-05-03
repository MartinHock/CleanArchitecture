using Clean.Architecture.Core.ContributorAggregate;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Clean.Architecture.Infrastructure.Data.Config;

public sealed class ContributorIdValueGenerator : ValueGenerator<ContributorId>
{
  private static int _current = int.MaxValue;

  public override bool GeneratesTemporaryValues => true;

  public override ContributorId Next(EntityEntry entry)
  {
    var nextValue = Interlocked.Decrement(ref _current);

    return ContributorId.From(nextValue);
  }
}
