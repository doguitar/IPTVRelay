function setFocus(id) {
    var e = document.getElementById(id);
    if (e && e.focus)
        e.focus();
}
function previewVideo(id, url) {
    var video = document.getElementById(id);
    if (!video) {
        setTimeout(() => previewVideo(id, url), 200);
        return;
    }
    if (Hls.isSupported()) {
        var hls = new Hls();
        hls.loadSource(url);
        hls.attachMedia(video);
        hls.on(Hls.Events.MANIFEST_PARSED, function () {
            video.play();
        });
    }
    // HLS.js is not supported on platforms that do not have Media Source Extensions (MSE) enabled.
    // When the browser has built-in HLS support (check using `canPlayType`), we can provide an HLS manifest (i.e. .m3u8 URL) directly to the video element through the `src` attribute.
    else if (video.canPlayType('application/vnd.apple.mpegurl')) {
        video.src = url;
        video.addEventListener('loadedmetadata', function () {
            video.play();
        });
    }
}