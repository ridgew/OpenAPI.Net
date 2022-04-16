﻿using Prism.Commands;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using Trading.UI.Sample.Models;
using Trading.UI.Sample.Services;

namespace Trading.UI.Sample.ViewModels
{
    public class CreateModifyOrderViewModel : DialogAwareViewBase
    {
        private bool _isModifyingMarketOrder;
        private bool _isModifyingPendingOrder;
        private MarketOrderModel _marketOrderModel;
        private PendingOrderModel _pendingOrderModel;
        private List<SymbolModel> _symbols;
        private readonly IApiService _apiService;
        private AccountModel _account;
        private OrderModel _orderUnderModification;
        private int _selectedTabIndex;
        private SymbolModel _pendingOrderSelectedSymbol;

        public CreateModifyOrderViewModel(IApiService apiService)
        {
            _apiService = apiService;

            PlaceMarketOrderCommand = new DelegateCommand(PlaceMarketOrder);

            ModifyMarketOrderCommand = new DelegateCommand(ModifyMarketOrder);

            PlacePendingOrderCommand = new DelegateCommand(PlacePendingOrder);

            ModifyPendingOrderCommand = new DelegateCommand(ModifyPendingOrder);
        }

        public DelegateCommand PlaceMarketOrderCommand { get; }

        public DelegateCommand ModifyMarketOrderCommand { get; }

        public DelegateCommand PlacePendingOrderCommand { get; }

        public DelegateCommand ModifyPendingOrderCommand { get; }

        public bool IsModifyingMarketOrder { get => _isModifyingMarketOrder; set => SetProperty(ref _isModifyingMarketOrder, value); }

        public bool IsModifyingPendingOrder { get => _isModifyingPendingOrder; set => SetProperty(ref _isModifyingPendingOrder, value); }

        public MarketOrderModel MarketOrderModel { get => _marketOrderModel; set => SetProperty(ref _marketOrderModel, value); }

        public PendingOrderModel PendingOrderModel { get => _pendingOrderModel; set => SetProperty(ref _pendingOrderModel, value); }

        public List<SymbolModel> Symbols { get => _symbols; set => SetProperty(ref _symbols, value); }

        public int SelectedTabIndex { get => _selectedTabIndex; set => SetProperty(ref _selectedTabIndex, value); }

        public SymbolModel PendingOrderSelectedSymbol
        {
            get => _pendingOrderSelectedSymbol;
            set
            {
                var pendingOrderModel = PendingOrderModel;

                if (SetProperty(ref _pendingOrderSelectedSymbol, value) is false || pendingOrderModel is null || IsModifyingPendingOrder is true) return;

                pendingOrderModel.Symbol = value;

                pendingOrderModel.Price = pendingOrderModel.TradeSide == ProtoOATradeSide.Buy ? value.Ask : value.Bid;
            }
        }

        public override void OnDialogClosed()
        {
            MarketOrderModel = null;
            PendingOrderModel = null;

            IsModifyingMarketOrder = false;
            IsModifyingPendingOrder = false;

            Symbols = null;

            _orderUnderModification = null;

            _account = null;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("Account", out _account))
            {
                Symbols = new List<SymbolModel>(_account.Symbols);
            }

            if (parameters.TryGetValue<MarketOrderModel>("Position", out var position))
            {
                Title = "Modify Market Order";

                _orderUnderModification = position;

                MarketOrderModel = position.Clone();

                IsModifyingMarketOrder = true;
            }
            else if (parameters.TryGetValue<PendingOrderModel>("PendingOrder", out var pendingOrderModel))
            {
                Title = "Modify Pending Order";

                _orderUnderModification = pendingOrderModel;

                PendingOrderModel = pendingOrderModel.Clone();

                IsModifyingPendingOrder = true;

                SelectedTabIndex = 1;
            }
            else
            {
                Title = "Create New Order";

                MarketOrderModel = new MarketOrderModel();
                PendingOrderModel = new PendingOrderModel();
            }
        }

        private void PlaceMarketOrder()
        {
            try
            {
                _apiService.CreateNewOrder(MarketOrderModel, _account.Id, _account.IsLive);
            }
            finally
            {
                OnRequestClose(new DialogResult(ButtonResult.OK));
            }
        }

        private void ModifyMarketOrder()
        {
            try
            {
                _apiService.ModifyPosition(_orderUnderModification as MarketOrderModel, MarketOrderModel, _account.Id, _account.IsLive);
            }
            finally
            {
                OnRequestClose(new DialogResult(ButtonResult.OK));
            }
        }

        private void ModifyPendingOrder()
        {
            try
            {
                _apiService.ModifyOrder(_orderUnderModification as PendingOrderModel, PendingOrderModel, _account.Id, _account.IsLive);
            }
            finally
            {
                OnRequestClose(new DialogResult(ButtonResult.OK));
            }
        }

        private void PlacePendingOrder()
        {
            try
            {
                _apiService.CreateNewOrder(PendingOrderModel, _account.Id, _account.IsLive);
            }
            finally
            {
                OnRequestClose(new DialogResult(ButtonResult.OK));
            }
        }
    }
}