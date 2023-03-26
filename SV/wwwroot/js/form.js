const addSellerBtn = document.querySelector("#add-seller");
addSellerBtn.addEventListener("click", addSellerFormFields);

function addSellerFormFields() {
    const sellersDiv = document.querySelector(".sellers");
    const newSellerRow = document.querySelector(".seller").cloneNode(true);
    newSellerRow.removeAttribute("hidden");
    sellersDiv.appendChild(newSellerRow);
}

const addBuyerBtn = document.querySelector("#add-buyer");
addBuyerBtn.addEventListener("click", addBuyerFormFields)

function addBuyerFormFields() {
    const heirsDiv = document.querySelector(".buyers");
    const newHeirRow = document.querySelector(".buyer").cloneNode(true);
    newHeirRow.removeAttribute("hidden");
    heirsDiv.appendChild(newHeirRow);
}