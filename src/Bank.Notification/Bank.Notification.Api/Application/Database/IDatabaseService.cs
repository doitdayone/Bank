
using Bank.Notification.Api.Domain.Entities.Notification;

namespace Bank.Notification.Api.Application.Database
{
    public interface IDatabaseService
    {
        Task<List<NotificationEntity>> GetAllAsync();
        Task<bool> AddAsync(NotificationEntity entity);
    }
}
