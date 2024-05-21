// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var getCookie = function (cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

var setCookie = function (cname, value) {
    document.cookie = cname + "=" + value;
    return value;
}

var toggleBoolCookie = function (cookieRef) {
    var val = getCookie(cookieRef);
    if (val == "true") {
        setCookie(cookieRef, "false");
    } else {
        setCookie(cookieRef, "true");
    }
}

//left toggle
var toggleNav = function () {
    var navBarCollapsed = getCookie("navBarCollapsed");
    if (navBarCollapsed == "false") {
        setCookie("navBarCollapsed", "true");
    } else {
        setCookie("navBarCollapsed", "false");
    }
}

var setCollapse = function (leftnavbars) {
    var navBarCollapsed = getCookie("navBarCollapsed");
    if (navBarCollapsed == "false") {
        leftnavbars.forEach(function (leftnavbar) {
            leftnavbar.classList.remove("show");
        })
    } else {
        leftnavbars.forEach(function (leftnavbar) {
            leftnavbar.classList.add("show");
        })
    }
}
var toggleElement = document.getElementById("leftnavToggle");
if (toggleElement) {
    toggleElement.onclick = toggleNav;
}

var leftnavbars = document.getElementsByClassName("leftnavbar");
if (leftnavbars) {
    setCollapse(Array.from(leftnavbars));
}

//group toggle
var toggleGroup = function (dataTarget) {
    toggleBoolCookie("group-collapse-" + dataTarget);
}

var groupTogglers = document.getElementsByClassName("ra-nav-group-header");
for (var i = 0; i < groupTogglers.length; i++) {
    let dataTarget = groupTogglers[i].attributes["data-target"].value.substring(1);
    let dataTargetDOM = document.getElementById(dataTarget);

    if (getCookie("group-collapse-" + dataTarget) == "true") {
        groupTogglers[i].setAttribute("aria-expanded", true);
        dataTargetDOM.classList.add("show");
    }

    groupTogglers[i].onclick = function () {
        toggleGroup(dataTarget);
    }
}
