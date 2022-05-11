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
    //$('#inputWeekDayField').datepicker({});
    $('.schArrDate').datepicker({});

    $('.emailTypeFields_calendar').datepicker({});

    $('.link-po').click(function () {
        var index = $(this).data('index');
        fastTrackShowDetails(index);
    });

    $('#SendEmailPostOrderPlacement').click(function (evt) {
        evt.preventDefault();
        $(this).prop("disabled", true);
        $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Submitting...');
        sendEmailPostOrder();
    });

    $('.show-email-options').change(function () {
        var parent = $(this).parent().parent();
        var po = $(this).data('po');
        var vendor = $(this).data('vendor');
        var isChecked = $(this).is(":checked");
        var isSteelcaseVendor = ($(this).data('issteelcase').toLowerCase() == "true");
        toggleDetails(parent, isChecked, po, vendor, isSteelcaseVendor);
    });

    $('.link-po--posto').click(function (evt) {
        var index = $(this).data('index');
        ShowDetails(index);
    });

    $("#clipboardPOPApp").click(function () {
        $("#inputClipboard").show();
        copyToClipboard(document.getElementById("inputClipboard"));

    });

    $("#inputClipboard").focusout(function () {
        $("#inputClipboard").hide();
    });

    $('#inputEmailType').change(function () {
        setupForm();

    });
    $('button.close').click(function () {
        $('#popupColsLD').css('display', 'none');
        $('#popupColsPOS').css('display', 'none');
        $('input[name="colsToExport"]').prop('checked', true);
    });    

    $('#processCodeFilter').change(function () {
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

    setupForm();
});

function setupForm() {
    if ($('#inputEmailType').val() == 'free') {
        $('#msgEmailForm').css('display', 'block');
        $('#divInputSubject').css('display', 'block');
    } else {
        $('#msgEmailForm').css('display', 'none');
        $('#divInputSubject').css('display', 'none');
    }

    var className = "emailTypeFields_" + $('#inputEmailType').val();
    $('.divCustomField').hide();
    $('.' + className).show();
}

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
}

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
    var inputSubject = $('#inputSubject').val();
    var emailData = [];
    lines.each(function (index, elem) {
        var po = $(elem).data('po');
        var vendor = $(elem).data('vendor');
        var isSteelcaseVnd = ($(elem).data('issteelcase').toLowerCase() == 'true');
        var poDate = $(elem).parent().parent().data("podate");
        var schedarrival = $(elem).parent().parent().data("schedarrival");
        var estarrival = $(elem).parent().parent().data("estarrival");
        var ackValue = getACKValue(elem);
        var orderNo = $(elem).data('orderno');
        var optionsLength = $('[id^=formLine-' + po + '-' + vendor.toString().replace('&', '') + '-]').length;
        for (var i = 1; i <= optionsLength; i++) {
            var sTo = $('#toAddress-' + po + '-' + i + '-' + vendor.toString().replace('&', '')).val();
            if (sTo == '') {
                alert('Please fill the email address on the corresponding PO #');
                okToSubmit = false;
            }
            if (emailType && emailType !== "default" && !isSteelcaseVnd) {
                emailData.push({
                    'PoSuffix': po,
                    'VendorNo': vendor,
                    'ACK': ackValue,
                    'EstimatedArrival': schedarrival,
                    'ScheduledArrivalDate': estarrival,
                    'OrderSuffixDate': poDate,
                    'EmailType': emailType,
                    'To': sTo,
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
    var ret = {
        'IncludeTitle': $('#includeTitle').is(":checked"),
        'OrderTitle': $('#hiddenOrderTitle').val(),
        'Quote_OrderNo': $('#hiddenOrderNo').val(),
        'SalesPersonID': $('#hiddenSalesPersonID').val(),
        'CustomerName': $('#hiddenCustomerName').val(),
        'Notes': $('#inputNotes').val(),
        //'WeekDayField': $('#inputWeekDayField').val(),
        'ProjectID': $('#projectID').text(),
        'To': '',
        'FromName': $('#inputYourName').val(),
        'From': $('#inputYourEmail').val(),
        'CC1': $('#inputCC1').val(),
        'CC2': $('#inputCC2').val(),
        'CompanyCode': $('#hdnCompanyCode').val(),
        'DataToSend': getEmailData(),
        'Company': 'S'
    };
    ret.customFields = [];
    var className = "emailTypeFields_" + $('#inputEmailType').val();
    $('.' + className).each(function (index) {
        var oIn = $(this).find('input')[0];
        ret.customFields.push({ 'name': $(oIn).attr('name'), 'value': $(oIn).val() });
    });

    return ret;
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
            url: '/postorderplacementosq/sendemail_po',
            data: data
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
    //$('#inputWeekDayField').val("");
    $('#inputCustomField').val("");
    $('#inputEmailType')[0].selectedIndex = 0;
    $("#inputEmailType").change();
    setupForm();
    var cbList = $('.show-email-options');
    var lines = cbList.filter(':checked');
    lines.each(function (index, elem) {
        $(elem).click();
    });
    $('#SendEmailPostOrderPlacement').prop("disabled", false);
    $('#SendEmailPostOrderPlacement').html('Submit');
}

function toggleDetails(elem, isShowing, po, vendor, isSteelcaseVnd) {
    $(elem).nextUntil('tr.breakrow').slideToggle(200);
    if (isShowing) {
        createEmailOptions(po, vendor, 1, isSteelcaseVnd);
    } else {
        $('[id^=formLine-' + po + '-' + vendor.toString().replace('&', '') + '-' + 1 + ']').remove();
    }
}

function createEmailOptions(po, keyV, num, isSteelcaseVnd) {
    var html = '';
    /*if (isSteelcaseVnd.toLowerCase() != 'true') {*/
    if (!isSteelcaseVnd) {
        if (num > 1) {
            $('#btnAdd-' + po + '-' + (num - 1) + '-' + keyV).hide();
            $('#btnRemove-' + po + '-' + (num - 1) + '-' + keyV).hide();
            $('#btnEdit-' + po + '-' + (num - 1) + '-' + keyV).hide();
        }
        var vendor = GetVendorEmails(keyV);
        keyV = keyV.toString().replace('&', '');
        var vendorInput = '';
        var addEditbutton = true;
        var addRemovebutton = (num > 1);
        if (vendor) {
            if (vendor.addresses.length >= num) {
                vendorInput = '<select class="custom-select mr-sm-2" style="margin-left:8px;width:310px" id="toAddress-' + po + '-' + num + '-' + keyV + '">';
                for (var i = 0; i < vendor.addresses.length; i++) {
                    vendorInput += '<option value="' + vendor.addresses[i] + '">' + vendor.addresses[i] + '</option>';
                }
                vendorInput += '</select>';
            } else {
                addEditbutton = false;
                //if (vendor.addresses.length === num) {
                //    vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" style="width:350px !important" id="toAddress-' + po + '-' + num + '-' + keyV + '" value="' + vendor.addresses[0] + '" />';
                //} else {
                vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" style="width:350px !important" id="toAddress-' + po + '-' + num + '-' + keyV + '" value="" />';
                //}
            }
        } else {
            addEditbutton = false;
            vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" style="width:350px !important" id="toAddress-' + po + '-' + num + '-' + keyV + '" value="" />';
        }

        html = '<div id="formLine-' + po + '-' + keyV + '-' + num + '" class="form form-inline mb-3">'
            + '<div class="form-group col-md-4 toaddress-' + po + '-' + num + '-' + keyV + '">'
            + '<div class="optionsEmails"> <label for="toAddress-' + po + '-' + num + '-' + keyV + '">To: </label>'
            + vendorInput + ' </div>'
            + '</div>'
            + '<div class="form-group col-md-3"><button type="button" class="btn btn-primary" id="btnAdd-' + po + '-' + num + '-' + keyV + '"'
            + ' onclick="createEmailOptions(\'' + po + '\',\'' + keyV + '\'' + ', ' + (num + 1) + ',' + (isSteelcaseVnd ? 'true' : 'false') + ')" title="Add New Email">+</button>'
            + (addRemovebutton ? '<button type="button" class="btn btn-danger m-2" id="btnRemove-' + po + '-' + num + '-' + keyV + '" onclick="removeForm(\'' + po + '\', ' + num + ',\'' + keyV + '\'' + ')" title="Remove">-</button>' : '')
            + (addEditbutton ? '<button type="button" class="btn btn-info m-2" id="btnEdit-' + po + '-' + num + '-' + keyV + '" onclick="swichToEdit(\'' + po + '\', ' + num + ',\'' + keyV + '\'' + ')" title="Switch To Edit">Edit</button>' : '')
            + '</div>';

    } else {
        html = '<div id="formLine-' + po + '-' + keyV + '-' + num + '" class="form form-inline mb-3">'
            + '<i class="pt-4 pl-4" style="color:red; font-weight:500">Please inquire via Steelcase Webtracks or through your Steelcase customer support representative.</i></div>';
    }

    $('#options-' + po + '-' + keyV).find('.container').append(html);

}
function swichToEdit(po, num, keyV) {
    var parent = $('#toAddress-' + po + '-' + num + '-' + keyV).parent();
    var value = $('#toAddress-' + po + '-' + num + '-' + keyV).val();
    $('#toAddress-' + po + '-' + num + '-' + keyV).remove();
    var vendorInput = '<input type="email" class="form-control ml-sm-2 w-85" style="width:350px !important" id="toAddress-' + po + '-' + num + '-' + keyV + '" value="' + value + '" />';
    $(parent).append(vendorInput);
    $('#btnEdit-' + po + '-' + num + '-' + keyV).hide();
}
function removeForm(po, num, keyV) {
    var orderno = $('#hiddenOrderNo').val();
    var companyCode = $('#hdnCompanyCode').val();
    if (num <= 1) {
        $('#cb-' + companyCode + orderno + '-' + po + '-' + keyV).click();
    } else {
        $('#formLine-' + po + '-' + keyV + '-' + num).remove();
        $('#btnAdd-' + po + '-' + (num - 1) + '-' + keyV).show();
        $('#btnRemove-' + po + '-' + (num - 1) + '-' + keyV).show();
    }
}

function ShowDetails(index) {
    var companyCode = $('#hdnCompanyCode').val();
    var title = "Lines Info " + companyCode + LinesPostOrder[index].OrderNo + "-" + LinesPostOrder[index].PoSuffix;
    var lines = LinesPostOrder[index].Lines;
    let proj = LinesPostOrder[index];
    $('#detailsModalTitle').html(title);
    $('#postorderTableDetails > tbody').html(templatePostOrder(lines, proj));
    $('#detailsModal').modal('show');
}

function templatePostOrder(data, proj) {
    var html = '';
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        html +=
            '<tr>'
            + '<td>' + row.LineNo + '</td>'
            + '<td>' + row.ProcessCode + '</td>'
            + '<td>' + row.LineSalesCode + '</td>'
            + '<td>' + proj.VendorName + '(' + proj.VendorNo.trim() + ')</td>'
            + '<td>' + row.Catalog + '</td>'
            + '<td>' + row.Description + '</td>'
            + '<td>' + row.Ordered + '</td>'
            + '<td>' + row.Received + '</td>'
            + '<td>' + row.ReceivedDate + '</td>'
            + '<td>' + row.RequestedArrival + '</td>'
            + '<td>' + row.EstimatedArrivalDate + '</td>'
            + '<td>' + row.ShipDate + '</td>'
            + '<td>' + row.GeneralTagging + '</td>'
            + '<td>' + proj.LatestSoftSchDt + '</td>'
            + '</tr>';
    }

    return html;
}

function GetVendorEmails(vendorNo) {
    var ret;
    $.ajax({
        type: "GET",
        url: '/postorderplacementosq/getvendorbyno?vendorno=' + vendorNo,
        async: false,
        dataType: 'json',
    }).done(function (response) {
        ret = response;
    });

    return ret;
}

function OpenPopupSelection(type) {
    if (type == 'LD') {
        $('#popupColsLD').css('display', 'block');
    } else {
        $('#popupColsPOS').css('display', 'block');
    }
    $('#hdnExportType').val(type);
}

function GeneratePDF(data) {
    $('.btnExport').html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');

    var type = $('#hdnExportType').val();
    var arrCols = type == 'LD' ? $('input[name="colsLDToExport"]:checked') : $('input[name="colsPOSToExport"]:checked');
    var strCols = "";

    for (var i in arrCols) {
        strCols += arrCols[i].value + ',';
    }

    data = JSON.stringify({
        data: data, type: type, cols: strCols, excludeLines: $('#processCodeFilter').is(':checked'), ProjectID: $('#projectID').val(), CustomerName: $('#customerName').val(), Location_Code: $('#locationCode').val(), SearchBy: $('#projectidpoposq').val()
    });

    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "POST",
        url: '/PostOrderPlacementOSQ/LineDetailsPDF',
        data: data
    }).done(function (result) {
        $('.btnExport').html('Submit');
        if (result === 'POS') {
            window.open('../files/pop/POPReportPOSuffixLineDetails.pdf');//, '_blank');
            $('#posReport').html('PO Suffix/Line Detail');
        } else if (result === "LD") {
            window.open('../files/pop/POPReportSummary.pdf');//, '_blank');
            $('#ldReport').html('PO Summary');
        } else {
            location.replace("/home/error?msg=sending");
        }

        $('#popupColsLD').css('display', 'none');
        $('#popupColsPOS').css('display', 'none');
        $('input[name="colsPOSToExport"]').prop('checked', true);
        $('input[name="colsLDToExport"]').prop('checked', true);
    }).fail(function (error) {
        location.replace("/home/error?msg=processing");
    });
}

function fastTrackShowDetails(index) {
    var title = "Lines Info " + LinesFastTrack[index].PO;
    var lines = LinesFastTrack[index].LinesInfo;
    $('#detailsModalTitle').html(title);
    $('#fasttrackTableDetails > tbody').html(templateFasTrack(lines));
    $('#detailsModal').modal('show');
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