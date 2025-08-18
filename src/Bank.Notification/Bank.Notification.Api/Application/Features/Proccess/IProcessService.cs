namespace Bank.Notification.Api.Application.Features.Proccess
{
    public interface IProcessService
    {
        Task Execute(string message, string subscription);
    }
}
