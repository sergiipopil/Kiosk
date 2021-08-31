// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function backToMainPage() {
    window.location.href = '/';
}
function openDetails(advertId, advertPrice) {
    window.open('/Product/Details/' + "?id=" + advertId + "&price=" + advertPrice, "_self");
}
var getUrlParameter = function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return typeof sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
    return false;
};
function submit() {
    $('#divSelectTopCategory').hide();
    $('#divCarManufactures').empty();
    $('#divCarManufactures').html('<div style="text-align: center; padding-top: 30%; display: block;"><img src="/images/load.gif" width="100px" /></div>');
    $.ajax({
        type: "GET",
        url: '@Url.Action("PartNumberInput", "Home")' + "?partNumber=" + $("#inputPartValue").val(),
        success: function (data) {
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
        }
    });
}
function topCategorySelected() {
    ViewData["Title"] = "Home Page";
}
function back() {
    showMainView();
    $("input#inputPartValue").val("");
    $('#divSearchByPartNumber').show();
    $('#divBreadcrumbs').hide();
    $('#divSelectTopCategory').show();
}
function BreadCrumbsHistory(manufacture, model, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, subChildId, subChildName) {
    $('#divBreadcrumbsItems').empty();
    $('#divBreadcrumbs').show();
    $('#divSearchByPartNumber').hide();
    $('#divSelectTopCategory').hide();

    $('#divBreadcrumbsItems').append(`<div id="divBreadcrumbsManufacture" onclick="selectManufacturer('${manufacture}')"><div class="parentDivBreadCrumbs">
        <div style="min-width:155px;min-height:115px;text-align:center;"><img src="/images/Catalog/Model_Logo/${manufacture}.webp" class="manufactureImageBreadcrumbs" /></div></div></div>`);
    if (model != null) {

        $('#divBreadcrumbsItems').append(`<div id="divBreadcrumbsModel" class="parentDivBreadCrumbs" onclick="selectCarModel('${manufacture}', '${model}')" style="display:grid;">
                <div style="min-width:155px;min-height:100px;max-height:100px;text-align:center;"><img src="/images/Catalog/CarModel/${manufacture}/${model}.png" class="manufactureImageBreadcrumbs" /></div><div class="manufactureTextBreadcrumbs"><span style="color:black;">${model}</span>
                            </div></div>`);
    }
    if (mainCategoryName != null) {
        $('#divBreadcrumbsItems').append(`<div id="divBreadcrumbsMainCategory" class="parentDivBreadCrumbs" onclick="selectMainCategory('${manufacture}', '${model}', '${mainCategoryId}', '${mainCategoryName}')" style="display:grid;">
                <div style="min-width:155px;min-height:90px;text-align:center;"><img src="/images/Catalog/AutoParts/${mainCategoryId}.png" class="manufactureImageCategoryBreadcrumbs" /></div><div class="manufactureTextBreadcrumbs"><span style="color:black;">${mainCategoryName}</span>
                            </div></div>`);
    }
    if (subCategoryName != null) {
        $('#divBreadcrumbsItems').append(`<div id="divBreadcrumbsSubCategory" class="parentDivBreadCrumbs" onclick="selectSubMainCategory('${manufacture}', '${model}', '${mainCategoryId}', '${mainCategoryName}', '${subCategoryId}', '${subCategoryName}')" style="display:grid;">
                <div style="min-width:155px;min-height:90px;text-align:center;"><img src="/images/Catalog/AutoParts/${subCategoryId}.png" class="manufactureImageCategoryBreadcrumbs" /></div><div class="manufactureTextBreadcrumbs"><span style="color:black;">${subCategoryName}</span>
                            </div></div>`);
    }
    if (subChildName != null) {
        $('#divBreadcrumbsItems').append(`<div id="divBreadcrumbsSubChildCategory" class="parentDivBreadCrumbs" style="display:grid;">
                <div style="min-width:155px;min-height:90px;text-align:center;"><img src="/images/Catalog/AutoParts/${subChildId}.png" class="manufactureImageCategoryBreadcrumbs" /></div><div class="manufactureTextBreadcrumbs"><span style="color:black;">${subChildName}</span>
                            </div></div>`);
    }
}
function onAjaxFailure(xhr, status, error) {
    $("#targetElement").html("<strong>An error occurred retrieving data:" + error + "<br/>.</strong>");
}
function onAjaxSuccess(data, status, xhr) {
    if (!$.trim(data)) {
        $("#targetElement").html("<div class='text-center'><strong>No results found for search.</strong></div>");
    }
}
function selectCarModel(manufacture, model) {
    $('#divSelectTopCategory').hide();
    $.ajax({
        type: "GET",
        url: '@Url.Action("ShowCategoryAutoParts", "Home")' + "?carManufactureName=" + manufacture + "&carModel=" + model,
        success: function (data) {
            BreadCrumbsHistory(manufacture, model);
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
            window.history.pushState($('#filter'), 'title', "?carManufactureName=" + manufacture + "&carModel=" + model);
        }
    });
    return false;
}
function selectMainCategory(manufacture, model, mainCategoryId, mainCategoryName) {
    $('#divSelectTopCategory').hide();
    $.ajax({
        type: "GET",
        url: '@Url.Action("ShowMainSubcategories", "Home")' + "?carManufactureName=" + manufacture + "&carModel=" + model + "&mainCategoryId=" + mainCategoryId + "&mainCategoryName=" + mainCategoryName,
        success: function (data) {
            BreadCrumbsHistory(manufacture, model, mainCategoryId, mainCategoryName);
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
            window.history.pushState($('#filter'), 'title', "?carManufactureName=" + manufacture + "&carModel=" + model + "&mainCategoryId=" + mainCategoryId + "&mainCategoryName=" + mainCategoryName);
        }
    });
}
function selectSubMainCategory(manufacture, model, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName) {
    $('#divCarManufactures').empty();
    $('#divCarManufactures').html('<div style="text-align: center; padding-top: 30%; display: block;"><img src="/images/load.gif" width="100px" /></div>');

    $('#divSelectTopCategory').hide();
    $.ajax({
        type: "GET",
        url: '@Url.Action("ShowMainSubChildsCategories", "Home")' + "?carManufactureName=" + manufacture + "&carModel=" + model + "&mainCategoryId=" + mainCategoryId + "&mainCategoryName=" + mainCategoryName +
            "&subCategoryId=" + subCategoryId + "&subCategoryName=" + subCategoryName,
        success: function (data) {
            console.log("start");
            BreadCrumbsHistory(manufacture, model, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName);
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
            window.history.pushState($('#filter'), 'title', "?carManufactureName=" + manufacture + "&carModel=" + model + "&mainCategoryId=" + mainCategoryId + "&mainCategoryName=" + mainCategoryName +
                "&subCategoryId=" + subCategoryId + "&subCategoryName=" + subCategoryName);
        }
    });
}
function getAllegroData(manufacture, model, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, subChildId, subChildName) {
    $('#divSelectTopCategory').hide();
    $('#divCarManufactures').empty();
    $('#divCarManufactures').html('<div style="text-align: center; padding-top: 30%; display: block;"><img src="/images/load.gif" width="100px" /></div>');
    $.ajax({
        type: "GET",
        url: '@Url.Action("ShowProductList", "Home")' + "?carManufactureName=" + manufacture + "&carModel=" + model + "&mainCategoryId=" + mainCategoryId + "&mainCategoryName=" + mainCategoryName +
            "&subCategoryId=" + subCategoryId + "&subCategoryName=" + subCategoryName + "&subChildId=" + subChildId + "&subChildName=" + subChildName,
        success: function (data) {
            BreadCrumbsHistory(manufacture, model, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, subChildId, subChildName);
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
            window.history.pushState($('#filter'), 'title', "?carManufactureName=" + manufacture + "&carModel=" + model + "&mainCategoryId=" + mainCategoryId + "&mainCategoryName=" + mainCategoryName +
                "&subCategoryId=" + subCategoryId + "&subCategoryName=" + subCategoryName + "&subChildId=" + subChildId + "&subChildName=" + subChildName);
        }
    });
}
function FilteredList(state, sorting, page) {
    $('#divCarManufactures').empty();
    $('#divCarManufactures').html('<div style="text-align: center; padding-top: 30%; display: block;"><img src="/images/load.gif" width="100px" /></div>');
    $.ajax({
        type: "GET",
        url: '@Url.Action("FilteredList", "Home")' + "?state=" + state + "&sorting=" + sorting + "&page=" + page,
        success: function (data) {
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
        }
    });
}

function selectManufacturer(carManufactureName) {
    $.ajax({
        type: "GET",
        url: '@Url.Action("SelectManufactureAndModel", "Home")' + "?carManufactureName=" + carManufactureName,
        success: function (data) {
            BreadCrumbsHistory(carManufactureName);
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
            window.history.pushState($('#filter'), 'title', "?carManufactureName=" + carManufactureName);
        }
    });
    return false;
}

function goToCart(selectedProductId) {
    window.open('/Product/CartView/' + "?selectedProductId=" + selectedProductId);
}
function showMainView(topCategoryId) {
    $('div[id*="divTopCategory_"]').removeClass("selected");
    $('#divTopCategory_' + topCategoryId).addClass("selected");
    $.ajax({
        type: "GET",
        url: '@Url.Action("ShowMainView", "Home")' + "?topCategoryId=" + topCategoryId,
        success: function (data) {
            $('#divCarManufactures').empty();
            $('#divCarManufactures').html(data);
        }
    });
}