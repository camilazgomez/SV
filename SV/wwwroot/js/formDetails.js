
const cne = (document.querySelector("#natureOfTheDeed").innerText).split(" ");
if (cne[cne.length - 1] == "Patrimonio") {
    if (!document.querySelector(".sellers").classList.contains("is-hidden")) {
        document.querySelector(".sellers").classList.toggle("is-hidden")
    }

}
else if (cne[0] === "Compraventa") {
    if (document.querySelector(".sellers").classList.contains("is-hidden")) {
        document.querySelector(".sellers").classList.toggle("is-hidden")
    }
}


const heirsRows = document.querySelectorAll(".heir-row");

for (let i = 0; i < heirsRows.length; i++) {
    const uncreditedOwnership = heirsRows[i].querySelector("select");

    const uncreditedOwnershipValue = uncreditedOwnership.options[uncreditedOwnership.selectedIndex].value;
    console.log(uncreditedOwnershipValue)

    if (uncreditedOwnershipValue === "true") {
        const uncreditedOwnershipPercentage = heirsRows[i].querySelector(".ownership-percentage");
        uncreditedOwnershipPercentage.innerHTML = "";
    }
}