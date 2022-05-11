var VendorsEmails = [];

$(document).ready(function () {

    $('#trackingLinkModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget)
        $('#trackingLink').data('orderno', button.data('orderno'))
        $('#trackingLink').data('po', button.data('po'))
        $('#trackingLink').data('keystring', button.data('keystring'))
        $('#trackingLink').data('companycode', button.data('companycode'))
        $('#trackingLink').data('ordertype', button.data('ordertype'))
        $('#trackingLink').data('upsertlevel', button.data('upsertlevel'))
        $('#trackingLink').data('field', button.data('field'))
        $('#trackingLink').data('source', button.data('source'))
        $('#trackingLink').val(button.data('value'))
    })

    $('#btnSaveTrackingLink').click(function () {
        var el = $('#trackingLink');

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

        var cell = $(this);
        cell.append('<div id="cell-loading" class="spinner-border spinner-border-sm" role="status"><span class="sr-only">Saving</span></div>');
        $.ajax({
            type: "POST",
            url: '/home/UpdateValues',
            data: data
        }).done(function (result) {
            console.log(result);
            cell.find('#cell-loading').remove();
            $('#trackingLinkModal').modal('toggle')
            $('#linkTrackLink-W' + data.OrderNo + '-' + data.PO).attr('href', 'http://' + data.Value)
            if (data.Value != "") {                
                $('#linkTrackLink-W' + data.OrderNo + '-' + data.PO).show()
            } else {
                $('#linkTrackLink-W' + data.OrderNo + '-' + data.PO).hide()
            }
        }).fail(function (error) {
            cell.find('#cell-loading').remove();
            $('#trackingLinkModal').modal('toggle')
            console.log(error);
        });
    })

    $('.schArrDate').datepicker();

    $('#SendEmailPostOrderPlacement').click(function (evt) {
        evt.preventDefault();
        $(this).prop("disabled", true);
        // add spinner to button
        $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');
        sendEmailPostOrder();
    });

    $('.show-email-options').change(function () {
        var parent = $(this).parent().parent();
        var po = $(this).data('po');
        var vendor = $(this).data('vendor');
        var isChecked = $(this).is(":checked");
        toggleDetails(parent, isChecked, po, vendor);
    });

    $('.link-po--posto').click(function (evt) {
        var index = $(this).data('index');
        ShowDetails(index);
    });

    LoadVendors();

    $("#clipboardPOPApp").click(function () {
        $("#inputClipboard").show();
        copyToClipboard(document.getElementById("inputClipboard"));

    });

    $("#inputClipboard").focusout(function () {
        $("#inputClipboard").hide();
    });

    function copyToClipboard(elem) {
        // create hidden text element, if it doesn't already exist
        var targetId = "_hiddenCopyText_";

        var origSelectionStart, origSelectionEnd;

        // can just use the original source element for the selection and copy
        target = elem;
        origSelectionStart = elem.selectionStart;
        origSelectionEnd = elem.selectionEnd;

        // select the content
        var currentFocus = document.activeElement;

        target.focus();
        target.setSelectionRange(0, target.value.length);

        // copy the selection
        var succeed;
        try {
            succeed = document.execCommand("copy");
        } catch (e) {
            succeed = false;
        }


        // restore original focus
        if (currentFocus && typeof currentFocus.focus === "function") {
            currentFocus.focus();
        }


        // restore prior selection
        elem.setSelectionRange(origSelectionStart, origSelectionEnd);
        return succeed;
    };

    $('#inputEmailType').change(function () {
        if ($(this).val() == 'free') {
            $('#msgEmailForm').css('display', 'block');
            $('#divInputSubject').css('display', 'block');
            $('#inputSubject').attr('required', 'required');
        } else {
            $('#msgEmailForm').css('display', 'none');
            $('#divInputSubject').css('display', 'none');
            $('#inputSubject').removeAttr('required');

        }
    });

    $('button.close').click(function () {
        $('#popupColsLD').css('display', 'none');
        $('#popupColsPOS').css('display', 'none');
        $('#popupColsDET').css('display', 'none');
        $('input[name="colsToExport"]').prop('checked', true);
        //$('#checkAll').prop('checked', true);
    });


    $('#showW1D1ShippedDT').change(function () {
        // Uncheck the other toggle
        $("#showD4IWShippedDT").prop("checked", false);

        if ($(this).is(":checked")) {
            $('tr.line-detail-row').hide();
            $('tr.line-detail-row[data-w1d1="True"]').show();
        } else {
            $('tr.line-detail-row').show();
        }
    });

    $('#showD4IWShippedDT').change(function () {
        // Uncheck the other toggle
        $("#showW1D1ShippedDT").prop("checked", false);

        if ($(this).is(":checked")) {
            $('tr.line-detail-row').hide();
            $('tr.line-detail-row[data-d4iw="True"]').show();
        } else {
            $('tr.line-detail-row').show();
        }
    });

});

function checkDate(date) {
    // regular expression to match required date format
    re = /^\d{1,2}\/\d{1,2}\/\d{4}$/;

    if (date.match(re)) {
        regs = date.split('/');
        if ($.inArray(regs[0], ['01', '03', '05', '07', '08', '10', '12']) !== -1) {
            if (regs[1] < 1 || regs[1] > 31) {
                return 'day';
            }
        } else if ($.inArray(regs[0], ['04', '06', '09', '11']) !== -1) {
            if (regs[1] < 1 || regs[1] > 30) {
                return 'day';
            }
        } else if (regs[0] === '02') {
            if (!(regs[2] % 4) && (regs[2] % 100 || (!(regs[2] % 100) && !(regs[2] % 400)))) {
                if (regs[1] < 1 || regs[1] > 29) {
                    return 'day';
                }
            } else {
                if (regs[1] < 1 || regs[1] > 28) {
                    return 'day';
                }
            }
        } else {
            return 'month';
        }
        return 'ok';
    } else if (date === '') {
        return 'ok';
    } else {
        return 'format';
    }
}


function getACKValue(cbElem) {
    var rows = $(cbElem).parent().parent();
    var ackInput = '';
    if (rows.length > 0) {
        var val = rows.find('td:nth-child(5)').html().trim();
        if (val.length > 0) {
            return val;
        } else {
            return ackInput;
        }
    } else {
        return ackInput;
    }
}

function getEmailData() {
    let okToSubmit = true;
    var cbList = $('.show-email-options');
    var lines = cbList.filter(':checked');
    var emailType = $('#inputEmailType').find('option:selected').val();
    let inputSubject = ''
    switch ($('#inputEmailType').val()) {
        case 'free':
            inputSubject = $('#inputSubject').val();
            break;
        case 'ack':
            inputSubject = 'Acknowledgment Inquiry';
            break;
        case 'track':
            inputSubject = 'Tracking Inquiry';
            break;
        default:
            break;
    }
    var emailData = [];
    lines.each(function (index, elem) {
        var po = $(elem).data('po');
        var vendor = $(elem).data('vendor');
        var poDate = $(elem).parent().parent().data("podate");
        var ackValue = getACKValue(elem);
        var orderNo = $(elem).data('orderno');
        var optionsLength = $('[id^=formLine-' + po + '-' + vendor.toString().replace('&', '') + '-]').length;
        for (var i = 1; i <= optionsLength; i++) {
            var toElem = $('#toAddress-' + po + '-' + i + '-' + vendor.toString().replace('&', ''));
            if (toElem.val() == '') {
                alert('Please fill the email address on the corresponding PO #');
                okToSubmit = false;
            }
            if (emailType && emailType !== "default" && isValidVendor(vendor)) {
                emailData.push({
                    'PoSuffix': po,
                    'VendorNo': vendor,
                    'ACK': ackValue,
                    'OrderSuffixDate': poDate,
                    'EmailType': emailType,
                    'To': $(toElem).val(),
                    'InputSubject': inputSubject,
                    'OrderNo': orderNo,
                    'OkToSubmit': okToSubmit
                });
            }
        }

    });

    return emailData;
}

function getEmailDataPostOrder() {
    return {
        'IncludeTitle': $('#includeTitle').is(":checked"),
        'OrderTitle': $('#hiddenOrderTitle').val(),
        'Quote_OrderNo': $('#hiddenOrderNo').val(),
        'SalesPersonID': $('#hiddenSalesPersonID').val(),
        'CustomerName': $('#hiddenCustomerName').val(),
        'Notes': $('#inputNotes').val(),
        'ProjectID': $('#projectID').text(),
        'To': '',
        'FromName': $('#inputYourName').val(),
        'From': $('#inputYourEmail').val(),
        'CC1': $('#inputCC1').val(),
        'CC2': $('#inputCC2').val(),
        'DataToSend': getEmailData(),
        'Company': 'W'
    };
}

function sendEmailPostOrder() {
    let data = getEmailDataPostOrder();
    let dontSend = false;
    if (data.DataToSend.length == 0) {
        alert('Please select at least one PO #');
        dontSend = true;
    } else {
        data.DataToSend.forEach(function (element, index, array) {
            if (!element.OkToSubmit) {
                dontSend = true;
            }
        });
    }
    if (!dontSend) {
        $.ajax({
            type: "POST",
            url: '/postorderplacementowp/sendemail_po',
            data: getEmailDataPostOrder()
        }).done(function (result) {
            if (result) {
                ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
                clearFormPostOrder();
            } else {
                location.replace("/home/error?msg=sending");
            }
        }
        ).fail(function (error) {
            location.replace("/home/error?msg=processing");
        });
    } else {
        $('#SendEmailPostOrderPlacement').prop("disabled", false);
        $('#SendEmailPostOrderPlacement').html('Submit');
    }
}

function clearFormPostOrder() {
    $('#inputYourEmail').val("");
    $('#inputCC1').val("");
    $('#inputCC2').val("");
    $('#inputYourName').val("");
    $('#inputNotes').val("");
    $('#inputEmailType').val('ack');
    var cbList = $('.show-email-options');
    var lines = cbList.filter(':checked');
    lines.each(function (index, elem) {
        $(elem).click();
    });
    $('#SendEmailPostOrderPlacement').prop("disabled", false);
    $('#SendEmailPostOrderPlacement').html('Submit');
}

function toggleDetails(elem, isShowing, po, vendor) {
    $(elem).nextUntil('tr.breakrow').slideToggle(200);
    if (isShowing) {
        createEmailOptions(po, vendor, 1);
    } else {
        $('[id^=formLine-' + po + '-' + vendor.toString().replace('&', '') + '-' + 1 + ']').remove();
    }
}

function createEmailOptions(po, keyV, num) {
    var html = '';
    if (isValidVendor(keyV)) {
        if (num > 1) {
            $('#btnAdd-' + po + '-' + (num - 1) + '-' + keyV).hide();
            $('#btnRemove-' + po + '-' + (num - 1) + '-' + keyV).hide();
        }
        var vendor = GetEmailsVendor(keyV);
        keyV = keyV.toString().replace('&', '');
        var vendorInput = '';
        if (vendor) {
            if (vendor.addresses.length > num) {
                vendorInput = '<select class="custom-select mr-sm-2" id="toAddress-' + po + '-' + num + '-' + keyV + '">';
                for (var i = 0; i < vendor.addresses.length; i++) {
                    vendorInput += '<option value="' + vendor.addresses[i] + '">' + vendor.addresses[i] + '</option>';
                }
                vendorInput += '</select>';
            } else {
                if (vendor.addresses.length === num) {
                    vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + po + '-' + num + '-' + keyV + '" value="' + vendor.addresses[0] + '" />';
                } else {
                    vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + po + '-' + num + '-' + keyV + '" value="" />';
                }
            }
        } else {
            vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" id="toAddress-' + po + '-' + num + '-' + keyV + '" value="" />';
        }

        var phono = vendor === null ? 'Check the Vendor Email App' : (num === 1) ? vendor.phone : '';
        html = '<div id="formLine-' + po + '-' + keyV + '-' + num + '" class="form form-inline mb-3">'
            + '<div class="form-group col-md-4 toaddress-' + po + '-' + num + '-' + keyV + '">'
            + '<div class="optionsEmails"> <label for="toAddress-' + po + '-' + num + '-' + keyV + '">To:</label>'
            + vendorInput + ' </div>'
            + '</div>'
            + '<div class="phone-container col-md-5 tophone-' + po + '-' + num + '-' + keyV + '">'
            + phono + ' </div>'
            + '<div class="form-group col-md-3"><button type="button" class="btn btn-primary" id="btnAdd-' + po + '-' + num + '-' + keyV + '"'
            + ' onclick="createEmailOptions(\'' + po + '\',\'' + keyV + '\'' + ', ' + (num + 1) + ')">+</button>'
            + '<button type="button" class="btn btn-danger m-2" id="btnRemove-' + po + '-' + num + '-' + keyV + '" onclick="removeForm(\'' + po + '\', ' + num + ',\'' + keyV + '\'' + ')">-</button>'
            + '</div>';

    } else {
        html = '<div id="formLine-' + po + '-' + keyV + '-' + num + '" class="form form-inline mb-3">'
            + '<i class="pt-4 pl-4" style="color:red; font-weight:500">Please inquire via Steelcase Webtracks or through your Steelcase customer support representative.</i></div>';
    }

    $('#options-' + po + '-' + keyV).find('.container').append(html);

}
function validateEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
}

function removeForm(po, num, keyV) {
    var orderno = $('#hiddenOrderNo').val();
    if (num <= 1) {
        $('#cb-W' + orderno + '-' + po + '-' + keyV).click();
    } else {
        $('#formLine-' + po + '-' + keyV + '-' + num).remove();
        $('#btnAdd-' + po + '-' + (num - 1) + '-' + keyV).show();
        $('#btnRemove-' + po + '-' + (num - 1) + '-' + keyV).show();
    }
}

function isValidVendor(vendor) {
    return !(vendor === 'STE01' || vendor === 'ONE20' || vendor === 'ONE22' || vendor === 'ONE23'
        || vendor === 'ONE24' || vendor === 'ONE26' || vendor === 'ONE27' || vendor === 'ONE28' || vendor === 'BRA00');
}


function ShowDetails(index) {

    var title = "Lines Info W" + LinesPostOrder[index].OrderNo + "-" + LinesPostOrder[index].PoSuffix;
    var lines = LinesPostOrder[index].Lines;
    $('#detailsModalTitle').html(title);
    $('#postorderTableDetails > tbody').html(templatePostOrder(lines));
    $('#detailsModal').modal('show');
}

function templatePostOrder(data) {
    var html = '';
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        var cls = (row.ProcessCode === 'DX') ? 'row-gray' : '';
        html +=
            '<tr class="' + cls + '">'
            + '<td>' + row.LineNo + '</td>'
            + '<td>' + row.ProcessCode + '</td>'
            + '<td>' + row.LineSalesCode + '</td>'
            + '<td>' + row.Werehouse_Network + '</td>'
            + '<td>' + row.Catalog + '</td>'
            + '<td>' + row.Description + '</td>'
            + '<td>' + row.Ordered + '</td>'
            + '<td>' + row.Received + '</td>'
            + '<td>' + row.ReceivedDate + '</td>'
            + '<td>' + row.RequestedArrival + '</td>'
            + '<td>' + (row.ScheduledArrivalDate ? row.ScheduledArrivalDate : row.EstimatedArrivalDate) + '</td>'
            + '<td>' + row.ShipDate + '</td>'
            + '<td>' + row.GeneralTagging + '</td>'
            + '</tr>';
    }

    return html;
}

function LoadVendors() {

    $.ajax({
        type: "GET",
        url: '/home/GetVendorsEmails?app=POP&region=OWP&isLiveISR=false',
        async: true
    }).done(function (result) {
        VendorsEmails = result;
    });
}

function GetEmailsVendor(vendorNo) {
    if (VendorsEmails) {
        var index = binarySearchVendorIndex(VendorsEmails, vendorNo);
        if (index !== -1) {
            var vendor = VendorsEmails[index];

            return vendor;
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

function OpenPopupSelection(type) {
    if (type == 'LD') {
        $('#popupColsLD').css('display', 'block');
    } else if (type == 'POS') {
        $('#popupColsPOS').css('display', 'block');
    } else if (type == 'DET') {
        $('#popupColsDET').css('display', 'block');
    }
    $('#hdnExportType').val(type);
}

function GeneratePDF(data) {
    $('.btnExport').html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');

    var type = $('#hdnExportType').val();
    var arrCols = "";
    var strCols = "";

    if (type == 'LD')
        arrCols = $('input[name="colsLDToExport"]:checked');
    else if (type == 'POS')
        arrCols = $('input[name="colsPOSToExport"]:checked');
    else if (type == 'DET')
        arrCols = $('input[name="colsDETToExport"]:checked');

    for (var i in arrCols) {
        strCols += arrCols[i].value + ',';
    }

    data = JSON.stringify({ data: data, type: type, cols: strCols });

    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "POST",
        url: '/postorderplacementowp/LineDetailsPDF',
        data: data
    }).done(function (result) {
        $('.btnExport').html('Submit');
        window.open(result, '_blank');
        $('#popupColsLD').css('display', 'none');
        $('#popupColsPOS').css('display', 'none');
        $('#popupColsDET').css('display', 'none');
        $('input[name="colsPOSToExport"]').prop('checked', true);
        $('input[name="colsLDToExport"]').prop('checked', true);
        $('input[name="colsDETToExport"]').prop('checked', true);
    }).fail(function (error) {
        location.replace("/home/error?msg=processing");
    });

}