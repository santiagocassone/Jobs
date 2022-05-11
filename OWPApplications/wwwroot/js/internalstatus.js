$(document).ready(function () {

    $("#tblSummary").tablesorter();
    $("#tblLineInfoPage").tablesorter();

    $('.emailTooltip').tooltip({
        boundary: "window",
        container: "body"
    });

    $('.schedDateTooltip').tooltip({
        boundary: "window",
        container: "body"
    });

    $('.DateTooltip').tooltip({
        boundary: "window",
        container: "body"
    });

    $('.costDateTooltip').tooltip({
        boundary: "window",
        container: "body"
    });

    $('#cutOffDateISR').datepicker({
        defaultDate: new Date(),
        todayHighlight: true,
        autoclose: true
    });

    $('#crdYourEmail').change(function () {
        $('#inputCC1').val($(this).val());
    });

    $('.crd').each(function () {
        if ($(this).data('alertdate') == 'yes') {
            $('#' + $(this).data('item')).show();
        }
    });

    $('.reqCRD').datepicker({
        autoclose: true
    })

    $('#isrCustomerMultiselect').selectpicker();

    $('#isrCustomerMultiselect').on('change', function () {
        var val_ = $('#isrCustomerMultiselect').val();
        $('.childCust').val(val_);
    });

    $('#salespersonSelect').on('change', function () {
        var val_ = $('#salespersonSelect').val();
        $('.childSales').val(val_);
    });




    $('#ordernoISR').change(function () {
        if ($(this).val() !== '') {
            $('#selespersonSelectISR').selectpicker('refresh').empty().selectpicker('refresh');
            $('#selespersonSelectISR').selectpicker('selectAll');
            $('#selespersonSelectISR').selectpicker('refresh');
            $('#isrCustomerMultiselect').selectpicker('refresh').empty().selectpicker('refresh');
            $('#isrCustomerMultiselect').selectpicker('selectAll');
            $('#isrCustomerMultiselect').selectpicker('refresh');
            $('#cutOffDateISR').val('');
            $('#cutOffDateISR').prop("disabled", true);
            $('#spType option:first').attr('selected', 'selected');
            $('#spType').prop("disabled", true);
        } else {
            $('#selespersonSelectISR').prop("disabled", false);
            $('#cutOffDateISR').prop("disabled", false);
            $('#spType').prop("disabled", false);
        }
    });

    $('#regionISR').change(function () {
        let regionISR = $(this).val();

        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: "/home/internalstatusreport/salespersons",
            data: { 'regionISR': regionISR },
            success: function (data) {
                var salespersons;
                if (data !== null) {
                    salespersons = data.map(function (option) {
                        return `<option value=${option.id}>${option.label}</option>`;
                    });
                } else {
                    salespersons = `<option value="">No data found</option>`;
                }

                $('#selespersonSelectISR').selectpicker('refresh').empty().append(salespersons).selectpicker('refresh');
            }
        });
    });

    $('#selespersonSelectISR').change(function () {
        var salespersons = $(this).val();
        let regionISR = $('#regionISR').val();

        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: "/home/internalstatusreport/salespersoncustomers",
            data: { 'salespersonid': salespersons, 'regionISR': regionISR },
            success: function (data) {
                var customersOptions;
                if (data !== null) {
                    customersOptions = data.map(function (option) {
                        return `<option value=${option.id}>${option.label}</option>`;
                    });
                } else {
                    customersOptions = `<option value="">No data found</option>`;
                }

                $('#isrCustomerMultiselect').selectpicker('refresh').empty().append(customersOptions).selectpicker('refresh');
                $('#isrCustomerMultiselect').selectpicker('selectAll');
                $('#isrCustomerMultiselect').selectpicker('refresh');
            }
        });
    });

    $('.show-email-options').change(function () {
        var parent = $(this).parent().parent();
        var line = $(this).data('line');
        var showlink = $(this).data('showlink');
        var isChecked = $(this).is(":checked");
        var vendor = $(this).data('vendor');
        var code = $(this).data('processcode');
        toggleDetails(parent, isChecked, line, showlink, vendor, code);

        SetEmailFormMessage();
    });


    $('.projCheck').click(function (evt) {
        $('#vOpt-' + $(this).data('orderno')).toggle('fast');
    });

    $('.vendCheck').click(function (evt) {
        $('#vndEmail-' + $(this).data('orderno') + '-' + $(this).data('vendorno') + '-' + $(this).data('suffix')).toggle('fast');
    });
    
    $('#SendEmailsInternalStatusVndInv').click(function (evt) {
        evt.preventDefault();
        if (!$(evt.target).closest("form").get(0).reportValidity()) {
            return false;
        }
        $(this).prop("disabled", true);
        // add spinner to button
        $(this).html(
            '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...'
        );
        sendEmailVendorInvoice();
    });

    $('#SendEmailsInternalStatus').click(function (evt) {
        evt.preventDefault();
        if ($('#msgEmailForm').css('display') === 'block' && $.trim($('#inputComments').val()).length === 0) {
            alert("Please, additional information is needed in Comments field.");
            return;
        } else {
            $(this).prop("disabled", true);
            // add spinner to button
            $(this).html(
                '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...'
            );
            sendEmailInternalStatus();
        }
    });

    $('#SendEmailsInternalStatusCRD').click(function (evt) {
        evt.preventDefault();
        if (!$(evt.target).closest("form").get(0).reportValidity()) {
            return false;
        }
        if ($('#msgEmailForm').css('display') === 'block' && $.trim($('#inputComments').val()).length === 0) {
            alert("Please, additional information is needed in Comments field.");
            return;
        } else {
            $(this).prop("disabled", true);
            // add spinner to button
            $(this).html(
                '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...'
            );
            sendEmailInternalStatusCRD($(this));

        }
    });

    $('#sendInvoiceEmailForm').click(function (evt) {
        evt.preventDefault();
        if (!$(evt.target).closest("form").get(0).reportValidity()) return false;

        $(this).prop("disabled", true);
        $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');

        sendInvoiceEmail($(this));
    });

    $('#projectIDFilter').keyup(function (evt) {
        var rows = $('table tbody tr');
        var filter = $(this).val();
        if (filter && filter !== "") {
            for (var i = 0; i < rows.length; i++) {
                var elem = $(rows[i]);
                var projectID = $(elem).find('.projId').html().trim();
                console.log(projectID);
                if (projectID.includes(filter)) {
                    $(elem).show();
                } else {
                    $(elem).hide();
                }
            }
        } else {
            $(rows).show();
        }
    });

    $('#accountabilityFilter').change(function () {
        var rows = $('table tbody tr');
        var filter = $(this).val();
        if (filter && filter !== "" && filter !== "All") {
            for (var i = 0; i < rows.length; i++) {
                var elem = $(rows[i]);
                var acc = $(elem).find('.accountability :selected').html().trim();
                if (acc.includes(filter)) {
                    $(elem).show();
                } else {
                    $(elem).hide();
                }
            }
        } else {
            $(rows).show();
        }
    });

    $('#completiondate').datepicker({
        defaultDate: new Date(),
        todayHighlight: true
    });

    $('#cb-invoice').click(function () {
        var isChcked = $(this).is(":checked");
        if (isChcked) {
            $('#invoice_type').show();
            if ($('#invoice-type_select').find('option:selected').val() === "BillOrder") {
                $('#invoice_date').show();
            } else {
                $('#invoice_received').show();
                $('#invoice_linesDetail').show();
            }
        } else {
            $('#invoice_type').hide();
            $('#invoice_date').hide();
            $('#invoice_received').hide();
            $('#invoice_linesDetail').hide();
        }
    });

    $('#invoice-type_select').change(function () {
        if ($('#invoice-type_select').find('option:selected').val() === "BillOrder") {
            $('#invoice_date').show();
            $('#invoice_received').hide();
            $('#invoice_linesDetail').hide();
        } else {
            $('#invoice_date').hide();
            $('#invoice_received').show();
            $('#invoice_linesDetail').show();
        }
    });

    $('#btnLinesDetail').click(function () {
        var title = "Lines detail info";
        var checkedLines = [];
        var checkedLineNos = [];
        var lines = $('.show-email-options').filter(':checked');
        lines.each(function (index, elem) {
            checkedLineNos.push($(elem).data('line'));
        });
        for (var i = 0; i < summaryInfo.Lines.length; i++) {
            if (checkedLineNos.indexOf(summaryInfo.Lines[i].LineNo) > -1) {
                checkedLines.push(summaryInfo.Lines[i]);
            }
        }
        $('#detailsModalTitle').html(title);
        $('#isrTableDetails > tbody').html(templateLinesDetail(checkedLines));
        $('#detailsModal').modal('show');
    });

    $('.showDet').click(function () {
        let orderNo = $(this).data('orderno');
        var checkedLines = [];
        summaryInfoData.map(function (val, ind, arr) {
            if (val.OrderNo == orderNo) {
                for (var i = 0; i < val.Lines.length; i++) {
                        checkedLines.push(val.Lines[i]);
                }
            }
        });     
        checkedLines.sort(function (a, b) {
            if (a.LineNo > b.LineNo) {
                return 1;
            }
            if (a.LineNo < b.LineNo) {
                return -1;
            }
            // a must be equal to b
            return 0;
        });
        $('#detailsModalTitle').html("Lines detail info");
        $('#isrTableDetails > tbody').html(templateLinesDetail(checkedLines));
        $('#detailsModal').modal('show');
    });

    if ($('#regionISRFilter').val() != null) {
        LoadVendors();
    }
    
    

    $('.multi-collapse').on('shown.bs.collapse', function () {
        $('#btnCost').html('All');
    });

    $('.multi-collapse').on('hidden.bs.collapse', function () {
        $('#btnCost').html('Cost Verified');
    });

    $('#includeCVSwitch').change(function () {
        var rows = $('table.isrTable tbody tr');
        if ($(this).is(":checked")) {
            rows.filter('.openCV').show();
        } else {
            rows.filter('.openCV').hide();
        }
    });

    setFormattedPercentageValues();
    $('#sortTh').click(function () {
        setFormattedPercentageValues();
    });

    $('#totalOpenSellToggle').change(function () {
        if ($(this).is(":checked")) {
            $('#divTotalOpenSell').css('display', 'block');
        } else {
            $('#divTotalOpenSell').css('display', 'none');
        }
    });

    $('.chkline').change(function () {
        var totalSell = 0;

        $('.chkline:checked').each(function () {
            totalSell += Number($(this).data('sell').replace(/[$,]/g, ''));
        });

        $('#totalOpenSell').val(totalSell);
        $('#spnTotalOpenSellValue').html('$ ' + addCommas(totalSell.toFixed(2)));
    });

    $('#invInvoiceType').change(function () {
        if ($('#invInvoiceType').find('option:selected').val() === "Complete") {
            $('#invInvoiceDate').show();
            $('#invInvoiceReceived').hide();
        } else {
            $('#invInvoiceDate').hide();
            $('#invInvoiceReceived').show();
        }
    });

    $('.txtComments').each(function () {
        this.style.height = (this.scrollHeight + 5) + 'px';
    });

    $('.txtComments').keyup(function () {
        autoresize(this);
    });

});
var InvoiceEmails;

function LoadVendors() {
    var url = '/home/GetVendorsEmails?app=POP&region=' + $('#regionISRFilter').val() + '&isLiveISR=true';
    $.ajax({
        type: "GET",
        url: url,
        async: true
    }).done(function (result) {
        InvoiceEmails = result;
    });
}

function GetEmailsVendor(vendorNo) {
    if (InvoiceEmails) {
        var index = binarySearchVendorIndex(InvoiceEmails, vendorNo);
        if (index !== -1) {
            return InvoiceEmails[index];
        }
    }
    return null;
}

function binarySearchVendorIndex(arr, vendor) {
    var l = 0, r = arr.length - 1;
    while (l <= r) {
        var m = Math.floor(l + (r - l) / 2);

        // Check if x is present at mid 
        if (arr[m].vendorNo === vendor)
            return m;

        // If x greater, ignore left half 
        if (arr[m].vendorNo < vendor)
            l = m + 1;

        // If x is smaller, ignore right half 
        else
            r = m - 1;
    }

    return -1;
}

var deliveredVendors = ['ONE20', 'ONE22', 'ONE26', 'ONE27', 'ONE28', 'ONE2W', 'UNI08'];

var emails = [
    { name: 'Billing Team', email: 'operationsbillingteam@oneworkplace.com' },
    { name: 'Frank Stout', email: 'fstout@oneworkplace.com' },
    { name: 'Luis Rodriguez', email: 'lrodriguez@oneworkplace.com' }
];

var emailsTo = {
    'PEN07': [{ name: 'PBI', email: 'clowensen@pbifurniture', description: '' }],
    'UNI08': [{ name: 'Sacramento', email: 'deannaw@oneworkplace.com', description: '' }],
    'SL': [{ name: 'Asset Team', email: 'assets@oneworkplace.com', description: '' }],
    'SM': [{ name: 'Asset Team', email: 'assets@oneworkplace.com', description: '' }],
    'AM': [{ name: 'AV Warehouse', email: 'jtsutumi@oneworkplace.com', description: '' }],
    'AV': [{ name: 'AV Warehouse', email: 'jtsutumi@oneworkplace.com', description: '' }],
    'IN': [{ name: 'Billing Team', email: 'operationsbillingteam@oneworkplace.com', description: '' }],
    'IO': [{ name: 'Billing Team', email: 'operationsbillingteam@oneworkplace.com', description: '' }],
    'DL': [{ name: 'Billing Team', email: 'operationsbillingteam@oneworkplace.com', description: '' }],
    'DM': [{ name: 'Billing Team', email: 'operationsbillingteam@oneworkplace.com', description: '' }],
    'RF': [{ name: 'Refurb', email: 'refurbdept@oneworkplace.com', description: '' }],
    'RM': [{ name: 'Refurb', email: 'refurbdept@oneworkplace.com', description: '' }],
    'SV': [{ name: 'Service Team', email: 'serviceteam@oneworkplace.com', description: '' }],
    'SW': [{ name: 'Service Team', email: 'serviceteam@oneworkplace.com', description: '' }],
    'WH': [{ name: 'Warehouse Team', email: 'lrodriguez@oneworkplace.com', description: '' }],
    'WI': [{ name: 'Warehouse Team', email: 'lrodriguez@oneworkplace.com', description: '' }]
};

function toggleDetails(elem, isShowing, lineno, showlink, vendor, code) {
    $(elem).nextUntil('tr.breakrow').slideToggle(200);
    let region = $('#regionISRFilter').val();
    if (isShowing) {
        createEmailSelect(lineno, 1, showlink, vendor, code, region);
    } else {
        $('[id^=formLine-' + lineno + ']').remove();
    }
}

function SetEmailFormMessage() {
    var selects = $('select[name="emailType"]');
    var value = 'none';
    for (var i in selects) {
        if (selects[i].value === 'NoCostProduct')
            value = 'block';
    }
    $('#msgEmailForm').css('display', value);
}

function createEmailSelect(lineno, num, showlink, vendor, code, region) {

    let soNotHard = false;
    if (num > 1) {
        $('#btnAdd-' + lineno + '-' + (num - 1)).hide();
        $('#btnRemove-' + lineno + '-' + (num - 1)).hide();
    }

    var html = '<div id="formLine-' + lineno + '-' + num + '" class="form form-inline mb-3"><div class="form-group col-md-5">'
        + '<label for="emailtype-' + lineno + '-' + num + '">Email Type:</label>'
        + '<select style="width:65%" class="custom-select ml-sm-2 emailtype" name="emailType" id="emailtype-' + lineno + '-' + num + '" onchange="emailTypeOnChange(this,' + lineno + ',' + num + ',\'' + vendor + '\'' + ',\'' + region + '\')"> '
        + '<option value="default">Choose..</option>';
        

    if (region === 'OWP') {
        switch (code) {

            case 'IP':
            case 'SO':
                if ($('#hardsched-' + lineno).val() == 'Yes') {
                    html += '<option value="Delivered">Ticketed/Delivered - SO</option>';
                } else {
                    soNotHard = true;
                }                
                html += '<option value="Freeform">Freeform</option>';
                break;

            case 'DX':
            case 'D2':
            case 'I2':
            case 'D3':
            case 'I3':
            case 'D4':
            case 'D5':
            case 'DG':
            case 'DN':
            case 'ID':
            case 'IG':
            case 'IW':
/*                html += '<option value="VendorInvoice">Vendor Invoice</option>'*/
                html += '<option value="NoCostProduct">CV - AME01 or No Cost Product</option>'
                    + '<option value="Freeform">Freeform</option>';
                break;

            case 'CD':
            case 'RM':
            case 'W1':
            case 'W7':
            case 'W8':
            case 'W9':
            case 'WF':
            case 'WG':
            case 'WL':
            case 'WN':
            case 'WP':
                html += '<option value="Received">Received</option>'
                    + '<option value="Ticketed">Ticketed/Delivered - Product</option>'
/*                    + '<option value="VendorInvoice">Vendor Invoice</option>'*/
                    + '<option value="NoCostProduct">CV - AME01 or No Cost Product</option>'
                    + '<option value="Freeform">Freeform</option>';
                break;
            case 'PA':
/*                html += '<option value="VendorInvoice">Vendor Invoice</option>'*/
                html += '<option value="NoCostProduct">CV - AME01 or No Cost Product</option>';
                break;
            case 'OM':
/*                html += '<option value="VendorInvoice">Vendor Invoice</option>'*/
                html += '<option value="NoCostProduct">CV - AME01 or No Cost Product</option>'
                    + '<option value="Freeform">Freeform</option>';
                break;
            case 'S9':
                html += '<option value="Ticketed">Ticketed/Delivered - Product</option>';
                break;
            case 'D1':
                html += '<option value="Ticketed">Ticketed/Delivered - Product</option>';
                   /* + '<option value="VendorInvoice">Vendor Invoice</option>';*/
                break;
            default:
                html += '<option value="Freeform">Freeform</option>';
                break;
        } 
    } else {
        html += '<option value="OSQReceived">Received</option>'
            + '<option value="OSQProduct">Ticketed/Delivered - Product</option>';
        if ($('#hardsched-' + lineno).val() == 'NS ' && code == 'SO') {
            soNotHard = true;            
        } else {
            html += '<option value="OSQLabor">Ticketed/Delivered - Labor</option>';
        }
/*            html += '<option value="OSQVendorInvoice">Vendor Invoice</option>'*/
        html += '<option value="OSQFreeform">Freeform</option>';
    }
    

    html += '</select></div>'
        + '<div class="form-group col-md-4 toaddress-' + lineno + '-' + num + '"></div>'
        + '<div class="form-group col-md-3"><button type="button" class="btn btn-primary" id="btnAdd-' + lineno + '-' + num + '" onclick="createEmailSelect(' + lineno + ', ' + (num + 1) + ',\'' + showlink + '\'' + ',\'' + vendor + '\',\'' + region + '\',\'' + code + '\')">+</button>'
        + '<button type="button" class="btn btn-danger m-2" id="btnRemove-' + lineno + '-' + num + '" onclick="removeForm(' + lineno + ', ' + num + ')">-</button>';

    var order = $('#hiddenOrderNo').val();
    if (showlink === "True") {
        html += '<a target="_blank" href="/orderadmin/default.asp?formAction=Search&orderno=' + order + '">Open ORT to cancel line.</a>';
    }
    html += '</div></div>';
    if (soNotHard) {
        html += '<div><span style="color:red;">SO line is not hard scheduled</span></div>';
    }

    $('#options-' + lineno).find('.detail-container').append(html);
}

function removeForm(lineno, num) {
    if (num <= 1) {
        $('#cb-' + lineno).click();
    } else {
        $('#formLine-' + lineno + '-' + num).remove();
        $('#btnAdd-' + lineno + '-' + (num - 1)).show();
        $('#btnRemove-' + lineno + '-' + (num - 1)).show();
    }
}

function emailTypeOnChange(elem, lineno, num, vendor, region) {
    var option = $(elem).find('option:selected').val();
    removeAnyOptions(lineno, num);
    if (region === 'OWP') {
        switch (option) {
            case "Received": addReceivedOptions(lineno, num); break;
            case "Ticketed": addTicketedOptions(lineno, num); break;
            case "Delivered": addDeliveredOptions(lineno, num); break;
            //case "Invoiced": addInvoicedOptions(lineno, num); break;
            case "VendorInvoice": addVendorInvoiceOptions(lineno, num, vendor); break;
            case "NoCostProduct": addNoCostProductOptions(lineno, num); break;
            case "Freeform": addFreeformOptions(lineno, num); break;
            default: removeAnyOptions(lineno, num); break;
        }
    } else {
        let html = '';
        switch (option) {
            case 'OSQReceived':
                html += '<div class="optionsEmails"  id="optionsOSQ-' + lineno + '-' + num + '"> <label for="toAddress-' + lineno + '-' + num + '">To:</label>'
                    + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value=' + getEmail('OSQ-Receiving') + ' readonly /> </div>';
                break;
            case 'OSQProduct':
                html += '<div class="optionsEmails"  id="optionsOSQ-' + lineno + '-' + num + '"> <label for="toAddress-' + lineno + '-' + num + '">To:</label>'
                    + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value=' + getEmail('OSQ-Ticketing Product') + ' readonly /> </div>';
                break;
            case 'OSQLabor':
                html += '<div class="optionsEmails"  id="optionsOSQ-' + lineno + '-' + num + '"> <label for="toAddress-' + lineno + '-' + num + '">To:</label>'
                    + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value=' + getEmail('OSQ-Ticketing Labor') + ' readonly /> </div>';
                break;
            case 'OSQVendorInvoice':
                var input = GetInvoiceVendorInput(lineno, num, vendor);
                html += '<div class="optionsEmails" id="optionsVendorInvoice-' + lineno + '-' + num + '"><label for="toAddress-' + lineno + '-' + num + '">To:</label>';
                html += input + '</div>';
                break;
            case 'OSQFreeform':
                html += '<div class="optionsEmails"  id="optionsOSQ-' + lineno + '-' + num + '"> <label for="toAddress-' + lineno + '-' + num + '">To:</label>'
                    + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" /> </div>';
                break;
            default: removeAnyOptions(lineno, num); break;
        }
        var el = $('.toaddress-' + lineno + '-' + num);

        $(el).append(html);
    }
    

    SetEmailFormMessage();
}

function removeAnyOptions(lineno, num) {
    $('#optionsReceived-' + lineno + '-' + num).remove();
    $('#optionsTicketed-' + lineno + '-' + num).remove();
    $('#optionsDelivered-' + lineno + '-' + num).remove();
    $('#optionsInvoiced-' + lineno + '-' + num).remove();
    $('#optionsVendorInvoice-' + lineno + '-' + num).remove();
    $('#optionsFreeform-' + lineno + '-' + num).remove();
    $('#optionsNoCostProduct-' + lineno + '-' + num).remove();
    $('#optionsOSQ-' + lineno + '-' + num).remove();
    $('#options-' + lineno).find('.detail-container').find('.receivedForm').remove();
}

function addFreeformOptions(lineno, num) {
    var el = $('.toaddress-' + lineno + '-' + num);
    var html = '<div class="optionsEmails"  id="optionsFreeform-' + lineno + '-' + num + '"> <label for="toAddress-' + lineno + '-' + num + '">To:</label>'
        + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" /> </div>';

    $(el).append(html);
}

function addNoCostProductOptions(lineno, num) {
    var el = $('.toaddress-' + lineno + '-' + num);
    var html = '<div class="optionsEmails"  id="optionsNoCostProduct-' + lineno + '-' + num + '"> <label for="toAddress-' + lineno + '-' + num + '">To:</label>'
        + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value=' + getEmail('No Cost Project') + ' readonly /> </div>';

    $(el).append(html);
}

function addReceivedOptions(lineno, num) {
    let el = $('.toaddress-' + lineno + '-' + num);    
    let html = '<div class="optionsEmails"  id="optionsReceived-' + lineno + '-' + num + '"> <label for="toAddress-' + lineno + '-' + num + '">To:</label>'
        + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value=' + getEmail('Received') + ' readonly /> </div>';

    $(el).append(html);

    let htmlForm = '<div class="form form-inline mb-3 receivedForm">' +
        '<div class="form-group col-3"><label for="carrier-' + lineno + '-' + num + '">Carrier:</label><input type="text" class="form-control ml-sm-2 carrier" id="carrier-' + lineno + '-' + num + '" /></div>' +
        '<div class="form-group col-3"><label for="tracking-' + lineno + '-' + num + '">Tracking #/BOL:</label><input type="text" class="form-control ml-sm-2 tracking" id="tracking-' + lineno + '-' + num + '" /></div>' +
        '<div class="form-group col-3"><label for="delivDate-' + lineno + '-' + num + '">Delivered Date:</label><input type="text" class="form-control ml-sm-2 delivered" id="delivDate-' + lineno + '-' + num + '" /></div>' +
        '</div > '
    $('#options-' + lineno).find('.detail-container').append(htmlForm);
    $('#delivDate-' + lineno + '-' + num).datepicker({
        todayHighlight: true,
        autoclose: true
    });
}

function addTicketedOptions(lineno, num) {
    var pc = $("#cb-" + lineno).data('processcode');
    var el = $('.toaddress-' + lineno + '-' + num);
    var html = '<div class="optionsEmails"  id="optionsTicketed-' + lineno + '-' + num + '"><label for="toAddress-' + lineno + '-' + num + '">To:</label>';
    if ($('#warehouse-' + lineno).val() == "U") {
        html += '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value="sacinstallteam@oneworkplace.com" />';
    } else {
        if (pc == 'D1') {
            html += '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value=' + getEmail('Ticketed-D1') + ' />';
        } else {
            html += '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value=' + getEmail('Ticketed') + ' />';
        }
    }
    html += '</div>';
    $(el).append(html);
}
function addDeliveredOptions(lineno, num) {
    var el = $('.toaddress-' + lineno + '-' + num);
    var html = '<div class="optionsEmails"  id="optionsDelivered-' + lineno + '-' + num + '"><label for="toAddress-' + lineno + '-' + num + '">To:</label>';
    var pc = $("#cb-" + lineno).data('processcode');
    var v = $("#cb-" + lineno).data('vendor');

    if ($('#warehouse-' + lineno).val() == "U") {
        html += '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value="sacinstallteam@oneworkplace.com" />';
    } else {
        if (deliveredVendors.includes(v)) { //TODO: see if implements rules below
            let emailAddress = getEmail('Delivered- vendor ' + v);
            html += '<select type="email" class="form-control  ml-sm-2 custom-select w-85" id="toAddress-' + lineno + '-' + num + '">';
            html += '<option value="' + emailAddress + '">' + emailAddress + '</option>';
        }
        else if (pc === 'SO') {
            var sc = $("#cb-" + lineno).data('salescode');
            if (emailsTo.hasOwnProperty(v)) {
                let emailAddress = getEmail('Delivered-SO-' + v);
                html += '<select type="email" class="form-control  ml-sm-2 custom-select w-85" id="toAddress-' + lineno + '-' + num + '">';
                html += '<option value="' + emailAddress + '">' + emailAddress + '</option>';
            } else if (emailsTo.hasOwnProperty(sc)) {
                let emailAddress = getEmail('Delivered-SO-' + sc);
                html += '<select type="email" class="form-control  ml-sm-2 custom-select w-85" id="toAddress-' + lineno + '-' + num + '">';
                html += '<option value="' + emailAddress + '">' + emailAddress + '</option>';
            } else {
                html += '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value="" />';
            }
        } else {
            let emailAddress = getEmailList('Delivered-NOTSO');
            html += '<select type="email" class="form-control  ml-sm-2 custom-select w-85" id="toAddress-' + lineno + '-' + num + '">';
            emailAddress.forEach(function (item) {
                html += '<option value="' + item + '">' + item + '</option>';
            });
        }
    }    
    html += '</div>';
    $(el).append(html);
}
//function addInvoicedOptions(lineno, num) {
//    var el = $('.toaddress-' + lineno + '-' + num);
//    var html = '<div class="optionsEmails"  id="optionsInvoiced-' + lineno + '-' + num + '"><label for="toAddress-' + lineno + '-' + num + '">To:</label>'
//        + '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value="dhatch@oneworkplace.com" readonly /></div>';

//    $(el).append(html);
//}
function addVendorInvoiceOptions(lineno, num, vendor) {
    var el = $('.toaddress-' + lineno + '-' + num);
    var input = GetInvoiceVendorInput(lineno, num, vendor);
    var html = '<div class="optionsEmails" id="optionsVendorInvoice-' + lineno + '-' + num + '"><label for="toAddress-' + lineno + '-' + num + '">To:</label>';
    html += input + '</div>';

    $(el).append(html);
}

function GetInvoiceVendorInput(lineno, num, vendorno) {
    var vendor = GetEmailsVendor(vendorno);
    var vendorInput = '';
    if (vendor) {
        if (vendor.addresses.length > 1) {
            vendorInput = '<select class="custom-select mr-sm-2" id="toAddress-' + lineno + '-' + num + '">';
            for (var i = 0; i < vendor.addresses.length; i++) {
                vendorInput += '<option value="' + vendor.addresses[i] + '">' + vendor.addresses[i] + '</option>';
            }
            vendorInput += '</select>';
        } else {
            vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value="' + vendor.addresses[0] + '" />';
        }
    } else {
        vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + lineno + '-' + num + '" value="" />';
    }

    return vendorInput;
}

function getEmailData() {
    var cbList = $('.show-email-options');
    var lines = cbList.filter(':checked');
    var emailData = [];
    var comments = $('#inputComments').val();
    lines.each(function (index, elem) {
        var custName = $(elem).data('custname');
        var lineno = $(elem).data('line');
        var vendor = $(elem).data('vendor');
        var ack = $(elem).data('ack');
        var PO = $(elem).data('owppo');
        var sell = $(elem).data('sell');
        var _location = $(elem).data('location');
        var companyCode = $(elem).data('companycode');
        var processCode = $(elem).data('processcode');
        var optionsLength = $('[id^=formLine-' + lineno + ']').length;
        var totalSell = $('#linkOrderNo').data('totalsell');
        var sellEligibleForPartialInvoicing = $('#linkOrderNo').data('selleligible');
        for (var i = 1; i <= optionsLength; i++) {
            var emailType = $('#emailtype-' + lineno + '-' + i).find('option:selected').val();
            var toElem = $('#toAddress-' + lineno + '-' + i);
            if (emailType && emailType !== "default") {
                let dataToPush = {
                    'LineNo': lineno,
                    'VendorNo': vendor,
                    'ACK': ack,
                    'OWP_PO': PO,
                    'EmailType': emailType,
                    'To': _getToValue(toElem, emailType),
                    'Comments': comments,
                    'Location': _location,
                    'Sell': sell,
                    'CustomerName': custName,
                    'TotalSell': totalSell,
                    'SellEligibleForPartialInvoicing': sellEligibleForPartialInvoicing,
                    'CompanyCode': companyCode,
                    'ProcessCode': processCode
                };

                if (emailType === 'Received') {
                    let form = $('#options-' + lineno).find('.detail-container');
                    dataToPush.Carrier = form.find('.carrier').val();
                    dataToPush.Tracking = form.find('.tracking').val();
                    dataToPush.Delivered = form.find('.delivered').val();
                }
                emailData.push(dataToPush);
            }
        }
    });

    var invoiceData = getInvoiceEmail(comments);
    if (invoiceData) {
        emailData.push(invoiceData);
    }

    return emailData;
}

function _getToValue(toElem, emailType) {
    var element = $(toElem).prop("tagName");
    switch (element) {
        case "INPUT": return $(toElem).val();
        case "SELECT": return $(toElem).find('option:selected').val();
        default: return null;
    }
}

function getEmailDataVendorInvoice() {

    let data = {
        'Region': $('#regionISRFilter').val(),
        'From': $('#vndInvYourEmail').val(),
        'Name': $('#vndInvYourName').val(),
        'CC1': $('#vndInvCC1').val(),
        'CC2': $('#vndInvCC2').val(),
        'Comments': $('#vndInvComments').val(),
        Orders: []
    }    
    let vList = $('.vends-' + $('#vndinvorder').val());
        let vendors = vList.filter(':checked');
        let finalVends = [];
        vendors.each(function (index, elemv) {
            finalVends.push({ VendorNo: $(elemv).data('vendorno'), VendorEmail: $('#vndEmail-' + $(elemv).data('vendorno')).val(), Suffix: $(elemv).data('suffix') });
        });
    data.Orders.push({ OrderNo: $('#vndinvorder').val(), Vendors: finalVends });

    return data;
}

function getEmailDataInternalStatus() {
    var checkedLines = '';
    var arrLines = $('.show-email-options').filter(':checked');
    arrLines.each(function (index, elem) {
        checkedLines += $(elem).data('line') + ', ';
    });

    return {
        'Quote_OrderNo': $('#hiddenOrderNo').val(),
        'From': $('#inputYourEmail').val(),
        'Name': $('#inputYourName').val(),
        'To': '',
        'CC1': $('#inputCC1').val(),
        'CC2': $('#inputCC2').val(),
        'DataToSend': getEmailData(),
        'HasToInvoice': $('#cb-invoice').is(":checked"),
        'InvoiceType': $('#cb-invoice').is(":checked") ? $('#invoice-type_select').find(":selected").text() : '',
        'CheckedLines': checkedLines.trim().slice(0, -1),
        'OrderTitle': $('#hiddenOrderTitle').val(),
        'InvoiceReceived': $('#cb-invoice').is(":checked") && $('#invoice-type_select').find(":selected").text() == "Partial" ? $('#invoice-received_select').find(":selected").val() : '',
        'TotalSellIncludingTax': $('#totalOpenSell').val(),
        'Accountability': $('#hiddenAccountability').val(),
        'Region': $('#regionISRFilter').val(),
        'CompanyCode': $('#companyCodeLog').val()
    };
}

function getEmailDataInternalStatusCRD() {
    let crdData = [];
    $('#tblSummary > tbody > tr').each(function () {
        $this = $(this);
        if ($(this).find('input.projCheck').is(":checked")) {
            crdData.push({
                'ProjectID': $(this).find('a.ordernocrd').html(),
                'CRD': $(this).find('span.crd').html(),
                'ReqCRD': $(this).find('input.reqCRD').val(),
                'Customer': $(this).find('span.cust').html()
            });
        }
    });
    console.log(crdData);
    return {
        'From': $('#crdYourEmail').val(),
        'Name': $('#inputYourName').val(),
        'CC1': $('#inputCC1').val(),
        'CC2': $('#inputCC2').val(),
        'Comments': $('#inputComments').val(),
        'DataToSend': crdData,
        'Region': $('#regionISRFilter').val()
    };
}

function getInvoiceEmailData() {
    return {
        'InvoiceType': $('#invInvoiceType').val(),
        'From': $('#invYourEmail').val(),
        'Name': $('#inputYourName').val(),
        'CC1': $('#invCC1').val(),
        'CC2': $('#invCC2').val(),
        'Comments': $('#invComments').val(),
        'LinesData': getInvoiceLinesData(),
        'CompletionDate': $('#invInvoiceType').val() === 'Complete' ? $('#completiondate').val() : '',
        'ReceivedOrPaid': $('#invInvoiceType').val() === 'Partial' ? $('#invoice-received_select').find(":selected").text() : '',
        'Region': $('#regionISRFilter').val()
    };    
}

function getInvoiceLinesData() {
    var cbList = $('.projCheck');
    var lines = cbList.filter(':checked');
    var emailData = [];

    lines.each(function (index, elem) {
        var orderNo = $(elem).data('orderno');
        var orderTitle = $(elem).data('ordertitle');
        var projectId = $(elem).data('projectid');
        var accountability = $(elem).data('accountability');
        var customerName = $(elem).data('customername');
        var totalSell = $(elem).data('totalsell');
        var totalCost = $(elem).data('totalcost');
        var sellEligibleForPartialInvoicing = $(elem).data('selleligible');
        let companyCode = $(elem).data('companycode');

        let dataToPush = {
            'OrderNo': orderNo,
            'OrderTitle': orderTitle,
            'ProjectID': projectId,
            'Accountability': accountability,
            'CustomerName': customerName,
            'TotalSell': totalSell,
            'TotalCost': totalCost,
            'SellEligibleForPartialInvoicing': sellEligibleForPartialInvoicing,
            'CompanyCode': companyCode
        };

        emailData.push(dataToPush);
    });

    return emailData;
}

function sendEmailVendorInvoice() {
    var data = getEmailDataVendorInvoice();
    let completeEmail = false;
    let selectVendor = false;
    data.Orders.forEach(function (index, elem) {
        if (index.Vendors.length == 0) {
            selectVendor = true;
        } else {
            index.Vendors.forEach(function (indexv, elem) {
                if (indexv.VendorEmail == '') {
                    completeEmail = true;
                }
            });
        }
    });

    if (completeEmail || selectVendor) {
        alert('Please select and complete vendors emails');
        $('#SendEmailsInternalStatusVndInv').prop("disabled", false);
        $('#SendEmailsInternalStatusVndInv').html('Submit');
    } else {
        $.ajax({
            type: "POST",
            url: '/home/sendemails_is_vi',
            data: data
        }).done(function (result) {
            if (result) {
                ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
                clearFormVendorInvoice();
            } else {
                location.replace("/home/error?msg=sending");
            }
        }
        ).fail(function (error) {
            location.replace("/home/error?msg=processing");
        });
    }
    
}

function sendEmailInternalStatus() {
    var data = getEmailDataInternalStatus();
    if (data.DataToSend.length <= 0) {
        alert("There is no rows checked to send email.");
        $('#SendEmailsInternalStatus').prop("disabled", false);
        $('#SendEmailsInternalStatus').html('Submit');
        return;
    } else {
        let flag = false;
        data.DataToSend.forEach(email => {
            if (email.EmailType === 'Received' && (email.Carrier === '' || email.Delivered === '' || email.Delivered === '')) {
                flag = true;
            }
        });
        if (flag) {
            alert("Please complete the Received email form.");
            $('#SendEmailsInternalStatus').prop("disabled", false);
            $('#SendEmailsInternalStatus').html('Submit');
            return;
        }
    }


    $.ajax({
        type: "POST",
        url: '/home/sendemails_is',
        data: data
    }).done(function (result) {
        if (result) {
            ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
            clearFormInternalStatus();
        } else {
            location.replace("/home/error?msg=sending");
        }
    }
    ).fail(function (error) {
        location.replace("/home/error?msg=processing");
    });
}

function sendEmailInternalStatusCRD(btn) {
    var data = getEmailDataInternalStatusCRD();
    if (data.DataToSend.length <= 0) {
        alert("There is no rows checked to send email.");
        btn.html('Submit');
        btn.prop("disabled", false);
        return;
    }
    var noReq = false;
    $.each(data.DataToSend, (i, v) => {
        if (v.ReqCRD === '') {
            noReq = true;
        }
    });

    if (noReq) {
        alert('Please complete all Requested CRDs on checked orders');
        btn.html('Submit');
        btn.prop("disabled", false);
        return;
    }
    $.ajax({
        type: "POST",
        url: '/home/sendemails_is_crd',
        data: data
    }).done(function (result) {
        if (result) {
            ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
            clearFormInternalStatusCRD();
            $('.reqCRD').val('');
        } else {
            $('.reqCRD').val('');
            location.replace("/home/error?msg=sending");
        }
    }
    ).fail(function (error) {
        $('.reqCRD').val('');
        location.replace("/home/error?msg=processing");
    });
}

function sendInvoiceEmail(btn) {
    var data = getInvoiceEmailData();

    if (data.LinesData.length <= 0) {
        alert("There is no rows checked to send email.");
        $('#sendInvoiceEmailForm').prop("disabled", false);
        $('#sendInvoiceEmailForm').html('Submit');
        return;
    }

    var form = new FormData();
    form.append('rawdata', JSON.stringify(data));
    var file = $('#fileAttachment')[0].files[0];
    if (file) {
        form.append('file', file);
    }

    $.ajax({
        type: "POST",
        url: '/home/sendInvoiceEmails',
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        cache: false,
        data: form
    }).done(function (result) {
        if (result) {
            ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
            clearFormInvoiceEmail();
        } else {
            location.replace("/home/error?msg=sending");
        }
    }).fail(function (error) {
        location.replace("/home/error?msg=processing");
    });
}

function clearFormVendorInvoice() {
    $('.vendCheck').each(function (i, e) {
        if ($(e).is(":checked")) {
            $(e).click();
        }
    });
    $('.projCheck').each(function (i, e) {
        if ($(e).is(":checked")) {
            $(e).click();
        }
    });
    $('#vndInvYourEmail').val('');
    $('#vndInvYourName').val('');
    $('#vndInvCC1').val('');
    $('#vndInvCC2').val('');
    $('#vndInvComments').val('');
    $('#SendEmailsInternalStatusVndInv').prop("disabled", false);
    $('#SendEmailsInternalStatusVndInv').html('Submit');
}

function clearFormInternalStatus() {
    $('#inputYourEmail').val("");
    $('#inputCC1').val("");
    $('#inputCC1').val("");
    $('#inputComments').val("");
    $('#SendEmailsInternalStatus').prop("disabled", false);
    $('#SendEmailsInternalStatus').html('Submit');
}

function clearFormInternalStatusCRD() {
    $('#crdYourEmail').val("");
    $('#inputCC1').val("");
    $('#inputCC1').val("");
    $('#inputYourName').val("");
    $('#inputComments').val("");
    $('#SendEmailsInternalStatusCRD').prop("disabled", false);
    $('#SendEmailsInternalStatusCRD').html('Submit');
    $('#tblSummary > tbody > tr').each(function () {
        $this = $(this);
        if ($(this).find('input.projCheck').is(":checked")) {
            $(this).find('input.projCheck').prop('checked', false);
        }
    });
}

function clearFormInvoiceEmail() {
    $('#invInvoiceType').val("Complete");
    $('#invYourEmail').val("");
    $('#invCC1').val("");
    $('#invCC2').val("");
    $('#invComments').val("");
    $('#sendInvoiceEmailForm').prop("disabled", false);
    $('#sendInvoiceEmailForm').html('Submit');
    $('.projCheck').prop('checked', false);
}

function getInvoiceEmail(comments) {
    var hasToInvoice = $('#cb-invoice').is(":checked");
    if (hasToInvoice) {
        var cdate = $('#completiondate').val();
        var emailType = $('#invoice-type_select').find('option:selected').val();
        return {
            'LineNo': -1,
            'VendorNo': '',
            'ACK': '',
            'OWP_PO': '',
            'EmailType': emailType,
            'To': '',
            'Comments': comments,
            'Location': '',
            'CompletionDate': cdate
        };
    }
    return null;
}

function setFormattedPercentageValues() {
    $('.sortTd').each(function () {
        $(this).html($(this).attr('data-value'));
    });
}

function addCommas(x) {
    var parts = x.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}

function templateLinesDetail(data) {
    var html = '';
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        html +=
            '<tr>'
            + '<td>' + row.LineNo + '</td>'
            + '<td>' + row.ProcessingCode + '</td>'
            + '<td>' + row.SalesCode + '</td>'
            + '<td>' + row.VendorNo + '</td>'
            + '<td>' + row.Catalog + '</td>'
            + '<td>' + row.Ordered + '</td>'
            + '<td>' + row.Received + '</td>'
            + '<td>' + row.Ticketed + '</td>'
            + '<td>' + row.Delivered + '</td>'
            + '<td>' + row.Invoiced + '</td>'
            + '<td>' + row.VendorInv + '</td>'
            + '<td>' + row.OpenSell + '</td>'
            + '<td>' + row.OpenCost + '</td>'
            + '<td>' + row.ScheduleDate + '</td>'
            + '<td>' + row.EstimatedArrivalDate + '</td>'
            + '<td>' + row.CustomerRequestDate + '</td>'
            + '</tr>';
    }

    var totalSalesTax = 0;
    var taxPct = 0;

    $('.chkline:checked').each(function () {
        totalSalesTax += Number($(this).data('taxamount')) + Number($(this).data('sell').replace(/[$,]/g, ''));
        taxPct = Number($(this).data('taxpct'));
    });

    var labelText = 'Total Sell Including Sales Tax for Selected Lines: ';
    var valueText = '$ ' + addCommas(totalSalesTax.toFixed(2)) + ' (' + addCommas((taxPct * 100).toFixed(2)) + '%)';

    html += '<tr>'
        + '<td colspan="16">' + labelText + valueText + '</td>'
        + '</tr>';

    return html;
}

function autoresize(textarea) {
    textarea.style.height = '0px';
    textarea.style.height = (textarea.scrollHeight + 5) + 'px';
}

function printPDFISR(summaryinfoData) {

    $('#pdfLink').html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');
    data = JSON.stringify(summaryinfoData);
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "POST",
        url: '/Home/GenerateISRPDF',
        data: data
    }).done(function (result) {
        $('#pdfLink').html('Print in PDF');
        if (result) {
            window.open('../files/ISRDetail' + result + '.pdf', '_blank');
        }
        else {
            location.replace("/home/error?msg=sending");
        }
    }).fail(function (error) {
        $('#pdfLink').html('Print in PDF');
        location.replace("/home/error?msg=processing");

    });
}

function getEmail(emailType) {
    var ret;
    $.ajax({
        type: "GET",
        url: '/Home/GetEmailByEmailType',
        async: false,
        data: { emailType }
    }).done(function (result) {
        ret = result;
    });
    return ret;
}

function getEmailList(emailType) {
    var ret;
    $.ajax({
        type: "GET",
        url: '/Home/GetEmailByEmailTypeList',
        async: false,
        data: { emailType }
    }).done(function (result) {
        ret = result;
    });
    return ret;
}