$(document).ready(function () {

    function scrollToDiv(divID) {
        var aTag = $(`div#${divID}`);
        $('html,body').animate({ scrollTop: aTag.offset().top  }, 'slow');
    }

    $('#inputOrderDate').datepicker({});
    $('#inputReturnInfoBy').datepicker({});


    $('#SendEmailQouteInquiryOSQ').click(function (evt) {
        evt.preventDefault();

        if (!$(evt.target).closest("form").get(0).reportValidity()) {
            return false;
        }

        var missingEmail = false;
        if ($(".vendors-cb:checked").length == 0) {
            alert("Please select vendor(s) to send email.");
            return false;
        } else {
            var checkedVendors = $(".vendors-cb:checked");
            checkedVendors.each(function () {
                var vendor = $(this).data('vendor');
                if (($("#vendorToAddress_" + vendor + " option:selected").length == 0 && $("#vendorToAddressFreeform_" + vendor).val() == "")
                    || $("#vendorToAddressSingleFreeform_" + vendor).val() == "") {
                    missingEmail = true;
                    console.log('asd', vendor);
                    $(`#vendor-${vendor}-missing-email-text`).show();
                } else {
                    $(`#vendor-${vendor}-missing-email-text`).hide();
                }
            });
        }
        if (missingEmail) {
            scrollToDiv('VendorQuoteRequestSection');
            return false;
        }
        $(this).prop("disabled", true);
        $(this).html(
            '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...'
        );

        sendEmailQuoteInquiry();
    });

    $('input[name="chkQIVendorOSQ"]').change(function () {
        console.log('asd', $(this).is(':checked'));

        if ($('input[name="chkQIVendorOSQ"]:checked').length == 1 && $('input[name="chkQIVendorOSQ"]:checked').attr('data-fzc')) {
            $('#inputZipCode').prop('required', false);
            $('#inputZipCode').prop('disabled', true);
        } else {
            $('#inputZipCode').prop('required', true);
            $('#inputZipCode').prop('disabled', false);
        }

        var vendor = $(this).data('vendor');
        //if ($(this).is(':checked')) {
        //    if ($("#vendorToAddress_" + vendor + " option").length == 0) {
        //        $(`#vendor-${vendor}-missing-email-text`).show();
        //    } else {
        //        $(`#vendor-${vendor}-missing-email-text`).hide();
        //    }
        //}

        if (!($(this).is(':checked'))) {
            $(`#vendor-${vendor}-missing-email-text`).hide();
        }
    });

    $('.vendors-cb').click(function () {
        var checked = ($(this).is(":checked"));
        var vendor = $(this).data('vendor');
        if (checked) {
            $('.picker_' + vendor).show();
            $('#vendorToAddressSingleFreeform_' + vendor).show();
            //$('#vendorToAddressFreeform_' + vendor).show();
            $('#fileAttachmentDiv_' + vendor).show();
            $('#btnAdd-' + vendor).show();
        } else {
            $('#vendorToAddress_' + vendor).hide();
            $('.picker_' + vendor).hide();
            $('#btnAdd-' + vendor).hide();
            $('#fileAttachmentDiv_' + vendor).hide();
            $('#vendorToAddressFreeform_' + vendor).hide();
        }
        setRequestedItemsList();
    });

    $('.selVendorAddresses').change(function () {
        setRequestedItemsList();
    });

    $('.custom-pickerselect').hide();

});

function setRequestedItemsList() {
    var chkElems = $('.vendors-cb:checkbox:checked');
    var checkedVendors = [];
    for (var i = 0; i < chkElems.length; i++) {
        checkedVendors.push($(chkElems[i]).data('vendor'));
    }
    for (var i = 0; i < checkedVendors.length; i++) {
        var selectedEmails = getVendorToAddress(checkedVendors[i]);
        var standardEmails = selectedEmails.find(x => !x.includes("(A)"));
        if (standardEmails !== undefined && standardEmails.length > 0) {
            $('.standardReqItems').css('display', 'block');
            $('.standardReqItems input[type="checkbox"]').prop('checked', true);
            return;
        } else {
            $('.standardReqItems').css('display', 'none');
            $('.standardReqItems input[type="checkbox"]').prop('checked', false);
        }
    }
}

function sendEmailQuoteInquiry() {

    $.ajax({
        type: "POST",
        url: '/QuoteInquiryOSQ/SendEmail_QI',
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
                'VendorNo': vendor,
                'VendorName': name,
                'Notes': $('#inputNotes').val(),
                'SpecifiedAndLocation': $('#inputSpecifiedAndLocation').val(),
                'ProjectNameAndLocation': $('#inputProjectNameAndLocation').val(),
                'ZipCode': $('#inputZipCode').val(),
                'AnticipatedDate': $('#inputOrderDate').val(),
                'ReturnInfoBy': $('#inputReturnInfoBy').val(),
                'Company': $('#inputCompany').val(),
                'Title': $('#inputTitle').val(),
                'PhoneNo': $('#inputPhoneNo').val(),
                'From': $('#inputYourEmail').val(),
                'CC1': $('#inputCC1').val(),
                'CC2': $('#inputCC2').val(),
                'RFP_BID': $('#rfp_bid').is(":checked"),
                'ExcludeCustomerName': $('#excludeCustomerName').is(":checked"),
                'ExcludeTitle': $('#excludeTitle').is(":checked"),
                'Lines': getVendorLines(vendor),
                'CustomerName': $('#hiddenCustomerName').val(),
                'YourName': $('#inputYourName').val(),
                'InputListPrice': $('#chkListPrice').is(":checked"),
                'InputNetOrDiscountOffList': $('#chkNetOrDiscountOffList').is(":checked"),
                'InputFreightEstimate': $('#chkFreightEstimate').is(":checked"),
                'InputCOMApproval': $('#chkCOMApproval').is(":checked"),
                'InputQuoteAsCOM': $('#chkQuoteAsCOM').is(":checked"),
                'InputQuoteAsGradedIn': $('#chkQuoteAsGradedIn').is(":checked"),
                'InputCOMYardageRequirements': $('#chkCOMYardageRequirements').is(":checked"),
                'InputAnyAdditionalCharges': $('#chkAnyAdditionalCharges').is(":checked"),
                'InputCurrentLeadTime': $('#chkCurrentLeadTime').is(":checked"),
                'InputWarrantyInfo': $('#chkWarrantyInfo').is(":checked"),
                'InputUpcomingPriceChangesAnticipated': $('#chkUpcomingPriceChangesAnticipated').is(":checked"),
                'InputWhatIsMissingForACompleteSpec': $('#chkWhatIsMissingForACompleteSpec').is(":checked"),
                'InputFabricTestingRequired': $('#chkFabricTestingRequired').is(":checked"),
                'InputShipToForCOM': $('#chkShipToForCOM').is(":checked"),
                'InputConfirmComShip': $('#chkConfirmComShip').is(":checked"),
                'InputEnvironmentalDataOrCertifications': $('#chkEnvironmentalDataOrCertifications').is(":checked"),
                'InputCostsAndLeadTimeForAirFreight': $('#chkCostsAndLeadTimeForAirFreight').is(":checked"),
                'InputDepositRequirements': $('#chkDepositRequirements').is(":checked"),
                'InputPaymentTerms': $('#chkPaymentTerms').is(":checked"),
                'Is61': $('#Is61').val(),
                'ProjectID': $('#projectid').val()
            });
        }
    });

    var form = new FormData();
    form.append('rawdata', JSON.stringify(data));

    $('.vendorAtt').each(function (i, e) {
        var vendor = $(e).data('vendor');
        var file = $('#fileAttachment_' + vendor)[0].files[0];
        if (file) {
            form.append('file', file, vendor + '##' + file.name);
        }
    });


    return form;
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
            'LineNo': $(this.cells[0]).html(),
            'Catalog': $(this.cells[2]).html(),
            'GeneralTagging': $(this.cells[3]).html(),
            'Description': $(this.cells[5]).html(),
            'QtyOrdered': $(this.cells[4]).html()
        });
    });
    return lines;
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
    $('#inputProjectNameAndLocation').val("");
    $('#inputReturnInfoBy').val("");
    $('#inputYourName').val("");
    $('#inputTitle').val("");
    $('#inputCompany').val("");
    $('#inputPhoneNo').val("");
    $('#SendEmailQouteInquiryOSQ').prop("disabled", false);
    $('#SendEmailQouteInquiryOSQ').html('Submit');
    $('.vendorToAddressFreeform').val("");
    $('.toAddress').val("");
    $('.reqItemsFromVendors input[type="checkbox"]').prop('checked', true);
    $('#chkQuoteAsGradedIn').prop('checked', false);
    $('#excludeCustomerName').prop('checked', false);
    $('#excludeTitle').prop('checked', false);
    $('#rfp_bid').prop('checked', false);
}

function addFreeForm(elem, vendor) {
    $(elem).hide();
    $('#vendorToAddressFreeform_' + vendor).show();
}