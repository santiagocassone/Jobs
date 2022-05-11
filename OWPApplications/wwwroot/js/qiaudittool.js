$(document).ready(function () {


    //
    // Enable compare button once a file has been selected
    //
    $("input:file").change(function () {
        $('#submitFilesForComparison').prop('disabled', false);
    });

    // 
    // Adjust the textarea size to match table.td height
    //
    $('.line-user-comment').each(function (index) {
        var height = $(this).closest('td').height();
        $(this).height(height);
    });



    function clearForm() {
        $('#inputYourEmail').val('');
        $('#inputYourName').val('');
        $('#inputSendToEmail').val('');
        $('#inputCC1').val('');
        $('#inputCC2').val('');
    }


    $(document).on('click', function () {
        $('.collapse').collapse('hide');
    })

    $('#SendEmailComparisonResults').click(function (evt) {
        // Cancel submit
        evt.preventDefault();



        // Check Form validation (cancel email delivery if form does not validates)
        if (!$(evt.target).closest("form").get(0).reportValidity()) {
            return false;
        }



        // Extract the HTML for the comparison table
        var origTable = $('#qi-audit-results-table').html();
        var comparisonTable = $('<div>', { html: origTable })
            .find('.prev-commments-box')
            .replaceWith(function () { return '' })
            .end()
            .find('.prev-commments-button')
            .replaceWith(function () { return '' })
            .end()
            .find('textarea')
            .replaceWith(function () {
                var comment = $(`#${this.id}`).val();
                if (comment) {
                    return '<div class="email-highlight">' + comment + '</div>';
                } else {
                    return '<div>' + comment + '</div>';
                }

            })
            .end()
            .html();

        // Extract comments information
        const comments = $('.line-user-comment').get().filter((v) => (v.value ? v.value.trim() : '') !== '').map((v) => { return { value: v.value, lineno: v.dataset.lineNo } });


        $.ajax({
            type: "POST",
            url: '/qiaudittool/sendemail',
            cache: false,
            data: {
                FromAddress: $('#inputYourEmail').val(),
                FromName: $('#inputYourName').val(),
                To: $('#inputSendToEmail').val(),
                CC1: $('#inputCC1').val(),
                CC2: $('#inputCC2').val(),
                QuoteNO: $('#hiddenOrderNo').val(),
                ComparisonTable: comparisonTable,
                Comments: comments
            }
        }).done(function (result) {
            if (result) {
                ShowSuccessAlert({ type: 'success', title: 'Success!', message: 'Your request has been submitted for processing.', autoclose: true });
                clearForm();
            } else {
                location.replace("/home/error?msg=sending");
            }
        }
        ).fail(function (error) {
            location.replace("/home/error?msg=processing");
        });
    });


});