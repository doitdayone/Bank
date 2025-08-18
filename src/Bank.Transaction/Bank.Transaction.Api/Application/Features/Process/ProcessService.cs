using Bank.Transaction.Api.Application.Database;
using Bank.Transaction.Api.Application.External.ServiceBusSender;
using Bank.Transaction.Api.Domain.Constants;
using Bank.Transaction.Api.Domain.Entities.Transaction;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;

namespace Bank.Transaction.Api.Application.Features.Process
{
    public class ProcessService : IProcessService
    {
        private readonly IDatabaseService  _databaseService;
        private readonly IServiceBusSenderService _serviceBusSenderService;
        public ProcessService(IDatabaseService databaseService, IServiceBusSenderService serviceBusSenderService)
        {
            _databaseService = databaseService;
            _serviceBusSenderService = serviceBusSenderService;
        }
        public async Task Execute(string message, string subscription)
        {
            switch (subscription)
            {
                case ReceivedSubscriptionsConstants.TRANSACTION_INITIATED:
                    await TransactionInitiated(message);
                    break;
                case ReceivedSubscriptionsConstants.BALANCE_CONFIRMED:
                    await BalanceConfirmed(message);
                    break;
                case ReceivedSubscriptionsConstants.BALANCE_FAILED:
                    await BalanceFailed(message);
                    break;
                case ReceivedSubscriptionsConstants.TRANSFER_FAILED:
                    await TransferFailed(message);
                    break;
                case ReceivedSubscriptionsConstants.TRANSFER_CONFIRMED:
                    await TransferConfirmed(message);
                    break;
            }
        }

        private async Task TransactionInitiated(string message)
        {
            var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
            entity.CurrentState = CurrentStateConstants.PENDING; 
            var saveEntity = await ProcessDatabase(entity);

            var eventModel = new { saveEntity.CorrelationId, saveEntity.CustomerId };
            if (saveEntity.Id != 0)
            {
                await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.BALANCE_INITIATED);
            }
            else
            {
                await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSACTION_FAILED);
            }
        }

        private async Task BalanceConfirmed(string message)
        {
            var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
            entity.CurrentState = CurrentStateConstants.PENDING;
            var saveEntity = await ProcessDatabase(entity);

            var eventModel = new { entity.CorrelationId, entity.CustomerId, entity.Amount, entity.SourceAccount, entity.DestinationAccount };
            await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSFER_INITIATED);
        }

        private async Task BalanceFailed(string message)
        {
            var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
            entity.CurrentState = CurrentStateConstants.CANCELED;
            var saveEntity = await ProcessDatabase(entity);

            var eventModel = new { entity.CorrelationId, entity.CustomerId, entity.Amount };

            //MS Notification
            await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSACTION_FAILED);
        }

        private async Task TransferConfirmed(string message)
        {
            var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
            entity.CurrentState = CurrentStateConstants.COMPLETED;
            var saveEntity = await ProcessDatabase(entity);

            var eventModel = new { entity.CorrelationId, entity.CustomerId, entity.Amount };

            //MS Notification
            await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSACTION_COMPLETED);
            //MS Balance
            await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSFER_CONFIRMED_BALANCE);
        }

        private async Task TransferFailed(string message)
        {
            var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
            entity.CurrentState = CurrentStateConstants.CANCELED;
            var saveEntity = await ProcessDatabase(entity);

            var eventModel = new { entity.CorrelationId, entity.CustomerId, entity.Amount };

            //MS Notification
            await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSACTION_FAILED);
            //MS Balance
            await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSFER_FAILED_BALANCE);
        }

        private async Task<TransactionEntity> ProcessDatabase(TransactionEntity entity)
        {
            var existEntity = _databaseService.Transaction
                .FirstOrDefault(x => x.CorrelationId == entity.CorrelationId);

            if (existEntity == null)
            {
                entity.TransactionDate = DateTime.UtcNow;
                await _databaseService.Transaction.AddAsync(entity);
                await _databaseService.SaveAsync();
                return entity;
            }
            else
            {
                existEntity.CurrentState = entity.CurrentState;
                existEntity.TransactionDate = DateTime.UtcNow;
                _databaseService.Transaction.Update(existEntity);
                await _databaseService.SaveAsync();
                return existEntity;
            }
        }
    }
}
