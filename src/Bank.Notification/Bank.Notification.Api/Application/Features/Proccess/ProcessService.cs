using Bank.Notification.Api.Application.Database;
using Bank.Notification.Api.Application.External.SendGridEmail;
using Bank.Notification.Api.Application.Models;
using Bank.Notification.Api.Domain.Constants;
using Bank.Notification.Api.Domain.Entities.Notification;
using Newtonsoft.Json;

namespace Bank.Notification.Api.Application.Features.Proccess
{
    public class ProcessService : IProcessService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ISendGridEmailService _sendGridEmailService;
        private readonly IConfiguration _configuration;
        public ProcessService(IDatabaseService databaseService, ISendGridEmailService sendGridEmailService, IConfiguration configuration) 
        {
            _databaseService = databaseService;
            _sendGridEmailService = sendGridEmailService;
            _configuration = configuration;
        }
        public async Task Execute(string message, string subscription)
        {
            var entity = JsonConvert.DeserializeObject<NotificationEntity>(message);

            string emailPayLoad = string.Empty;
            string fromEmail = _configuration["SENDGRIDFROMEMAIL"];
            string toEmail = "dorayaki22t5@gmail.com"; // This should ideally come from the entity or configuration

            if (subscription.Equals(ReceivedSubscriptionsConstants.TRANSACTION_COMPLETED))
            {
                entity.TransactionState = true;
                entity.Content = "Transaction completed successfully.";

                string status = "Transaction completed successfully.";
                emailPayLoad = CreateSendGridModel.Create(status, entity.Content, fromEmail, toEmail);
            }
            else
            {
                entity.TransactionState = false;
                entity.Content = "Transaction failed, try again later.";

                string status = "Transaction failed";
                emailPayLoad = CreateSendGridModel.Create(status, entity.Content, fromEmail, toEmail);
            }

            // Send email using SendGrid
            await _sendGridEmailService.Execute(emailPayLoad);

            await ProcessDatabase(entity);
        }
        public async Task ProcessDatabase(NotificationEntity entity)
        {
            entity.Type = "email";
            await _databaseService.AddAsync(entity);
        }
    }
}
