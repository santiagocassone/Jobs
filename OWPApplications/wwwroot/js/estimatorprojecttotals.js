$(document).ready(function () {

    // disable tax when vendor is BAN01
    $('#vendorNo').on('change', function () {
        if ($("#vendorNo option:selected").text() == "BAN01") {
            $('#taxable').attr('readonly', true);
        } else {
            $('#taxable').attr('readonly', false);
        }
    });

    if ($('#laborQuoteStatus').find('option:selected').html() == 'Canceled') {
        document.querySelectorAll('fieldset.canceledFieldset')[0].disabled = false;
        let list = $('.canceledFieldset')[0].elements;
        for (let item of list) {
            item.disabled = true;
            if (item.id == 'laborQuoteStatus' || item.id == 'btnSave') {
                item.disabled = false;
            }
        }

    }
    SetFurnitureTotalHs();
    $(document).on("change", ".furnitureQty", function () {
        var tr = $(this).parent().parent().closest('tr');
        var qty = $(this).val();
        var hsxqty = $(tr).find('.furnitureHsXQty').val();
        if (qty && hsxqty) {
            var inputHs = $(tr).find('.furnitureHs');
            $(inputHs).val(Number(qty) * Number(hsxqty));
            SetFurnitureTotalHs();
        }
    });
    $(document).on("change", ".furnitureHsXQty", function () {
        var tr = $(this).parent().parent().closest('tr');
        var qty = $(tr).find('.furnitureQty').val();
        var hsxqty = $(this).val();
        if (qty && hsxqty) {
            var inputHs = $(tr).find('.furnitureHs');
            $(inputHs).val(Number(qty) * Number(hsxqty));
            SetFurnitureTotalHs();
        }
    });
    $(document).on("change", ".furnitureHs", function () {
        var tr = $(this).parent().parent().closest('tr');
        var qty = $(tr).find('.furnitureQty').val();
        var hs = $(this).val();
        if (qty && hs) {
            var inputHsXQty = $(tr).find('.furnitureHsXQty');
            $(inputHsXQty).val(Number(hs) / Number(qty));
        }
        SetFurnitureTotalHs();
    });

    SetAddonsTotalHs();
    $('.addOnsHs').change(function () {
        SetAddonsTotalHs();
    });

    SetMiscChargesTotalDlls();
    $('.miscChargesQty').change(function () {
        var tr = $(this).parent().parent().closest('tr');
        var qty = $(this).val();
        var rate = $(tr).find('.miscChargesRate').val();
        if (qty && rate) {
            var inputDlls = $(tr).find('.miscChargesDlls');
            $(inputDlls).val(Number(qty) * Number(rate));
            SetMiscChargesTotalDlls();
        }
    });
    $('.miscChargesRate').change(function () {
        var tr = $(this).parent().parent().closest('tr');
        var qty = $(tr).find('.miscChargesQty').val();
        var rate = $(this).val();
        if (qty && rate) {
            var inputDlls = $(tr).find('.miscChargesDlls');
            $(inputDlls).val(Number(qty) * Number(rate));
            SetMiscChargesTotalDlls();
        }
    });
    $('.miscChargesDlls').change(function () {
        var tr = $(this).parent().parent().closest('tr');
        var qty = $(tr).find('.miscChargesQty').val();
        var dlls = $(this).val();
        if (qty && dlls) {
            var inputRate = $(tr).find('.miscChargesRate');
            $(inputRate).val(Number(dlls) / Number(qty));
        }
        SetMiscChargesTotalDlls();
    });

    SetTotalHs();
    SetSurchargeValues();
    $('.hs, .rates, #surchargePct').change(function () {
        SetTotalHs();
        SetSurchargeValues();
    });

    SetInstallTotal();
    $('#complexProject').change(function () {
        SetInstallTotal();
    });

    SetCommonTotalNoDays();
    $('#noOfDays').change(function () {
        SetCommonTotalNoDays();
    });

    $('#surchargePct').val((getNumber($('#surchargePct').val()).toFixed()).toString() + '%');
    $('#surchargePct').click(function () {
        $(this).val($(this).val().replace('%', ''));
    });
    $('#surchargePct').blur(function () {
        $(this).val((getNumber($(this).val()).toFixed()).toString() + '%');
    });

    if ($('#hdnEstimSuccess').val() != "") {
        if ($('#hdnEstimSuccess').val() == 'success') {
            alert('The estimation has been done successfully!');
        } else if ($('#hdnEstimSuccess').val() == 'error') {
            alert('There was an error estimating the Labor Quote.');
        }
    }

    if ($('#hdnEstimSaved').val() != "") {
        if ($('#hdnEstimSaved').val() == 'saved') {
            alert('The estimation has been saved successfully!');
        } else if ($('#hdnEstimSaved').val() == 'error') {
            alert('There was an error saving the Labor Quote.');
        }
    }

    $('#btnCancel').click(function (e) {
        e.preventDefault();
        $('#myModal').css('display', 'block');
    });

    $('.close').click(function () {
        $('#myModal').css('display', 'none');
    });

    $('#btnCancelYes').click(function () {
        var quoteno = $('#quoteNo').val();
        var region = $('#regionFiltered').val();
        let btn = $(this);
        btn.prop("disabled", true);
        btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');
        $.ajax({
            type: "POST",
            url: '/estimatorprojecttotals/cancelquote',
            data: { quoteNo: quoteno, region: region }
        }).done(function () {
            location.replace("/estimatorprojecttotals/index");
        }).fail(function () {
            alert("There was a problem cancelling the quote.")
            btn.prop("disabled", false);
            btn.html('Yes');
            $('#myModal').css('display', 'none');
        });
    });

    $('#btnCancelNo').click(function () {
        $('#myModal').css('display', 'none');
    });

    var isAttached = $('#hdnIsAttached').val();
    if (isAttached) {
        $('#notes').attr('rows', '16');
        $('#divAttchs').css('height', '80px');
    }

    $('.deleteAttachment').click(function () {
        var file = $(this).data('delatt');

        $.ajax({
            type: "POST",
            url: '/estimatorprojecttotals/deleteattachmentfile',
            data: { attFile: file }
        }).done(function () {
            location.reload();
        }).fail(function () {
            alert("There was a problem deleting the file.")
        });
    });

    $('#addFurnitureItem').click(function () {
        var num = (Number($('#hdnNumNewFurns').val()) + 1).toString();
        $('#hdnNumNewFurns').val(num);

        var html = '<tr id="' + num + '">'
            + '<td width="400px"><input class="form-control form-rounded mt-2 newfurns" type="text" onblur="saveFurnsValues(this)" name="newfurndesc" style="width:90%" /></td>'
            + '<td width="150px"><input class="form-control form-rounded mt-2 newfurns furnitureQty" type="number" step="0.01" onblur="saveFurnsValues(this)" name="newfurnqty" /></td>'
            + '<td width="150px"><input class="form-control form-rounded mt-2 newfurns furnitureHsXQty" type="number" step="0.01" onblur="saveFurnsValues(this)" name="newfurnhsxqty" /></td>'
            + '<td width="150px"><input class="form-control form-rounded mt-2 newfurns furnitureHs" type="number" step="0.01" onblur="saveFurnsValues(this)" name="newfurnhs" /></td>'
            + '<td><span class="remFurnitureItem" data-row=' + num + ' onclick="removeFurn(this)">[x]</span></td>'
            + '</tr>';

        $('#lastFurnTr').before(html);
    });

    $('#btnSave').click(function (evt) {
        evt.preventDefault();
        $(this).prop("disabled", true);
        $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');
        saveQuote('Save');
    });

    $('#btnEstimate').click(function (evt) {
        evt.preventDefault();
        $(this).prop("disabled", true);
        $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');
        saveQuote('Estimate');
    });
});

function saveQuote(submit) {
    var lqCode = parseInt($('input[name="laborQuoteNo"]').val());
    $.ajax({
        type: "POST",
        url: '/estimatorprojecttotals/' + submit + 'Quote',
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        cache: false,
        data: getQuoteData()
    }).done(function (response) {
        if (response) {
            $('#btn' + submit).prop("disabled", false);
            $('#btn' + submit).html(submit == "Save" ? "Save" : "Complete Estimate");
            location.replace("/estimatorprojecttotals/index?lqCode=" + lqCode + "&region=" + $('#regionFiltered').val());
        } else {
            location.replace("/home/error?msg=savinglaborquote");
        }
    }).fail(function (error) {
        location.replace("/home/error?msg=savinglaborquote");
    });
}

function getQuoteData() {
    var newFurns = [];
    var arr = $(".hdnfurnval");
    $(arr).each(function () {
        newFurns.push($(this).val());
    });

    let hoursAndRates = [];
    let count = parseInt($('#hrCount').val());
    for (var i = 0; i < count; i++) {
        hoursAndRates.push({
            'Type': $('#hsType-' + i).val(),
            'Hours': getNumber($('#Hs-' + i).val()),
            'Rate': getNumber($('#HourlyRate-' + i).val())
        });
    }

    var data = [];
    data.push({
        'Address': $('input[name="address"]').val(),
        'City': $('input[name="city"]').val(),
        'State': $('input[name="state"]').val(),
        'Zip': $('input[name="zip"]').val(),
        'LaborQuoteNo': $('#laborQuoteNo_').val(),
        'Scope': $('#scope').val(),
        'Equipments': $('#equipments').val(),
        'VendorNo': $('#vendorNo').find('option:selected').val(),
        'Taxable': $('#taxable').find('option:selected').val(),
        'Notes': $('#notes').val(),
        'EstimatedBy': $('#estimatedBy').val(),
        'CustomerName': $('#customerName').val(),
        'ComplexProject': $('#complexProject').find('option:selected').val(),
        'NoOfDays': $('#noOfDays').val(),
        'InstallTotal': $('#installTotal').val(),
        'HsSurcharge': $('#hsSurcharge').val(),
        'SurchargePct': $('#surchargePct').val(),
        'RegHs': $('#regHs').val(),
        'OtHs': $('#otHs').val(),
        'DtHs': $('#dtHs').val(),
        'RegHourlyRate': $('#regHourlyRate').val(),
        'OtHourlyRate': $('#otHourlyRate').val(),
        'DtHourlyRate': $('#dtHourlyRate').val(),
        'LaborQuoteStatus': $('#laborQuoteStatus').find('option:selected').val(),
        'CurrLaborQuoteStatus': $('#currLaborQuoteStatus').val(),
        'TruckCap': $('#truckCap').find('option:selected').val(),
        'ProjectId': $('#projectId').val(),
        'QuoteNo': $('#quoteOrOrderNo').val(),
        'SystemQty': $('input[name="systemQty"]').val(),
        'SystemHsXQty': $('input[name="systemHsXQty"]').val(),
        'SystemHs': $('input[name="systemHs"]').val(),
        'DeskAndTablesQty': $('input[name="deskAndTablesQty"]').val(),
        'DeskAndTablesHsXQty': $('input[name="deskAndTablesHsXQty"]').val(),
        'DeskAndTablesHs': $('input[name="deskAndTablesHs"]').val(),
        'StorageAndMiscQty': $('input[name="storageAndMiscQty"]').val(),
        'StorageAndMiscHsXQty': $('input[name="storageAndMiscHsXQty"]').val(),
        'StorageAndMiscHs': $('input[name="storageAndMiscHs"]').val(),
        'SeatingQty': $('input[name="seatingQty"]').val(),
        'SeatingHsXQty': $('input[name="seatingHsXQty"]').val(),
        'SeatingHs': $('input[name="seatingHs"]').val(),
        'StairDistStagingQty': $('input[name="stairDistStagingQty"]').val(),
        'StairDistStagingHs': $('input[name="stairDistStagingHs"]').val(),
        'WallChannelDrywallQty': $('input[name="wallChannelDrywallQty"]').val(),
        'WallChannelDrywallHs': $('input[name="wallChannelDrywallHs"]').val(),
        'WallChannelConcreteQty': $('input[name="wallChannelConcreteQty"]').val(),
        'WallChannelConcreteHs': $('input[name="wallChannelConcreteHs"]').val(),
        'GrommetHoleCutsQty': $('input[name="grommetHoleCutsQty"]').val(),
        'GrommetHoleCutsHs': $('input[name="grommetHoleCutsHs"]').val(),
        'WorksurfaceCutsQty': $('input[name="worksurfaceCutsQty"]').val(),
        'WorksurfaceCutsHs': $('input[name="worksurfaceCutsHs"]').val(),
        'DeliveryTimeQty': $('input[name="deliveryTimeQty"]').val(),
        'DeliveryTimeHs': $('input[name="deliveryTimeHs"]').val(),
        'WarehouseQty': $('input[name="warehouseQty"]').val(),
        'WarehouseHs': $('input[name="warehouseHs"]').val(),
        'NewLabor1': $('input[name="newLabor1"]').val(),
        'NewLabor1Qty': $('input[name="newLabor1Qty"]').val(),
        'NewLabor1Hs': $('input[name="newLabor1Hs"]').val(),
        'NewLabor2': $('input[name="newLabor2"]').val(),
        'NewLabor2Qty': $('input[name="newLabor2Qty"]').val(),
        'NewLabor2Hs': $('input[name="newLabor2Hs"]').val(),
        'NewLabor3': $('input[name="newLabor3"]').val(),
        'NewLabor3Qty': $('input[name="newLabor3Qty"]').val(),
        'NewLabor3Hs': $('input[name="newLabor3Hs"]').val(),
        'NewLabor4': $('input[name="newLabor4"]').val(),
        'NewLabor4Qty': $('input[name="newLabor4Qty"]').val(),
        'NewLabor4Hs': $('input[name="newLabor4Hs"]').val(),
        'Truck': $('input[name="truck"]').val(),
        'TruckQty': $('input[name="truckQty"]').val(),
        'TruckRate': $('input[name="truckRate"]').val(),
        'MileageParking': $('input[name="mileageParking"]').val(),
        'Supplies': $('input[name="supplies"]').val(),
        'LoadCrew': $('input[name="loadCrew"]').val(),
        'LoadCrewQty': $('input[name="loadCrewQty"]').val(),
        'LoadCrewRate': $('input[name="loadCrewRate"]').val(),
        'Disposal': $('input[name="disposal"]').val(),
        'NewMisc1': $('input[name="newMisc1"]').val(),
        'NewMisc1Dollars': $('input[name="newMisc1Dollars"]').val(),
        'NewMisc2': $('input[name="newMisc2"]').val(),
        'NewMisc2Dollars': $('input[name="newMisc2Dollars"]').val(),
        'NewMisc3': $('input[name="newMisc3"]').val(),
        'NewMisc3Dollars': $('input[name="newMisc3Dollars"]').val(),
        'NewMisc4': $('input[name="newMisc4"]').val(),
        'NewMisc4Dollars': $('input[name="newMisc4Dollars"]').val(),
        'NewMisc5': $('input[name="newMisc5"]').val(),
        'NewMisc5Dollars': $('input[name="newMisc5Dollars"]').val(),
        'NewMisc6': $('input[name="newMisc6"]').val(),
        'NewMisc6Dollars': $('input[name="newMisc6Dollars"]').val(),
        'CommonTotalDlls': getNumber($('input[name="commonTotalDlls"]').val()),
        'HoursAndRates': hoursAndRates,
        'NewFurnItems': newFurns,
        'Region': $('#regionFiltered').val(),
        'LaborQuoteHeaderID': $('#lqHeaderID').val(),
        'IsScheduled': $('input[name="IsScheduled"]')[0].checked
    });

    var form = new FormData();
    form.append('rawdata', JSON.stringify(data));
    $.each($('input[name="fileAttachment"]')[0].files, function (i, file) {
        form.append('files', file);
    });

    return form;
}

function addAttFile() {
    let id = $('.fileAttDiv').last().attr('id').split('-');
    $("<div class='input-group fileAttDiv' id='fileAttDiv-" + (parseInt(id[1]) + 1) + "'><input class='form-control form-rounded mt-2' style='height: 44px;' type='file' name='fileAttachment' /><button type='button' class='btn btn-danger btn-sm fas fa-trash' style='margin-top: 8px;' onclick='removeAttFile(" + (parseInt(id[1]) + 1) + ")'></button></div>").insertAfter('#fileAttDiv-' + id[1]);
}

function removeAttFile(id) {
    $('#fileAttDiv-' + id).remove();
}

function removeFurn(elem) {
    var rowNum = $(elem).data('row');
    $('tr#' + rowNum).remove();
    $('#hdnfurnval' + rowNum).remove();
    SetFurnitureTotalHs();
}

function saveFurnsValues(elem) {
    var num = $(elem).parent().parent().closest('tr').attr('id');
    var tds = $(elem).parent().closest('td').siblings().addBack();
    var desc = $(tds).find('input[name="newfurndesc"]').val();
    var qty = $(tds).find('input[name="newfurnqty"]').val();
    var hsxqty = $(tds).find('input[name="newfurnhsxqty"]').val();
    var hs = $(tds).find('input[name="newfurnhs"]').val();
    var hdnVal = desc + '|' + qty + '|' + hsxqty + '|' + hs;
    if ($('#hdnfurnval' + num).length)
        $('#hdnfurnval' + num).val(hdnVal);
    else
        $('#hdnFurnsContainer').append('<input type="hidden" id="hdnfurnval' + num + '" class="hdnfurnval" value="' + hdnVal + '" />');

    SetFurnitureTotalHs();
}

function SetProjectTotalHours() {
    var sum = 0;
    $('.totalHs').each(function () {
        sum += getNumber($(this).val());
    });
    $('input[name="projTotalHs"]').val(sum);
}

function SetHoursValidation() {
    var projTotalHs = getNumber($('input[name="projTotalHs"]').val());
    var totalHs = getNumber($('input[name="totalHs"]').val());
    var hsValidationValue = totalHs - projTotalHs;
    $('input[name="hsValidation"]').val(hsValidationValue);
    if (hsValidationValue != 0) {
        $('input[name="hsValidation"]').css('color', 'red');
    } else {
        $('input[name="hsValidation"]').css('color', 'black');
    }
}

function SetTotalLaborCosts() {
    let count = parseInt($('#hrCount').val());
    let hs;
    let rate;
    let total = 0;
    for (var i = 0; i < count; i++) {
        rate = getNumber($('#HourlyRate-' + i).val());
        hs = getNumber($('#Hs-' + i).val());
        total += rate * hs;
    }
    $('input[name="totalLaborCosts"').val('$' + parseFloat(total.toFixed(3)).toLocaleString());
}

function SetInstallTotal() {
    var complexProject = $('#complexProject').find(':selected').val();
    var miscTotalDollars = getNumber($('input[name="miscTotalDollars"]').val());
    var totalLaborCosts = getNumber($('input[name="totalLaborCosts"]').val());

    if (complexProject === '0') {
        $('#installTotal').val('$' + parseFloat((miscTotalDollars + totalLaborCosts).toFixed(3)).toLocaleString());
    } else {
        $('#installTotal').val('See scope above for Breakdown');
        $('#installTotal').attr('title', 'See scope above for Breakdown.');
    }
}

function SetHealthSafetySurcharge() {
    var totalLaborCost = getNumber($('input[name="totalLaborCosts"]').val());
    var surchargePct = getNumber($('#surchargePct').val());
    $('#hsSurcharge').val('$' + (Math.round(totalLaborCost * surchargePct / 100)));
}

function SetBreakdownLaborTotal() {
    $('input[name="commonTotalDlls"]').val($('input[name="totalLaborCosts"]').val());
}

function SetBreakdownTotalHs() {
    var laborTotal = getNumber($('input[name="commonTotalDlls"]').val());
    $('.brkType').each(function () {
        $('input[name="TotalHs-' + this.value + '"]').val(parseFloat((laborTotal / getNumber($('input[name="HourlyRate-' + this.value + '"]').val())).toFixed(3)).toLocaleString());
    })
}

function SetTotalManHoursPerDay() {
    var commonTotalNoDays = Number($('input[name="commonTotalNoDays"]').val());
    $('.brkType').each(function () {
        $('input[name="TotalManHs-' + this.value + '"]').val(commonTotalNoDays === 0 ? '0' : parseFloat((getNumber($('input[name="TotalHs-' + this.value + '"]').val()) / commonTotalNoDays).toFixed(3)).toLocaleString());
    })
}

function SetNumberOfMenPerDay() {
    $('.brkType').each(function () {
        $('input[name="TotalNoOfMen-' + this.value + '"]').val(parseFloat((getNumber($('input[name="TotalManHs-' + this.value + '"]').val()) / 8).toFixed(3)).toLocaleString());
    })
}

function SetSurchargeValues() {
    let count = parseInt($('#hrCount').val());
    let surcharge;
    let rate;
    for (var i = 0; i < count; i++) {
        rate = getNumber($('#HourlyRate-' + i).val());
        surcharge = parseFloat((rate * getNumber($('#surchargePct').val()) / 100).toFixed(2));
        $('input[name="surcharge-' + i + '"]').val('$' + surcharge.toLocaleString());
        $('input[name="surchargeDlls-' + i + '"]').val('$' + parseFloat(Math.round(surcharge * rate).toFixed(3)).toLocaleString());
    }
    SetHealthSafetySurcharge();
}

function SetFurnitureTotalHs() {
    var sum = 0;
    $('.furnitureHs').each(function () {
        sum += Number($(this).val());
    });
    $('input[name="furnitureTotalHs"]').val(sum.toLocaleString());

    SetProjectTotalHours();
    SetHoursValidation();
}

function SetAddonsTotalHs() {
    var sum = 0;
    $('.addOnsHs').each(function () {
        sum += getNumber($(this).val());
    });
    $('input[name="addOnsTotalHs"]').val(sum.toLocaleString());

    SetProjectTotalHours();
    SetHoursValidation();
}

function SetMiscChargesTotalDlls() {
    var sum = 0;
    $('.miscChargesDlls').each(function () {
        sum += Number($(this).val());
    });
    $('input[name="miscTotalDollars"]').val('$' + parseFloat(sum.toFixed(3)).toLocaleString());

    SetInstallTotal();
}

function SetTotalHs() {
    var sum = 0;
    $('.hs').each(function () {
        sum += Number($(this).val());
    });
    $('input[name="totalHs"]').val(sum);

    SetHoursValidation();
    SetTotalLaborCosts();
    SetInstallTotal();
    SetBreakdownLaborTotal();
    SetBreakdownTotalHs();
    SetTotalManHoursPerDay();
    SetNumberOfMenPerDay();
}

function SetCommonTotalNoDays() {
    $('input[name="commonTotalNoDays"]').val($('#noOfDays').val());
    SetTotalManHoursPerDay();
    SetNumberOfMenPerDay();
}

function GenerateEstTot() {

    $('#pdfButton').html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');
    var quote = {
        "quoteno": $("input[name=labqcode]").val(),
        "min": $('#hdnIsReadOnly').val(),
        "region": $('#regionFiltered').val()
    };
    $.ajax({
        type: "POST",
        url: '/EstimatorProjectTotals/EstProjTotPDF',
        data: JSON.stringify(quote),
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
    }).done(function (result) {
        if (result) {
            window.open('../files/EstProjTot' + result + '.pdf', '_blank');
            $('#pdfButton').html('Generate PDF');
        } else {
            location.replace("/home/error?msg=sendingpdf");
        }
    }
    ).fail(function (error) {
        location.replace("/home/error?msg=processingpdf");
    });
}

function getNumber(value) {
    value = value.replace('$', '').replace('%', '');
    return +value.split(',').join('');
}