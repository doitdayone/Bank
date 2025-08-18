﻿using Bank.Balance.Api.Application.Database;
using Bank.Balance.Api.Application.External.ServiceBusSender;
using Bank.Balance.Api.Domain.Constants;
using Bank.Balance.Api.Domain.Entities.Balance;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;

namespace Bank.Balance.Api.Application.Features.Process
{
    public class ProcessService : IProcessService
    {
        private readonly IDatabaseService _databaseService;
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
                case ReceivedSubscriptionsConstants.BALANCE_INITIATED:
                    await BalanceInitiated(message);
                    break;
                case ReceivedSubscriptionsConstants.TRANSFER_CONFIRMED_BALANCE:
                    await TransferConfirmedBalance(message);
                    break;
                case ReceivedSubscriptionsConstants.TRANSFER_FAILED_BALANCE:
                    await TransferFailedBalance(message);
                    break;

            }
        }

        private async Task BalanceInitiated(string message)
        {
            var entity = JsonConvert.DeserializeObject<BalanceEntity>(message);
            entity.CurrentState = CurrentStateConstants.PENDING;
            var savedEntity = await ProcessDatabase(entity);

            var eventModel = new { entity.CorrelationId, entity.CustomerId };
            if (savedEntity.Id!= 0)
            {
                //MS Transaction
                await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.BALANCE_CONFIRMED);
            }
            else
            {
                //MS Transaction
                await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.BALANCE_FAILED);
            }

        }

        private async Task TransferConfirmedBalance(string message)
        {
            var entity = JsonConvert.DeserializeObject<BalanceEntity>(message);
            entity.CurrentState = CurrentStateConstants.COMPLETED;
            await ProcessDatabase(entity);
        }

        private async Task TransferFailedBalance(string message)
        {
            var entity = JsonConvert.DeserializeObject<BalanceEntity>(message);
            entity.CurrentState = CurrentStateConstants.CANCELED;
            await ProcessDatabase(entity);
        }

        private async Task<BalanceEntity> ProcessDatabase(BalanceEntity entity)
        {
            var existEntity = _databaseService.Balance
                .FirstOrDefault(x => x.CorrelationId == entity.CorrelationId);

            if (existEntity == null)
            {
                entity.BalanceDate = DateTime.UtcNow;
                await _databaseService.Balance.AddAsync(entity);
                await _databaseService.SaveAsync();
                return entity;
            }
            else
            {
                existEntity.CurrentState = entity.CurrentState;
                existEntity.BalanceDate = DateTime.UtcNow;
                _databaseService.Balance.Update(existEntity);
                await _databaseService.SaveAsync();
                return existEntity;
            }
        }
    }
}
