// Seleccionamos el elemento "select" del campo de años
var yearSelect = document.getElementById("year");

// Obtenemos el año actual
var currentYear = new Date().getFullYear();
var selectedYear = document.getElementById("selectedYear").value;

if (selectedYear != 0) {
    var option = document.createElement("option");
    option.value = selectedYear;
    option.text = selectedYear;
    yearSelect.add(option);
}
// Agregamos opciones al select para los años desde 2023 hasta 1900
for (var i = currentYear; i >= 1900; i--) {
    var option = document.createElement("option");
    option.value = i;
    option.text = i;
    yearSelect.add(option);
}

const eraseFilterBtn = document.querySelector("#eraseFilterBtn");

eraseFilterBtn.addEventListener("click", hideTable);

function hideTable() {
    if (!document.querySelector(".table").classList.value.includes('is-hidden')) {
        document.querySelector(".table").classList.toggle("is-hidden");
    }
}