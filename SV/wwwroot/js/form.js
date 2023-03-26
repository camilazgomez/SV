const addSellerBtn = document.querySelector("#add-seller");
addSellerBtn.addEventListener("click", addSellerFormFields);

function addSellerFormFields() {
    const sellersDiv = document.querySelector(".sellers");
    const newSellerRow = document.querySelector(".seller").cloneNode(true);
    newSellerRow.removeAttribute("hidden");
    sellersDiv.appendChild(newSellerRow);
}

const addHeirBtn = document.querySelector("#add-heir");
addHeirBtn.addEventListener("click", addHeirFormFields)

function addHeirFormFields() {
    const heirsDiv = document.querySelector(".heirs");
    const newHeirRow = document.querySelector(".heir").cloneNode(true);
    newHeirRow.removeAttribute("hidden");
    heirsDiv.appendChild(newHeirRow);
}