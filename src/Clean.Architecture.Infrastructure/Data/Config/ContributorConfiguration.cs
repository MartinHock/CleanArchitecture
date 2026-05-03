using Clean.Architecture.Core.ContributorAggregate;

namespace Clean.Architecture.Infrastructure.Data.Config;

public class ContributorConfiguration : IEntityTypeConfiguration<Contributor>
{
  public void Configure(EntityTypeBuilder<Contributor> builder)
  {
    builder.Property(entity => entity.Id)
      .ValueGeneratedOnAdd()
      .HasVogenConversion()
      .HasValueGenerator<ContributorIdValueGenerator>()
      .IsRequired();

    builder.Property(entity => entity.Name)
      .HasVogenConversion()
      .HasMaxLength(ContributorName.MaxLength)
      .IsRequired();

    builder.OwnsOne(entity => entity.PhoneNumber);

    builder.Property(entity => entity.Status)
      .HasConversion(
        status => status.Value,
        value => ContributorStatus.FromValue(value));
  }
}
