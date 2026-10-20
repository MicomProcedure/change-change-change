mergeInto(LibraryManager.library, {
  WebScreenDisplay_SetDevicePixelRatioLimit: function (maxRatio) {
    Module.devicePixelRatio = Math.min(window.devicePixelRatio || 1, maxRatio);
  }
});
