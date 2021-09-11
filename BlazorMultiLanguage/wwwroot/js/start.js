// complete tranlation by example. 
// Only hard code at the moment
let s = localStorage.getItem("Language");
let loading = 'Loading...';
let promptError = "An untreated error has occurred.";
let reload = 'Reload';
if (s === 'ES') {
    loading = 'Cargando...';
    promptError = 'Ha ocurrido un error no tratado.'
    reload = 'Recargar';
}
// others...
//
document.getElementById("app").innerHTML = loading;
document.getElementById("prompt-error").innerHTML = promptError;
document.getElementById("reload").innerHTML = reload;