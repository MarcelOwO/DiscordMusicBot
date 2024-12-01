function GetLocalStorage(key) {
    return localStorage.getItem(key);
}

function SetLocalStorage(key, value) {
    localStorage.setItem(key, value);
}

function RemoveLocalStorage(key) {
    localStorage.removeItem(key);
}

function ClearLocalStorage() {
    localStorage.clear();
}
