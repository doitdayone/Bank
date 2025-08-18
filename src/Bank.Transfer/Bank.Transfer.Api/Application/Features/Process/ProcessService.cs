﻿
using Bank.Transfer.Api.Application.Database;
using Bank.Transfer.Api.Application.External.ServiceBusSender;
using Bank.Transfer.Api.Application.Features.Process;
using Bank.Transfer.Api.Domain.Constants;
using Bank.Transfer.Api.Domain.Entities.Transfer;
using Newtonsoft.Json;

namespace Bank.Transfer.Api.Application.Features.Process
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
        public async Task Execute(string message)
        {
            await TransferInitiated(message);
        }

        private async Task TransferInitiated(string message)
        {
            var entity = JsonConvert.DeserializeObject<TransferEntity>(message);
            var savedEntity = await ProcessDatabase(entity);

            var eventModel = new { entity.CorrelationId, entity.CustomerId };
            if (savedEntity.Id != 0)
            {
                //MS Transaction
                await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSFER_CONFIRMED);
            }
            else
            {
                //MS Transaction
                await _serviceBusSenderService.Execute(eventModel, SendSubscriptionConstants.TRANSFER_FAILED);
            }
        }

        private async Task<TransferEntity> ProcessDatabase(TransferEntity entity)
        {
            entity.TransferDate = DateTime.UtcNow;
            entity.CurrentState = CurrentStateConstants.COMPLETED;
            await _databaseService.Transfer.AddAsync(entity);
            await _databaseService.SaveAsync();
            return entity;
        }
    }
}
