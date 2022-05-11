$(document).ready(function ()  {

    $('#popCustomerMultiselect').selectpicker();

    $('#popdashboardform').on('submit', function () {
        ShowLoading();
    });

    $('#salespersonSelect').change(function () {

        // Get the newly selected Salesperson ID
        var salesperson = $(this).val();

        // Disable customer multiselect until is loaded
        $('#popCustomerMultiselect').prop('disabled', true);

        $.get("/popdashboard/salespersoncustomers", {
            salespersonid: salesperson, region: $('#regionpopdash').val() })
            .done(function (data) {
                console.log('customers reloaded');
                // Build the list of Select Option tags
                var customersOptions = data.map(function (option) {
                    return `<option value=${option.id}>${option.label}</option>`;
                });

                // Insert them into the Select component
                $('#popCustomerMultiselect').selectpicker('refresh').empty().append(customersOptions).selectpicker('refresh');
                // Mark all as selected
                $('#popCustomerMultiselect').selectpicker('selectAll');
                // Enable the control once it has data (it also needs the refresh to become enabled)
                $('#popCustomerMultiselect').prop('disabled', false);
                $('#popCustomerMultiselect').selectpicker('refresh');
            });
    });

    // 
    // Adjust textareas to content height (up to 250px)
    //
    $('textarea').each(function (idx, txtarea) {
        if (txtarea.scrollHeight > txtarea.clientHeight) {
            $(txtarea).height(Math.min(txtarea.scrollHeight, 250));
        }
    });

    $('#showOnlyOne22Lines').change(function () {
        var rows = $('table.openQuotes tbody tr');
        if ($(this).is(":checked")) {
            rows.filter('.notOne22Line').hide();
        } else {
            rows.filter('.notOne22Line').show();
        }
    });

    $('#showOnlyOne22LinesOS').change(function () {
        var rows = $('table.orderStatusing tbody tr');
        if ($(this).is(":checked")) {
            rows.filter('.notOne22LineOS').hide();
        } else {
            rows.filter('.notOne22LineOS').show();
        }
    });

    $('#showQuotesWithoutCRD').change(function () {
        var rows = $('table.openQuotes tbody tr');
        if ($(this).is(":checked")) {
            rows.filter('.notQuoteWithoutCRD').hide();
        } else {
            rows.filter('.notQuoteWithoutCRD').show();
        }
    });

    $('#customerViewSwitch').change(function () {
        $('#orderStatusingMainView').toggle();
        $('#customerView').toggle();
        $('#customerViewReport').toggle();
    });

    $('input[name="estArrDate"]').datepicker();
});

function GeneratePDF() {

    $('#customerViewReport').html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');

    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "GET",
        url: '/POPDashboard/GetCustomerView',
        data: { salesperson: $('#salesperson').val(), selectedCustomers: $('#selectedCustomers').val(), customerno: $('#customerNo').val(), projectid: $('#projectId').val(), region: $('#region').val() }
    }).done(function (response) {
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: "POST",
            url: '/POPDashboard/CustomerViewPDF',
            data: JSON.stringify({ data: response, customers: $('#popCustomerMultiselect option:selected').toArray().map(item => item.innerText) })
        }).done(function (result) {
            if (result) {
                window.open('../files/PDBCustomerViewDetails' + result + '.pdf', '_blank');
                $('#customerViewReport').html('Customer View');
            } else {
                location.replace("/home/error?msg=sendingpdf");
            }
        }
        ).fail(function (error) {
            location.replace("/home/error?msg=processingpdf");
        });
    });
}