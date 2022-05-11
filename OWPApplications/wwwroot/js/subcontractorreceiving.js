$(document).ready(function () {

    $('#orderNo').focusout(function () {
        if ($(this).val() !== '') {
            $('#dateFrom').val('');
            $('#dateTo').val('');
        }
    });

    $('#dateFrom').focusout(function () {
        $('#orderNo').val('');
    });

    $('#dateTo').focusout(function () {
        $('#orderNo').val('');
    });

    var oldUpdateableInputValue = '';
    $('.inputQtyLineReceived').focusin(function () {
        oldUpdateableInputValue = this.value;
    });

    $('.inputQtyLineReceived').focusout(function () {
        if (oldUpdateableInputValue !== this.value) {
            var r = confirm("Please confirm the QTY you entered matches what was received at your warehouse. Failure to accurately report these quantities will result in future issues.");
            if (r == true) {
                $.ajax({
                    type: "POST",
                    url: '/SubcontractorReceiving/UpdateQtyReceived',
                    data: {
                        CompanyCode: this.dataset.compcode,
                        POSuffix: this.dataset.posuffix,
                        OrderIndex: this.dataset.orderindex,
                        LineIndex: this.dataset.lineindex,
                        NewQLRValue: this.value
                    }
                }).done(function (result) {
                    console.log(result);
                }).fail(function (error) {
                    console.log(error);
                });
            }
        }
    });

    $('#btnSubmitEmailSubcontractor').click(function (evt) {
        evt.preventDefault();
        // Check that the user filled the email To
        let emailto = $('#subYourEmail').val();
        if (!emailto || !emailto.trim()) {
            alert('Please fill out Your Email Address.');
            return;
        }

        if ($('.show-email-options').is(':checked')) {
            $(this).prop("disabled", true);
            $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');
            sendEmailSCR();
        } else {
            alert('There is no rows checked to send email.');
        }
    });

    $('#btnSubmitRequestEmailSubcontractor').click(function (evt) {
        evt.preventDefault();
        $(this).prop("disabled", true);
        $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');
        sendEmailSCR();
    });

    $('#dateFrom').datepicker({
        todayHighlight: true,
        autoclose: true
    });
    $('#dateTo').datepicker({
        todayHighlight: true,
        autoclose: true
    });

    $('#dateFrom').datepicker('setDate', localStorage.getItem("scrDateFrom") !== null ? localStorage.getItem("scrDateFrom") : getDefaultDateFrom());
    $('#dateTo').datepicker('setDate', localStorage.getItem("scrDateTo") !== null ? localStorage.getItem("scrDateTo") : getDefaultDateTo());

    $('#selAll').change(function () {
        $('.show-email-options').prop('checked', $(this).is(':checked'));
    });

    $('button.close').click(function () {
        $('#popupRequestEmail').css('display', 'none');
    });
});

function getDefaultDateFrom() {
    var dFrom = new Date();
    dFrom.setDate(dFrom.getDate() - 5);
    return dFrom;
}

function getDefaultDateTo() {
    var dTo = new Date();
    dTo.setDate(dTo.getDate() + 5);
    return dTo;
}

function sendEmailSCR() {
    $.ajax({
        type: "POST",
        url: '/subcontractorreceiving/sendemail_scr',
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        cache: false,
        data: getEmailDataSCR()
    }).done(function (response) {
        if (response.success) {
            ShowSuccessAlert({ type: 'success', title: 'Success!', message: response.responseText, autoclose: true });
            clearEmailFormSCR();            
        } else {            
            location.replace("/home/error?msg=sending");
        }
    }).fail(function (error) {
        location.replace("/home/error?msg=processing");
    });
}

function getEmailDataSCR() {
    var data = [];
    data.push({
        'EmailType': $('#hdnEmailType').val(),
        'SubcontractorName': $('#inputYourName').val(),
        'POReference': $('#hdnPOReference').val(),
        'ProductVendor': $('#hdnProductVendor').val(),
        'Date': $('#hdnDate').val(),
        'Notes': $('#inputNotes').val(),
        'To': $('#subYourEmail').val(),
        'FromName': $('#inputYourName').val(),
        'From': $('#subYourEmail').val(),
        'CC1': $('#inputCC1').val(),
        'CC2': $('#inputCC2').val(),
        'LinesData': getEmailLineDataSCR(),
        'IssueType': $('#inputIssueType').find('option:selected').val(),
        'IssueDetail': $('#inputIssueDetail').find('option:selected').val(),
        'Quantity': $('#inputQty').val(),
        'Description': $('#inputDesc').val(),
        'VendorName': $('#hdnVendorName').val(),
        'PORef': $('#hdnPORef').val()
    });

    var form = new FormData();
    form.append('rawdata', JSON.stringify(data));
    if ($('#inputAttachment')[0] !== undefined) {
        var file = $('#inputAttachment')[0].files[0];
        if (file) {
            form.append('file', file);
        }
    }

    return form;
}

function getEmailLineDataSCR() {
    var cbList = $('.show-email-options');
    var lines = cbList.filter(':checked');
    var emailData = [];
    lines.each(function (index, elem) {
        var lineNo = $(elem).data('line');
        var orderIndex = $(elem).data('orderindex');
        var orderNo = $(elem).data('orderno');
        var qty = $(elem).data('qty');
        var poSuffix = $(elem).data('posuffix');
        var vendorName = $(elem).data('vendorname');
        var productVendor = $(elem).data('prodvendor');
        var lineIndex = $(elem).data('lineindex');
        var lineQtyReceived = $('.inputQtyLineReceived[data-lineindex="' + lineIndex + '"]').val();
        var lqrWasModified = qty != lineQtyReceived;
        emailData.push({
            'LineNo': lineNo,
            'Qty': qty,
            'VendorName': vendorName,
            'POSuffix': poSuffix,
            'ProductVendor': productVendor,
            'LineQtyReceived': lineQtyReceived,
            'LQRWasModified': lqrWasModified,
            'OrderIndex': orderIndex,
            'OrderNo': orderNo
        });
    });

    return emailData;
}

function clearEmailFormSCR() {
    $('#inputEmailType').val('155').change();
    $('#inputIssueType').val('159').change();
    $('#inputIssueDetail').val('166').change();
    $('#subYourEmail').val($('#hdnUserEmail').val());
    $('#inputCC1').val($('#hdnUserEmail').val());
    $('#inputCC2').val('');
    $('#inputYourName').val('');
    $('#inputNotes').val('');
    $('#inputQty').val('');
    $('#inputDesc').val('');
    $('#inputAttachment').val('');
    $('#reportIssueForm').css('display', 'none');
    var cbList = $('.show-email-options');
    var lines = cbList.filter(':checked');
    lines.each(function (index, elem) {
        $(elem).click();
    });
    $('#btnSubmitEmailSubcontractor').prop("disabled", false);
    $('#btnSubmitEmailSubcontractor').html('Submit');
    $('#btnSubmitRequestEmailSubcontractor').prop("disabled", false);
    $('#btnSubmitRequestEmailSubcontractor').html('Submit');
    $('#popupRequestEmail').css('display', 'none');
    $('#selAll').prop('checked', false);
}

function setDates() {
    localStorage.setItem("scrDateFrom", $('#dateFrom').val());
    localStorage.setItem("scrDateTo", $('#dateTo').val());
}

function openPopupRequestEmail(type, vendorName, poRef) {
    $('#hdnEmailType').val(type);
    $('#hdnVendorName').val(vendorName);
    $('#hdnPORef').val(poRef);
    $('#popupRequestEmail').css('display', 'block');    
}