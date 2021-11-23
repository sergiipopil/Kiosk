function showme(id, text, textId) {
    var car = document.createElement('i');
    car.classList.add("fa");

    var divid = document.getElementById(id);
    var clicky = document.getElementById(textId);
    if (divid.style.display == 'block') {
        divid.style.display = 'none';
        clicky.innerText = text;
        car.classList.add("fa-arrow-down");
    } else {
        divid.style.display = 'block';
        clicky.innerText = text;
        car.classList.add("fa-arrow-up");
    }
    clicky.appendChild(car);
}