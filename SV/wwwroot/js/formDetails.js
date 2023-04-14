
const cne = (document.querySelector("#natureOfTheDeed").innerText).split(" ");
if (cne[cne.length - 1] == "Patrimonio") {
    console.log("aui")
    if (!document.querySelector(".sellers").classList.contains("is-hidden")) {
        console.log("entre")
        document.querySelector(".sellers").classList.toggle("is-hidden")
    }

}
else if (cne[0] === "Compraventa") {
    console.log("en cv")
    if (document.querySelector(".sellers").classList.contains("is-hidden")) {
        console.log("if")
        document.querySelector(".sellers").classList.toggle("is-hidden")
    }
}