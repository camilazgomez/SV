// Seleccionamos el elemento "select" del campo de años
var yearSelect = document.getElementById("year");

// Obtenemos el año actual
var currentYear = new Date().getFullYear();

// Agregamos opciones al select para los años desde 2023 hasta 1900
for (var i = currentYear; i >= 1900; i--) {
    var option = document.createElement("option");
    option.value = i;
    option.text = i;
    yearSelect.add(option);
}