$(document).ready(function () {
    $('#selLocations').selectpicker();

    $('.link-quoteInfo').click(function (evt) {
        var index = $(this).data('index');
        $('#detailsModal-' + index).modal('show');
    });

    $('#region').change(function () {
        $.ajax({
            type: "POST",
            url: '/stockinventory/loadlocations',
            data: { region: $('#region').val() }
        }).done(function (result) {
            $("#selLocations option").each(function () {
                $(this).remove();
            });
            if (result != null) {
                result.forEach(function (value, index, elem) {
                    $("#selLocations").append(new Option(value.label, value.id));
                });
                $("#selLocations").selectpicker('refresh');
            }
        }).fail(function (error) {
            location.replace("/home/error?msg=processing");
        });
    });
});