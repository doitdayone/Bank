﻿using Bank.Gateway.Api.Application.Features;
using Bank.Gateway.Api.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Gateway.Api.Api.Endpoint
{
    public static class ApiGatewayEndpoint
    {
        public static void GatewayEndpoints(WebApplication app)
        {
            app.MapPost("/api/gateway", async ([FromBody] EndPointModel modelRequest,
                [FromServices] IProcessService _processService) =>
            {
                await _processService.Execute(modelRequest);
                return modelRequest;
            });
        }
    }
}
