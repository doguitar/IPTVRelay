function setFocus(id) {
    var e = document.getElementById(id);
    if (e && e.focus)
        e.focus();
}