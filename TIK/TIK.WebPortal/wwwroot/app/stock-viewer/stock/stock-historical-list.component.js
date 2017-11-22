(function () {
    'use strict';
    
    angular.module('stockViewer').component('stockHistoricalList', {
        bindings: {
            stockHistorical: '<',
        },
        controllerAs: 'vm',
        controller: function () {
            var vm = this;

        },
        templateUrl: 'App/stock-viewer/stock/stock-historical-list.component.html'
    });    
})();