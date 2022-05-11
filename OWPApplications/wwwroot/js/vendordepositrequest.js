$(document).ready(function () {

    $('#vdrform').on('submit', function () {
        ShowLoading();
    });

    $('#depositDueDate').datepicker({});

    $('#depositTerms').change(function () {
        let totalCost = $('#totalCost').text().replace('$', '').trim();

        switch ($(this).val()) {
            case '0':
                $("#depositTermsCustom").prop('required', false);
                $('#customDTField').hide();
                $('#depositAmt').val($('#totalCostWithPA').text());
                break;
            case '1':
                $("#depositTermsCustom").prop('required', false);
                $('#customDTField').hide();
                $('#depositAmt').val(addCommas('$ ' + Math.round(parseFloat(totalCost.replace(',', '')) * 0.5 * 100) / 100));
                break;
            case '2':
                $("#depositTermsCustom").prop('required', false);
                $('#customDTField').hide();
                $('#depositAmt').val(addCommas('$ ' + Math.round(parseFloat(totalCost.replace(',', '')) * 0.4 * 100) / 100));
                break;
            case '3':
                $("#depositTermsCustom").prop('required', false);
                $('#customDTField').hide();
                $('#depositAmt').val(addCommas('$ ' + Math.round(parseFloat(totalCost.replace(',', '')) * 0.33 * 100) / 100));
                break;
            case '4':
                $("#depositTermsCustom").prop('required', false);
                $('#customDTField').hide();
                $('#depositAmt').val($('#totalCostWithPA').text());
                break;
            case '5':
                $('#customDTField').show();
                $("#depositTermsCustom").prop('required', true);
                $('#depositTermsCustom').val('');
                $('#depositAmt').val('');
                break;
            default:
                $("#depositTermsCustom").prop('required', false);
                $('#customDTField').hide();
                $('#depositAmt').val('');
                break;
        }
    });

    $('#SendEmailVDR').click(function (evt) {
        evt.preventDefault();
        if (!$(evt.target).closest("form").get(0).reportValidity()) {
            return false;
        }
        $(this).prop("disabled", true);
        // add spinner to button
        $(this).html(
            '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...'
        );
        sendEmailVDR();
    });

    function sendEmailVDR() {

        $.ajax({
            type: "POST",
            url: '/VendorDepositRequest/SendEmail_VDR',
            enctype: 'multipart/form-data',
            processData: false,
            contentType: false,
            cache: false,
            data: getEmailDataVDR()
        }).done(function (result) {
            if (result === 'OK') {
                ShowSuccessAlertVDR({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
            } else {
                location.replace("/home/error?msg=sending");
            }
        }
        ).fail(function (error) {
            location.replace("/home/error?msg=processing");
        });
    }

    function getEmailDataVDR() {
        var data = [];
        data.push({
            'To': 'bferrari@oneworkplace.com',
            'From': $('#inputYourEmail').val(),
            'CC1': $('#inputCC1').val(),
            'CC2': $('#inputCC2').val(),
            'YourName': $('#inputYourName').val(),
            'DueDate': $('#depositDueDate').val(),
            'Order': $('#orderPO').text(),
            'Vendor': $('#vendorName').text(),
            'DepositTerms': $("#depositTerms option:selected").text(),
            'AmtDue': $("#depositAmt").val(),
            'Notes': $('#inputNotes').val(),
            'CustomDepositTerms': $('#depositTermsCustom').val(),
            'PreviouslyPaid': $('#previouslyPaid').text(),
            'POTotal': $('#totalCost').text(),
            'PaymentType': $("#paymentType option:selected").text(),
            'CurrencyType': $("#currencyType option:selected").text(),
            'CurrencyCustom': $('#inputCustomCurrency').val(),
            'CostVerified': $('#costVerified').text()
        });

        var form = new FormData();
        form.append('rawdata', JSON.stringify(data));
        var file = $('#fileAttachment')[0].files[0];
        if (file) {
            form.append('file', file);
        }

        return form;
    }

    function ShowSuccessAlertVDR(props) {
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
                location.replace("/vendordepositrequest/index");
            }, 4000);
        }
    }

    function addCommas(nStr) {
        nStr += '';
        x = nStr.split('.');
        x1 = x[0];
        x2 = x.length > 1 ? '.' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1)) {
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        }
        return x1 + x2;
    }

    $("#depositTermsCustom").change(function () {
        var val = Math.abs(parseInt(this.value, 10) || 0);
        this.value = val > 100 ? 100 : val;
        let totalCost = $('#totalCost').text().replace('$', '').trim();
        $('#depositAmt').val(addCommas('$ ' + Math.round(parseFloat(totalCost.replace(',', '')) * this.value / 100 * 100) / 100));
    });

    $('#currencyType').change((elem) => {
        if (elem.currentTarget.value === '3') {
            $("#inputCustomCurrency").prop('required', true);
            $('#customCurrencyDiv').show();
        } else {
            $('#customCurrencyDiv').hide();
            $("#inputCustomCurrency").prop('required', false);
            $("#inputCustomCurrency").val('');
        }
    });

});

