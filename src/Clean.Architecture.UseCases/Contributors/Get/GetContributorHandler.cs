using Clean.Architecture.Core.ContributorAggregate;
using Clean.Architecture.Core.ContributorAggregate.Specifications;

namespace Clean.Architecture.UseCases.Contributors.GetById;

/// <summary>
/// Queries don't necessarily need to use repository methods, but they can if it's convenient.
/// </summary>
public class GetContributorHandler(
  IReadRepository<Contributor> repository)
  : IQueryHandler<GetContributorQuery, Result<ContributorDto>>
{
  public async ValueTask<Result<ContributorDto>> Handle(
    GetContributorQuery request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    var spec = new ContributorByIdSpec(request.ContributorId);

    var entity = await repository
      .FirstOrDefaultAsync(spec, cancellationToken)
      .ConfigureAwait(false);

    if (entity is null)
    {
      return Result.NotFound();
    }

    return new ContributorDto(
      entity.Id,
      entity.Name,
      entity.PhoneNumber ?? PhoneNumber.Unknown);
  }
}

