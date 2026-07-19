using Clean.Architecture.Core.ContributorAggregate;

#pragma warning disable CA1716 // Preserve the established public namespace.
namespace Clean.Architecture.UseCases.Contributors.Get;
#pragma warning restore CA1716

public record GetContributorQuery(ContributorId ContributorId) : IQuery<Result<ContributorDto>>;
