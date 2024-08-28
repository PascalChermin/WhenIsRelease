"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var CopyClipboardDirective = /** @class */ (function () {
    function CopyClipboardDirective() {
        this.copied = new core_1.EventEmitter();
    }
    CopyClipboardDirective.prototype.onClick = function (event) {
        var _this = this;
        event.preventDefault();
        if (!this.payload)
            return;
        var range = document.createRange();
        range.selectNodeContents(document.body);
        document.getSelection().addRange(range);
        var listener = function (e) {
            var clipboard = e.clipboardData || window["clipboardData"];
            clipboard.setData("text", _this.payload.toString());
            e.preventDefault();
            _this.copied.emit(_this.payload);
        };
        document.addEventListener("copy", listener, false);
        document.execCommand("copy");
        document.removeEventListener("copy", listener, false);
        document.getSelection().removeAllRanges();
    };
    __decorate([
        core_1.Input("copy-clipboard")
    ], CopyClipboardDirective.prototype, "payload", void 0);
    __decorate([
        core_1.Input("context")
    ], CopyClipboardDirective.prototype, "context", void 0);
    __decorate([
        core_1.Output("copied")
    ], CopyClipboardDirective.prototype, "copied", void 0);
    __decorate([
        core_1.HostListener("click", ["$event"])
    ], CopyClipboardDirective.prototype, "onClick", null);
    CopyClipboardDirective = __decorate([
        core_1.Directive({ selector: '[copy-clipboard]' })
    ], CopyClipboardDirective);
    return CopyClipboardDirective;
}());
exports.CopyClipboardDirective = CopyClipboardDirective;
//# sourceMappingURL=copy-clipboard.directive.js.map