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

const form = document.getElementById("form"); 
form.addEventListener("submit", defineHiddenFieldState);

function defineHiddenFieldState(event) {
    const allLastUncreditedOwnershipSellerInputs = document.querySelectorAll('[name="uncreditedOwnershipSeller"]');
    const allLastUncreditedOwnershipSellerHiddenFields = document.querySelectorAll('[name="uncreditedClickedSeller"]');
   
    allLastUncreditedOwnershipSellerInputs.forEach(function (checkbox, indexOfCheckbox) {
        if (checkbox.checked) {
            allLastUncreditedOwnershipSellerHiddenFields[indexOfCheckbox].setAttribute("value", true); 
            
        }
    });

    const allLastUncreditedOwnershipBuyerInputs = document.querySelectorAll('[name="uncreditedOwnershipBuyer"]');
    const allLastUncreditedOwnershipBuyerHiddenFields = document.querySelectorAll('[name="uncreditedClickedBuyer"]');

    allLastUncreditedOwnershipBuyerInputs.forEach(function (checkbox, indexOfCheckbox) {
        if (checkbox.checked) {
            allLastUncreditedOwnershipBuyerHiddenFields[indexOfCheckbox].setAttribute("value", true);

        }
    });
   
    
}

window.onload = function () {
    addSellerFormFields();
    addBuyerFormFields();
    
}


