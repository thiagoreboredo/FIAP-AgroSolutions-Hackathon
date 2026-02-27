namespace AlertService.Services;

public interface IPropertyStatusClient
{
    Task UpdateTalhaoStatusAsync(Guid talhaoId, string status);
}
