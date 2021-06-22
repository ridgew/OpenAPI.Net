﻿using ASP.NET.Demo.Models;
using ASP.NET.Demo.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET.Demo.Hubs
{
    public class TradingAccountHub : Hub
    {
        private readonly ITradingAccountsService _tradingAccountsService;

        public TradingAccountHub(ITradingAccountsService tradingAccountsService)
        {
            _tradingAccountsService = tradingAccountsService;
        }

        public async Task LoadAccount(string accountLogin)
        {
            _ = await _tradingAccountsService.GetAccountModelByLogin(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("AccountLoaded", accountLogin);
        }

        public async Task GetSymbols(string accountLogin)
        {
            var accountModel = await _tradingAccountsService.GetAccountModelByLogin(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("Symbols", new { accountLogin, Symbols = accountModel.Symbols.Select(iSymbol => new { iSymbol.Name, iSymbol.Bid, iSymbol.Ask, iSymbol.Id }) });
        }

        public async IAsyncEnumerable<SymbolQuote> GetSymbolQuotes(string accountLogin, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            var channel = _tradingAccountsService.GetSymbolsQuoteChannel(accountId);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var quote))
                {
                    yield return quote;
                }
            }
        }

        public void StopSymbolQuotes(string accountLogin)
        {
            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            _tradingAccountsService.StopSymbolQuotes(accountId);
        }

        public async Task GetPositions(string accountLogin)
        {
            var accountModel = await _tradingAccountsService.GetAccountModelByLogin(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("Positions", new
            {
                accountLogin,
                Positions = accountModel.Positions.Select(marketOrder => Position.FromMarketOrder(marketOrder))
            });
        }

        public async IAsyncEnumerable<Position> GetPositionUpdates(string accountLogin, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            var channel = _tradingAccountsService.GetPositionUpdatesChannel(accountId);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var marketOrder))
                {
                    yield return Position.FromMarketOrder(marketOrder);
                }
            }
        }

        public void StopPositionUpdates(string accountLogin)
        {
            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            _tradingAccountsService.StopPositionUpdates(accountId);
        }

        public async Task ClosePosition(string accountLogin, string positionId)
        {
            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            await _tradingAccountsService.ClosePosition(accountId, Convert.ToInt64(positionId));
        }

        public async Task CloseAllPositions(string accountLogin)
        {
            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            await _tradingAccountsService.CloseAllPosition(accountId);
        }
    }
}