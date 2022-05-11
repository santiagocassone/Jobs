$(document).ready(function () {
    $('.stgNameSelectPicker').selectpicker();

    $('.stgNameSelectPicker').on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) {
        var names = [];
        var a = $(this).children("option:selected").each(function () {
            names.push($(this).val());
        });
        var orderno = $(this).data().orderno;
        $.ajax({
            type: "POST",
            url: '/home/UpdateStagingNames',
            data: { names, orderno }
        }).done(function (result) {
            if (result === 'Ok') {
                ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
            } else {
                location.replace("/home/error?msg=sending");
            }
        }
        ).fail(function (error) {
            location.replace("/home/error?msg=processing");
        });

    });

    $('#SendEmailWarehouseDT').click(function (evt) {
        evt.preventDefault();

        $(this).prop("disabled", true);
        $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading...');        

        var arrOrd = $('.ordCheck').filter(':checked');

        if (arrOrd.length <= 0) {
            alert("There is no rows checked to send email.");
            $('#SendEmailWarehouseDT').prop("disabled", false);
            $('#SendEmailWarehouseDT').html('Submit');
            return;
        } else {
            var data = {
                'From': $('#wdtYourEmail').val(),
                'Name': $('#inputYourName').val(),
                'CC1': $('#inputCC1').val(),
                'CC2': $('#inputCC2').val(),
                'Comments': $('#inputComments').val(),
                'InstallDate': $('#searchDate').val(),
                'DataToSend': [],
            };

            arrOrd.each(function (index, elem) {
                data.DataToSend.push({
                    'OrderNo': $(elem).data('orderno'),
                    'CustName': $(elem).data('customername')
                })
            });


            $.ajax({
                type: "POST",
                url: '/home/sendemails_wdt',
                data: data
            }).done(function (result) {
                if (result) {
                    ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
                    $('#wdtYourEmail').val("");
                    $('#inputCC1').val("");
                    $('#inputCC1').val("");
                    $('#inputComments').val("");
                    $('#inputYourName').val("");
                    $('#SendEmailWarehouseDT').prop("disabled", false);
                    $('#SendEmailWarehouseDT').html('Submit');
                } else {
                    location.replace("/home/error?msg=sending");
                }
            }
            ).fail(function (error) {
                location.replace("/home/error?msg=processing");
            });
        }

        
    });
});