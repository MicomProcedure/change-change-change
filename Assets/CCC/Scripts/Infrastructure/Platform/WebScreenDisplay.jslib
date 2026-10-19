mergeInto(LibraryManager.library, {
  WebScreenDisplay_SetDevicePixelRatioLimit: function (maxRatio) {
    Module.devicePixelRatio = Math.min(window.devicePixelRatio || 1, maxRatio);
  },

  WebScreenDisplay_IsFullscreenSupported: function () {
    return document.fullscreenEnabled ? 1 : 0;
  }
});