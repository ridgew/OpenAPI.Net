﻿$(document).ready(function () {
    $(".dropdown-toggle").dropdown();

    var isLoaded = false;

    $('#accountLoadingModal').on('shown.bs.modal', function () {
        if (isLoaded) {
            isLoaded = false;
            $('#accountLoadingModal').modal('hide');
        }
    })

    var tradingAccountConnection = new signalR.HubConnectionBuilder().withUrl("/tradingAccountHub").build();

    tradingAccountConnection.start().then(function () {
        $("#accounts-list").on("change", onAccountChanged);

        onAccountChanged();
    }).catch(function (err) {
        return console.error(err.toString());
    });

    tradingAccountConnection.on("AccountLoaded", function (accountLogin) {
        tradingAccountConnection.invoke("GetSymbols", accountLogin).catch(function (err) {
            return console.error(err.toString());
        });

        tradingAccountConnection.invoke("GetPositions", accountLogin).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();

        $('#accountLoadingModal').modal('hide');

        isLoaded = true;
    });

    tradingAccountConnection.on("Positions", function (data) {
        var rows = '';
        $.each(data.positions, function (i, position) {
            var row = `<tr id="${position.id}">${getPositionRowData(position)}</tr>`;

            rows += row;
        });

        $('#positions-table-body').html(rows);

        tradingAccountConnection.stream("GetPositionUpdates", data.accountLogin)
            .subscribe({
                next: (position) => {
                    var row = $('#positions-table-body').find(`#${position.id}`);

                    if (position.volume == 0) {
                        row.remove();
                        return;
                    }
                    else if (row.length == 0) {
                        newRow = `<tr id="${position.id}">${getPositionRowData(position)}</tr>`;

                        $('#positions-table-body').append(newRow);

                        return;
                    }
                    else {
                        row.html(getPositionRowData(position));
                    }
                },
                complete: () => {
                    console.info("quotes completed");
                },
                error: (err) => {
                    console.error(err.toString());
                },
            });

        event.preventDefault();
    });

    tradingAccountConnection.on("Symbols", function (data) {
        var rows = '';
        $.each(data.symbols, function (i, symbol) {
            var row = `<tr id="${symbol.id}">
                            <td>${symbol.name}</td>
                            <td id="bid">${symbol.bid}</td>
                            <td id="ask">${symbol.ask}</td></tr>`;

            rows += row;
        });

        $('#symbols-table-body').html(rows);

        tradingAccountConnection.stream("GetSymbolQuotes", data.accountLogin)
            .subscribe({
                next: (quote) => {
                    var bid = $('#symbols-table-body > #' + quote.id + ' > #bid');
                    var ask = $('#symbols-table-body > #' + quote.id + ' > #ask');

                    bid.html(quote.bid);
                    ask.html(quote.ask);
                },
                complete: () => {
                    console.info("quotes completed");
                },
                error: (err) => {
                    console.error(err.toString());
                },
            });

        event.preventDefault();
    });

    $(document).on("click", ".close-position", function () {
        tradingAccountConnection.invoke("ClosePosition", $("#accounts-list").val(), $(this).attr('id')).catch(function (err) {
            return console.error(err.toString());
        });
    });

    $(document).on("click", "#closeAllPositionsButton", function () {
        tradingAccountConnection.invoke("CloseAllPositions", $("#accounts-list").val()).catch(function (err) {
            return console.error(err.toString());
        });
    });

    $(document).on("click", ".modify-position", function () {
        var positionId = $(this).attr('id');

        alert('you clicked on button #' + positionId);
    });

    function onAccountChanged() {
        isLoaded = false;

        $("#accountLoadingModal").modal({
            backdrop: 'static',
            keyboard: false
        });

        $('#accountLoadingModal').modal('toggle')

        var accountLogin = $("#accounts-list").val();

        tradingAccountConnection.invoke("StopSymbolQuotes", accountLogin).catch(function (err) {
            return console.error(err.toString());
        });

        tradingAccountConnection.invoke("LoadAccount", accountLogin).catch(function (err) {
            return console.error(err.toString());
        });

        event.preventDefault();
    };

    function getPositionRowData(position) {
        return `<td id="id">${position.id}</td>
                <td id="symbol">${position.symbol}</td>
                <td id="direction">${position.direction}</td>
                <td id="volume">${position.volume}</td>
                <td id="openTime">${position.openTime}</td>
                <td id="price">${position.price}</td>
                <td id="stopLoss">${position.stopLoss}</td>
                <td id="takeProfit">${position.takeProfit}</td>
                <td id="commission">${position.commission}</td>
                <td id="swap">${position.swap}</td>
                <td id="margin">${position.margin}</td>
                <td id="pips">${position.pips}</td>
                <td id="label">${position.label}</td>
                <td id="comment">${position.comment}</td>
                <td id="grossProfit">${position.grossProfit}</td>
                <td id="netProfit">${position.netProfit}</td>
                <td id="buttons">
                    <button type="button" class="modify-position btn btn-secondary mr-1" id="${position.id}" data-bs-toggle="tooltip" data-bs-placement="top" title="Modify"><i class="fas fa-edit"></i></button>
                    <button type="button" class="close-position btn btn-danger ml-1" id="${position.id}" data-bs-toggle="tooltip" data-bs-placement="top" title="Close"><i class="fas fa-times"></i></button>
                </td>`;
    }
});