/**
 * APP MODIFICATIONS:
 * - all initXxx functions (and other initializing functions) were removed
 * - getColor function was removed (used only by switchery plugin)
*/

(function ($) {
    'use strict';

    /**
    * Pages.
     * @constructor
     * @property {string}  VERSION      - Build Version.
     * @property {string}  AUTHOR       - Author.
     * @property {string}  SUPPORT      - Support Email.
     * @property {string}  pageScrollElement  - Scroll Element in Page.
     * @property {object}  $body - Cache Body.
     */
    var Pages = function() {
        this.VERSION = "3.0.0";
        this.AUTHOR = "Revox";
        this.SUPPORT = "support@revox.io";

        this.pageScrollElement = 'html, body';
        this.$body = $('body');

        this.setUserOS();
        this.setUserAgent();
    }

    /** @function setUserOS
    * @description SET User Operating System eg: mac,windows,etc
    * @returns {string} - Appends OSName to Pages.$body
    */
    Pages.prototype.setUserOS = function() {
        var OSName = "";
        if (navigator.appVersion.indexOf("Win") != -1) OSName = "windows";
        if (navigator.appVersion.indexOf("Mac") != -1) OSName = "mac";
        if (navigator.appVersion.indexOf("X11") != -1) OSName = "unix";
        if (navigator.appVersion.indexOf("Linux") != -1) OSName = "linux";

        this.$body.addClass(OSName);
    }

    /** @function setUserAgent
    * @description SET User Device Name to mobile | desktop
    * @returns {string} - Appends Device to Pages.$body
    */
    Pages.prototype.setUserAgent = function() {
        if (navigator.userAgent.match(/Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile/i)) {
            this.$body.addClass('mobile');
        } else {
            this.$body.addClass('desktop');
            if (navigator.userAgent.match(/MSIE 9.0/)) {
                this.$body.addClass('ie9');
            }
        }
    }

    /** @function isVisibleXs
    * @description Checks if the screen size is XS - Extra Small i.e below W480px
    * @returns {$Element} - Appends $('#pg-visible-xs') to Body
    */
    Pages.prototype.isVisibleXs = function() {
        (!$('#pg-visible-xs').length) && this.$body.append('<div id="pg-visible-xs" class="visible-xs" />');
        return $('#pg-visible-xs').is(':visible');
    }

    /** @function isVisibleSm
    * @description Checks if the screen size is SM - Small Screen i.e Above W480px
    * @returns {$Element} - Appends $('#pg-visible-sm') to Body
    */
    Pages.prototype.isVisibleSm = function() {
        (!$('#pg-visible-sm').length) && this.$body.append('<div id="pg-visible-sm" class="visible-sm" />');
        return $('#pg-visible-sm').is(':visible');
    }

    /** @function isVisibleMd
    * @description Checks if the screen size is MD - Medium Screen i.e Above W1024px
    * @returns {$Element} - Appends $('#pg-visible-md') to Body
    */
    Pages.prototype.isVisibleMd = function() {
        (!$('#pg-visible-md').length) && this.$body.append('<div id="pg-visible-md" class="visible-md" />');
        return $('#pg-visible-md').is(':visible');
    }

    /** @function isVisibleLg
    * @description Checks if the screen size is LG - Large Screen i.e Above W1200px
    * @returns {$Element} - Appends $('#pg-visible-lg') to Body
    */
    Pages.prototype.isVisibleLg = function() {
        (!$('#pg-visible-lg').length) && this.$body.append('<div id="pg-visible-lg" class="visible-lg" />');
        return $('#pg-visible-lg').is(':visible');
    }

    /** @function getUserAgent
    * @description Get Current User Agent.
    * @returns {string} - mobile | desktop
    */
    Pages.prototype.getUserAgent = function() {
        return $('body').hasClass('mobile') ? "mobile" : "desktop";
    }

    /** @function setFullScreen
    * @description Make Browser fullscreen.
    */
    Pages.prototype.setFullScreen = function(element) {
        // Supports most browsers and their versions.
        var requestMethod = element.requestFullScreen || element.webkitRequestFullScreen || element.mozRequestFullScreen || element.msRequestFullscreen;

        if (requestMethod) { // Native full screen.
            requestMethod.call(element);
        } else if (typeof window.ActiveXObject !== "undefined") { // Older IE.
            var wscript = new ActiveXObject("WScript.Shell");
            if (wscript !== null) {
                wscript.SendKeys("{F11}");
            }
        }
    }

    $.Pages = new Pages();
    $.Pages.Constructor = Pages;

})(window.jQuery);
