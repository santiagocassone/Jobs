$(document).ready(function () {

    $('#selStatus').change(function () {
        var selectedValue = $(this).find(':selected').val();
        if (selectedValue == 'complete') {
            $('.Complete').show();
            $('.Canceled').hide();
        } else if (selectedValue == 'canceled') {
            $('.Complete').hide();
            $('.Canceled').show();
        } else {
            $('.Complete').show();
            $('.Canceled').show();
        }
    });

    $('select[name="selEstimatedBy"]').change(function () {
        var selectedValue = $(this).find(':selected').val();
        var lqCode = $(this).find(':selected').data('lqcode');
        let region = $('input[name=region]').val();

        $.ajax({
            type: "POST",
            url: '/estimatordashboard/setestimator',
            data: { code: lqCode, estimatorName: selectedValue, region: region }
        });
    });

});