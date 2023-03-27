const addSellerBtn = document.querySelector("#add-seller");
addSellerBtn.addEventListener("click", addSellerFormFields);

function addSellerFormFields() {
    const sellersDiv = document.querySelector(".sellers");
    const newSellerRow = document.querySelector(".seller").cloneNode(true);
    newSellerRow.removeAttribute("hidden");
    sellersDiv.appendChild(newSellerRow);

    const allLastUncreditedOwnershipSellerInputs = document.querySelectorAll('[name="uncreditedOwnershipSeller"]');
    const lastUncreditedOwnershipSellersCount = allLastUncreditedOwnershipSellerInputs.length;
    const lastUncreditedOwnershipSeller = allLastUncreditedOwnershipSellerInputs[lastUncreditedOwnershipSellersCount - 1];
    const newUncreditedOwnershipSellerId = "uncreditedOwnershipSeller-" + (lastUncreditedOwnershipSellersCount - 1).toString();
    lastUncreditedOwnershipSeller.setAttribute("id", newUncreditedOwnershipSellerId);
}

const addBuyerBtn = document.querySelector("#add-buyer");
addBuyerBtn.addEventListener("click", addBuyerFormFields);

function addBuyerFormFields() {
    const heirsDiv = document.querySelector(".buyers");
    const newHeirRow = document.querySelector(".buyer").cloneNode(true);
    newHeirRow.removeAttribute("hidden");
    heirsDiv.appendChild(newHeirRow);

    const allLastUncreditedOwnershipBuyersInputs = document.querySelectorAll('[name="uncreditedOwnershipBuyer"]');
    const lastUncreditedOwnershipBuyersCount = allLastUncreditedOwnershipBuyersInputs.length;
    const lastUncreditedOwnershipBuyer = allLastUncreditedOwnershipBuyersInputs[lastUncreditedOwnershipBuyersCount - 1];
    const newUncreditedOwnershipBuyerId = "uncreditedOwnershipBuyer-" + (lastUncreditedOwnershipBuyersCount - 1).toString();
    lastUncreditedOwnershipBuyer.setAttribute("id", newUncreditedOwnershipBuyerId);
}

window.onload = function () {
    addSellerFormFields();
    addBuyerFormFields();
}