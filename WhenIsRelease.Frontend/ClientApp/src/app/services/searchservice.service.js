"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var SearchService = /** @class */ (function () {
    function SearchService(http, baseUrl) {
        this.http = http;
        this.baseUrl = baseUrl;
    }
    SearchService.prototype.search = function (searchQuery, selectedRegions, selectedPlatforms) {
        var filter = '';
        if (selectedRegions.length > 0) {
            filter += '?regions=' + selectedRegions.map(function (r) { return r.id; }).join('&regions=');
        }
        if (selectedPlatforms.length > 0) {
            filter += filter.indexOf('?') > -1 ? '&' : '?';
            filter += 'platforms=' + selectedPlatforms.map(function (p) { return p.id; }).join('&platforms=');
        }
        return this.http.get(this.baseUrl + 'api/GameService/search/' + searchQuery + filter);
    };
    SearchService = __decorate([
        core_1.Injectable(),
        __param(1, core_1.Inject('BASE_URL'))
    ], SearchService);
    return SearchService;
}());
exports.SearchService = SearchService;
//# sourceMappingURL=searchservice.service.js.map