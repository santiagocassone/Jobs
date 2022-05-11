$(document).ready(function () {

    $('#lqdbDateFrom').datepicker({
        todayHighlight: true,
        autoclose: true
    });

    $('#lqdbDateTo').datepicker({
        todayHighlight: true,
        autoclose: true
    });

    $('#laborQuoteTable').DataTable({
        "scrollX": true,
        "paging": true
    });

    document.querySelector('#lstCustomer').addEventListener('input', function (e) {
        var input = e.target,
            list = input.getAttribute('list'),
            options = document.querySelectorAll('#' + list + ' option'),
            hiddenInput = document.getElementById(input.getAttribute('id') + '-hidden'),
            inputValue = input.value;

        hiddenInput.value = inputValue;

        for (var i = 0; i < options.length; i++) {
            var option = options[i];

            if (option.value === inputValue) {
                hiddenInput.value = option.getAttribute('data-value');
                break;
            }
        }
    });

});