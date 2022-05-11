/* 
 * TODO: Maybe split this file in differents files, one for each app.
 */

let valuesStore = null;

var checkedLoadedLine = false;

$(document).ready(function () {

    $(function () {
        $('[data-toggle="tooltip"]').tooltip();
    });

    $('.datepicker').datepicker({
        defaultDate: new Date(),
        todayHighlight: true,
        autoclose: true
    });

    initForm();

    initFormWarehouse();

    $.validator.addMethod("valueNotEquals", function (value, element, params) {
        console.log(value, element, params);
        return params !== value;
    }, "Please select the vendor!");

    $('#contact-form').validate({
        rules: {
            selectvendor: {
                valueNotEquals: "",
                required: true
            },
            inputTo: {
                required: true,
                email: true
            },
            inputSpecifiedAndLocation: {
                required: true
            },
            inputZipCode: {
                minlength: 2,
                required: true
            },
            inputOrderDate: {
                required: true,
                date: true
            },
            inputYourEmail: {
                required: true,
                email: true
            },
            inputCC1: {
                email: true
            },
            inputCC2: {
                email: true
            }
        },
        messages: {
            inputTo: { valueNotEquals: "Please select the vendor!" }
        }
    });

    // QUOTE INQUIRY //

    $('#inputOrderDate').datepicker({});

    $('#includeBOSwitch').change(function () {
        var from = $(this).data("from");
        if (from === "standardprice") {
            toggleBOSwitchStandardPrice(this);
        } else {
            toggleBOSwitchQuoteInquiry(this);
        }
    });

    $('#inputRedlineOrderDate').datepicker({
        defaultDate: new Date(),
        todayHighlight: true,
        autoclose: true
    });

    $('.inputComment').focusout(function () {
        var quoteNo = $(this).data('quoteno');
        var lineNo = $(this).data('lineno');
        var comment = $(this).val();

        $.ajax({
            type: "POST",
            url: '/home/UpdateQuoteInquiryComment',
            data: { quoteNo: quoteNo, lineNo: lineNo, comment: comment }
        });
    });

    $('#SendEmailQouteInquiry').click(function (evt) {
        // Aborts Form submit
        evt.preventDefault();
        // Check Form validation (cancel email delivery if form does not validates)
        if (!$(evt.target).closest("form").get(0).reportValidity()) {
            return false;
        }
        // Check if some Vendor is selected (cancel email delivery if not)
        var missingEmail = false;
        if ($(".vendors-cb:checked").length == 0) {
            alert("Please select vendor(s) to send email.");
            return false;
        } else {
            var checkedVendors = $(".vendors-cb:checked");
            checkedVendors.each(function () {
                var vendor = $(this).data('vendor');
                if ($("#vendorToAddress_" + vendor + " option:selected").length == 0 && $("#vendorToAddressFreeform_" + vendor).val() == "") {
                    missingEmail = true;
                }
            });
        }
        if (missingEmail) {
            alert("Please select or enter an email address for all selected vendors.");
            return false;
        }

        $(this).prop("disabled", true);
        // add spinner to button
        $(this).html(
            '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...'
        );
        sendEmailQuoteInquiry();
    });

    $('#SendEmailRedline').click(function (evt) {
        evt.preventDefault();

        // Check Form validation (cancel email delivery if form does not validates)
        if (!$(evt.target).closest("form").get(0).reportValidity()) {
            return false;
        }

        $.ajax({
            type: "POST",
            url: '/home/sendredlineemail',
            enctype: 'multipart/form-data',
            processData: false,
            contentType: false,
            cache: false,
            data: getRedlineEmailData()
        }).done(function (result) {
            if (result) {
                ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
                clearFormRedline();
            } else {
                location.replace("/home/error?msg=sending");
            }
        }
        ).fail(function (error) {
            location.replace("/home/error?msg=processing");
        });
    });

    $('input[name="chkQIVendor"]').change(function () {
        if ($('input[name="chkQIVendor"]:checked').length == 1 && $('input[name="chkQIVendor"]:checked').attr('data-fzc')) {
            $('#inputZipCode').prop('required', false);
            $('#inputZipCode').prop('disabled', false);
        } else {
            $('#inputZipCode').prop('required', true);
            $('#inputZipCode').prop('disabled', false);
        }
        if ($(this).is(':checked')) {
            var vendor = $(this).data('vendor');
            if ($("#vendorToAddress_" + vendor + " option").length == 0) {
                alert("Please enter an email address for all selected vendors.");
            }
        }
    });

    $('.vendors-cb').click(function () {
        var checked = ($(this).is(":checked"));
        var vendor = $(this).data('vendor');
        if (checked) {
            $('.picker_' + vendor).show();
            $('#vendorToAddress_' + vendor).show();
            $('#btnAdd-' + vendor).show();
        } else {
            $('#vendorToAddress_' + vendor).hide();
            $('.picker_' + vendor).hide();
            $('#btnAdd-' + vendor).hide();
            $('#vendorToAddressFreeform_' + vendor).hide();
        }
    });

    $('.custom-pickerselect').hide();

    $('#totalList').html('Total List : $' + $('#totalListInput').val());

    // ********************** //

    $('.values-input').focusin(function () {
        valuesStore = $(this).val();
    });

    $('.values-input').focusout(function () {
        var el = $(this);

        let data = {
            OrderNo: el.data('orderno'),
            LineNo: el.data('line'),
            FieldID: el.data('field'),
            Value: el.val(),
            PO: el.data('po'),
            Source: el.data('source'),
            Company: el.data('companycode'),
            KeyString: el.data('keystring'),
            OrderType: el.data('ordertype'),
            Region: el.data('region'),
            LineType: el.data('linetype'),
            ProjectID: el.data('projectid'),
            UpsertLevel: el.data('upsertlevel')
        }

        if (data.Value !== valuesStore) {
            updateValues(data, el);
        }
    });

    $('.values-input-date').change(function () {
        var el = $(this);

        let data = {
            OrderNo: el.data('orderno'),
            LineNo: el.data('line'),
            FieldID: el.data('field'),
            Value: el.val(),
            PO: el.data('po'),
            Source: el.data('source'),
            Company: el.data('companycode'),
            KeyString: el.data('keystring'),
            OrderType: el.data('ordertype'),
            Region: el.data('region'),
            LineType: el.data('linetype'),
            ProjectID: el.data('projectid'),
            UpsertLevel: el.data('upsertlevel')
        }
            updateValues(data, el);
    });

    $('.link-po').click(function () {
        var index = $(this).data('index');
        fastTrackShowDetails(index);
    });

    $('.link-orderno-wdt').click(function () {
        var orderNo = $(this).data('orderno');
        warehouseShowDetails(orderNo);
    });


    $('#gotosummary').click(function () {
        //$('.details-container').hide();
        //$('#summary-container').show();
    });

    $('#inputVendorTo').change(function (ev) {
        var email = $(this).data('email');
        $('#inputTo').val(email);
    });

    $('#inputYourEmail').change(function () {
        $('#inputCC1').val($(this).val());
    });

    setLQACustomerSelectColor();
    $('#selCustomers').change(function () {
        setLQACustomerSelectColor();
    });

    $('#detailsModal').on('hidden.bs.modal', function () {
        if (checkedLoadedLine) {
            $('#wdtContent').css('display', 'none');
            $('#divSavingChanges').css('display', 'block');
            location.reload();
        }
    })

});

// WAREHOUSE DT
function setLoaded(elem) {
    var isLoaded = $(elem).is(':checked');
    var lineNo = $(elem).data('lineno');
    var orderNo = $(elem).data('orderno');

    $.ajax({
        type: "POST",
        url: '/home/UpdateLoadedLine',
        data: { orderNo, lineNo, isLoaded }
    }).done(function (result) {
        if (result === 'Ok') {
            checkedLoadedLine = true;
        } else {
            location.replace("/home/error?msg=sending");
        }
    }).fail(function (error) {
        location.replace("/home/error?msg=processing");
    });
}
// ..

function setLQACustomerSelectColor() {
    var current = $('#selCustomers').val();
    if (current != '') {
        $('#selCustomers').css('color', '#495057');
    } else {
        $('#selCustomers').css('color', 'gray');
    }
}

function ShowLoading() {
    $('#loading').show();
    $('#noresults').hide();

    localStorage.setItem("inputDateFrom", $('#dprDateFrom').val());
    localStorage.setItem("inputDateTo", $('#dprDateTo').val());

    localStorage.setItem("inputSRDateFrom", $('#dateFromStatsReport').val());
    localStorage.setItem("inputSRDateTo", $('#dateToStatsReport').val());
}


function updateValues(data, elem) {
    var cell = elem.parent();
    cell.append('<div id="cell-loading" class="spinner-border spinner-border-sm" role="status"><span class="sr-only">Saving</span></div>');
    $.ajax({
        type: "POST",
        url: '/home/UpdateValues',
        data: data
    }).done(function (result) {
        console.log(result);
        cell.find('#cell-loading').remove();
    }).fail(function (error) {
        console.log(error);
    });
}

function updateComment(val, line_no, order_no, po_suffix, keystring, source, elem) {
    var cell = elem.parent();
    cell.append('<div id="cell-loading_' + line_no + '" class="spinner-border spinner-border-sm" role="status"><span class="sr-only">Saving</span></div>');
    $.ajax({
        type: "POST",
        url: '/home/UpdateComment',
        data: {
            OrderNo: order_no,
            PO: po_suffix,
            Source: source,
            Company: "W",
            LineNo: line_no,
            KeyString: keystring,
            Comment: val
        }
    }).done(function (result) {
        console.log(result);
        cell.find('#cell-loading_' + line_no).remove();
    }).fail(function (error) {
        console.log(error);
    });
}

/* Quote Inquiry */

function getEmailDataQuoteInquiry() {
    var data = [];
    $('.vendors-cb').each(function (i, e) {
        var checked = $(e).is(":checked");
        var vendor = $(e).data('vendor');
        var name = $(e).data('name');
        var toAddress = getVendorToAddress(vendor);
        if (checked && toAddress.length > 0) {

            data.push({
                'QuoteHeader': $('#hiddenQuoteHeader').val(),
                'Quote_OrderNo': $('#hiddenQuoteNo').val(),
                'To': toAddress,
                'VendorName': name,
                'Notes': $('#inputNotes').val(),
                'SpecifiedAndLocation': $('#inputSpecifiedAndLocation').val(),
                'ZipCode': $('#inputZipCode').val(),
                'AnticipatedDate': $('#inputOrderDate').val(),
                'From': $('#inputYourEmail').val(),
                'CC1': $('#inputCC1').val(),
                'CC2': $('#inputCC2').val(),
                'RFP_BID': $('#rfp_bid').is(":checked"),
                'ExcludeTitle': $('#excludeTitle').is(":checked"),
                'Lines': getVendorLines(vendor),
                'CustomerName': $('#hiddenCustomerName').val(),
                'YourName': $('#inputYourName').val(),
                'ItemPricing': $('#chkPricing').is(":checked"),
                'ItemFreight': $('#chkFreight').is(":checked"),
                'ItemLeadTime': $('#chkLeadTime').is(":checked"),
                'ItemComYardage': $('#chkComYardage').is(":checked"),
                'ItemComApproval': $('#chkComApproval').is(":checked"),
                'ItemConfirmComShip': $('#chkConfirmComShip').is(":checked"),
                'ItemConfirmComAcrylic': $('#chkConfirmComAcrylic').is(":checked"),
                'ItemBudgetaryPricing': $('#chkBudgetaryPricing').is(":checked")
            });
        }
    });

    var form = new FormData();
    form.append('rawdata', JSON.stringify(data));
    var file = $('#fileAttachment')[0].files[0];
    if (file) {

        form.append('file', file);
    }

    return form;
}

function addFreeForm(elem, vendor) {
    $(elem).hide();
    $('#vendorToAddressFreeform_' + vendor).show();
}



function getVendorToAddress(vendor) {
    if (vendor.toString().includes('&')) vendor = vendor.toString().replace('&', '');
    var toElem = $('#vendorToAddress_' + vendor);
    var freeElem = $('#vendorToAddressFreeform_' + vendor);
    var element = $(toElem).prop("tagName");
    var addresses = [];
    switch (element) {
        case "INPUT":
            var val = $(toElem).val();
            if (val) {
                addresses = addresses.concat(val.split(';'));
            }
            break;
        case "SELECT":
            var arr = $(toElem).val();
            if (arr.length > 0) addresses = arr;
            var valfree = $(freeElem).val();
            if (valfree) {
                addresses = addresses.concat(valfree.split(';'));
            }
            break;
        default: return addresses;
    }

    return addresses;
}

function getVendorLines(vendor) {
    var lines = [];
    var rows = $('table.inquirytable tbody tr.vend-' + vendor);
    rows.each(function () {
        lines.push({
            'LineNo': $(this.cells[1]).html(),
            'Catalog': $(this.cells[3]).html(),
            'GeneralTagging': $(this.cells[4]).html(),
            'Description': $(this.cells[6]).html(),
            'QtyOrdered': $(this.cells[5]).html()
        });
    });
    return lines;
}

function sendEmailQuoteInquiry() {

    $.ajax({
        type: "POST",
        url: '/home/sendemail_qi',
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        cache: false,
        data: getEmailDataQuoteInquiry()
    }).done(function (result) {
        if (result) {
            ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
            clearFormQuoteInquiry();
        } else {
            location.replace("/home/error?msg=sending");
        }
    }
    ).fail(function (error) {
        location.replace("/home/error?msg=processing");
    });
}

function getQuoteGP(orderno, include) {
    $.ajax({
        async: true,
        type: "GET",
        url: '/home/GetQuoteGP?orderno=' + orderno + '&includeBO=' + include
    }).done(function (result) {
        $('#totalGP').html(result);
    }
    ).fail(function (error) {
        $('#totalGP').html('');
    });
}

function clearFormQuoteInquiry() {
    $('.vendors-cb').each(function (i, e) {
        if ($(e).is(":checked")) {
            $(e).click();
        }
    });
    $('#inputSpecifiedAndLocation').val("");
    $('#inputZipCode').val("");
    $('#inputOrderDate').val("");
    $('#inputYourEmail').val("");
    $('#inputCC1').val("");
    $('#inputCC2').val("");
    $('#inputNotes').val("");
    $('#SendEmailQouteInquiry').prop("disabled", false);
    $('#SendEmailQouteInquiry').html('Submit');
    $('.vendorToAddressFreeform').val("");
    $('.toAddress').val("");
}

function toggleBOSwitchQuoteInquiry(elem) {
    var rows = $('table.inquirytable tbody tr');
    var quoteno = $('#hiddenQuoteNo').val();
    if ($(elem).is(":checked")) {
        rows.filter('.row-isbo').show();
        getQuoteGP(quoteno, 1);
    } else {
        rows.filter('.row-isbo').hide();
        getQuoteGP(quoteno, 0);
    }
}

function clearFormRedline() {
    $('#inputRedlineTo').val('');
    $('#inputRedlineYourEmail').val('');
    $('#inputRedlineCC1').val('');
    $('#inputRedlineCC2').val('');
    $('#inputRedlineRfpBid').prop('checked', false);
    $('.inputComment').val('');
    $('.chkline').prop('checked', false);
}

function getRedlineEmailData() {
    var data = [];
    var cbList = $('.show-email-options');
    var lines = cbList.filter(':checked');
    var linesData = [];
    lines.each(function (index, elem) {
        var lineNo = $(elem).data('line');
        var vendor = $(elem).data('vendor');
        var catalogNo = $(elem).data('catalogno');
        var generalTagging = $(elem).data('graltagging');
        var qtyOrdered = $(elem).data('qtyordered');
        var description = $(elem).data('description');
        var gpDlls = $(elem).data('gpdlls');
        var list = $(elem).data('list');
        var lineSell = $(elem).data('linesell');
        var cost = $(elem).data('cost');
        var comment = $('.inputComment[data-lineno="' + lineNo + '"]').val();

        let dataToPush = {
            'LineNo': lineNo,
            'VendorNo': vendor,
            'CatalogNo': catalogNo,
            'GeneralTagging': generalTagging,
            'QtyOrdered': qtyOrdered,
            'Description': description,
            'GPDlls': gpDlls,
            'List': list,
            'LineSell': lineSell,
            'Comment': comment,
            'Cost': cost
        };

        linesData.push(dataToPush);
    });

    data.push({
        'To': $('#inputRedlineTo').val(),
        'YourEmail': $('#inputRedlineYourEmail').val(),
        'CC1': $('#inputRedlineCC1').val(),
        'CC2': $('#inputRedlineCC2').val(),
        'RFP_Bid': $('#inputRedlineRfpBid').is(":checked"),
        'QuoteNO': $('#hiddenRedlineOrderNo').val(),
        'CustomerName': $('#hiddenRedlineCustomerName').val(),
        'OrderTitle': $('#hiddenRedlineOrderTitle').val(),
        'LinesData': linesData
    });

    var form = new FormData();
    form.append('rawdata', JSON.stringify(data));
    var file = $('#inputRedlineAttachment')[0].files[0];
    if (file) {
        form.append('file', file);
    }

    return form;
}


/* Fast Track */

var fromDate;
var toDate;

function initForm() {
    var nowTemp = new Date();
    var now = new Date(nowTemp.getFullYear(), nowTemp.getMonth(), nowTemp.getDate(), 0, 0, 0, 0);

    fromDate = $('#fromDate').datepicker({
        todayHighlight: true
    }).on('changeDate', function (ev) {
        fromDate.hide();
        toDate.update();
        $('#toDate')[0].focus();
    }).data('datepicker');

    toDate = $('#toDate').datepicker({
        beforeShowDay: function (date) {
            if (date < fromDate.getDate()) {
                return false;
            }
        },
        todayHighlight: true

    })
        .on('changeDate', function (ev) {
            toDate.hide();
        }).data('datepicker');

}

function templateFasTrack(data) {
    var html = '';
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        html +=
            '<tr>'
            + '<td>' + row.LineNo + '</td>'
            + '<td>' + row.SalesCode + '</td>'
            + '<td>' + row.Vendor + '</td>'
            + '<td>' + row.CatalogNo + '</td>'
            + '<td>' + row.Description + '</td>'
            + '<td>' + row.QtyOrdered + '</td>'
            + '<td>' + row.QtyReceived + '</td>'
            + '<td class="receivedStatusColor-' + row.ReceivedStatusColor + '">' + row.ReceivedStatus + '</td>'
            + '</tr>';
    }

    return html;
}

function fastTrackShowDetails(index) {
    var title = "Lines Info " + LinesFastTrack[index].PO;
    var lines = LinesFastTrack[index].LinesInfo;
    $('#detailsModalTitle').html(title);
    $('#fasttrackTableDetails > tbody').html(templateFasTrack(lines));
    $('#detailsModal').modal('show');
}
/* Standard Price*/

function toggleBOSwitchStandardPrice(elem) {
    var rows = $('table tbody tr');
    var orderno = $('#hiddenOrderNo').val();
    if ($(elem).is(":checked")) {
        getTotals(orderno, 1);
        rows.filter('.row-isbo').show();
    } else {
        getTotals(orderno, 0);
        rows.filter('.row-isbo').hide();
    }
}

function getTotals(orderno, include) {
    $.ajax({
        async: true,
        type: "GET",
        url: '/home/GetTotalsStandardPrice?orderno=' + orderno + '&includeBO=' + include
    }).done(function (result) {
        $('#totalSell').html(result.TotalSell);
        $('#totalCost').html(result.TotalCost);
        $('#gpDollars').html(result.GPDollars);
        $('#gpPct').html(result.GPPct);
        $('#tax').html(result.Tax);
        $('#total_W_Tax').html(result.Total_W_Tax);

        $('#pidTotalSell').html(result.PIDTotalSell);
        $('#pidTotalCost').html(result.PIDTotalCost);
        $('#pidGPDollars').html(result.PIDGPDollars);
        $('#pidGPPct').html(result.PIDGPPct);
    }
    ).fail(function (error) {
        $('#totalSell').html('');
        $('#totalCost').html('');
        $('#gpDollars').html('');
        $('#gpPct').html('');
        $('#tax').html('');
        $('#total_W_Tax').html('');

        $('#pidTotalSell').html('');
        $('#pidTotalCost').html('');
        $('#pidGPDollars').html('');
        $('#pidGPPct').html('');
    });
}

/* Warehouse DT Report */
var warehouseDate;

function initFormWarehouse() {
    var nowTemp = new Date();
    var tomorrow = (new Date()).setDate(nowTemp.getDate() + 1);

    warehouseDate = $('#date').datepicker({
        todayHighlight: true,
        defaultDate: tomorrow
    }).data('datepicker');


}

function templateWarehouse(data) {
    var blankLocLines = data.filter(function (obj) {
        return obj.Location === '';
    });
    data = data.filter(function (obj) {
        return obj.Location !== '';
    });
    var data = data.sort((a, b) => a.Location.localeCompare(b.Location));
    for (var i in blankLocLines) {
        data.push(blankLocLines[i])
    }

    var html = '';
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        var isChecked = row.IsLoadedLine ? 'checked' : '';
        html +=
            '<tr>'
            + '<td>' + row.LineNo + '</td>'
            + '<td class="wdt-received-color-' + row.QtyReceivedColor + '">' + row.QtyOrdered + '</td>'
            + '<td>' + row.QtyReceived + '</td>'
            + '<td>' + row.QtyCartons + '</td>'
            + '<td>' + row.QtyScheduled + '</td>'
            + '<td>' + row.Vendor + '</td>'
            + '<td>' + row.CatalogNo + '</td>'
            + '<td>' + row.Description + '</td>'
            + '<td class="wdt-location-color-' + row.LocationColor + '">' + row.Location + '</td>'
            + '<td><input type="checkbox"' + isChecked + ' class="cbIsLoaded" data-orderno="' + row.OrderNo + '" data-lineno="' + row.LineNo + '" onchange="setLoaded(this)" /></td>'
            + '</tr>';
    }

    return html;
}

function warehouseShowDetails(orderNo) {
    var title = "Lines Info " + orderNo;
    var lines = getWarehouseLineDetails(LinesWarehouse, orderNo);
    $('#detailsModalTitle').html(title);
    $('#warehouseTableDetails > tbody').html(templateWarehouse(lines));
    $('#detailsModal').modal('show');
    var link = "/home/DownloadCSVWarehouseDT/?date=" + $("#csvWarehouseLink").data().date + "&orderno=" + orderNo + "&warehouse=" + $("#csvWarehouseLink").data().warehouse;
    $("#csvWarehouseLink").attr("href", link);
}

function getWarehouseLineDetails(summary, orderNo) {
    for (var i in summary) {
        if (summary[i].OrderNo == orderNo) {
            return summary[i].LinesInfo;
        }
    }
}

/* Default Pricing Report */

$('#aOpenDPR').click(function () {
    var d = new Date();
    var day = d.getDate();
    var month = d.getMonth() - 1;
    var year = d.getFullYear();
    var startDate = new Date(year, month, day);

    localStorage.clear();
    $('#dprDateFrom').datepicker('setDate', startDate);
    $('#dprDateTo').datepicker('setDate', new Date());
    $('#selLocations option[value="ALL"]').attr('selected', 'selected');
});

/*Share*/

function ShowSuccessAlert(props) {
    var html = '<div class="alert alert-' + props.type + ' alert-dismissible fade show" role="alert">' +
        '<h4 class="alert-heading">' + props.title + '</h4>' +
        '<p>' + props.message + '</p>' +
        '<hr>' +
        '<button type="button" class="close" data-dismiss="alert" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span>' +
        '</button></div>';

    $('#alert').append(html);

    if (props.autoclose) {
        setTimeout(function () {
            $(".alert").alert('close');
        }, 4000);
    }
}

function clearDateValues() {
    localStorage.clear();
}