function processAjax(areaVal, cityVal, cbName) {
    $.ajax({
        type: "GET",
        url: '@Url.Action("Delivery", "Product")' + "?area=" + areaVal + '&city=' + cityVal,

        success: function (data) {
            console.log(data);
            var entities = data.cities.length == 0 ? data.departments : data.cities;
            var cities = $('body').find("[data-id='" + cbName + "']").closest('div').find('ul');
            var sel = $('#' + cbName);
            //sel.html('<option value=""> - Choose - </option>');
            cities.html('<li data-original-index="0" class="selected active"><a tabindex="0" class="" data-tokens="null" role="option" aria-disabled="false" aria-selected="true"><span class="text"> - Choose area - </span><span class="glyphicon glyphicon - ok check - mark"></span></a></li>');
            $.each(entities, function (index, item) {
                cities.append(`<li data-original-index="${index + 1}"><a tabindex="0" class="" data-tokens="null" role="option" aria-disabled="false" aria-selected="false"><span class="text">${item.description}</span><span class="glyphicon glyphicon-ok check-mark"></span></a></li>`);
                sel.append(`<option value="${item.description}">${item.description}</option>`);
            });
            sel.selectpicker({
                liveSearch: true,
                showSubtext: true
            });

        }
    });
}
function showDeliveryFields(divName) {
    if (divName == '#nova') {
        $(divName).show();
        $('#courier').hide();

        $('#divBtnNova').addClass('selected');
        $('#divBtnCourier').removeClass('selected');
    }
    else {
        $('#nova').hide();
        $('#courier').show();

        $('#divBtnNova').removeClass('selected');
        $('#divBtnCourier').addClass('selected');

    }
}
function onAreaChanged() {
    var val = $('#cbArea').val();
    console.log(val);
    processAjax(val, '', 'cbCity');
}

function onCityChanged() {
    console.log($('#cbCity').val());
    processAjax('', $('#cbCity').val(), 'cbDepartment');
}



function submit() {
    console.log("submit");
    $.ajax({
        type: "POST",
        url: '@Url.Action("Submit", "Product")',
        //data:"",
        data: { selectedDepartment: $('#cbDepartment').val() },
        success: function (data) {
            Disable(id, data);
            //alert("Cool");
        }
    });
}