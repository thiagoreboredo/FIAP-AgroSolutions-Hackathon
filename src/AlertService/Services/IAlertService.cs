using AlertService.Models;

namespace AlertService.Services;

public interface IAlertService
{
    Task ProcessAlertAsync(DroughtAlert alert);
}
