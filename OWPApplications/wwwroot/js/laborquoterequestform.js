$(document).ready(function () {

    // check files size
    $('#frmLaborQuote').submit(function () {
        let size = 0;
        $('.checkSize').each(function (i, e) {
            if (e.files[0] != null) {
                size += e.files[0].size;
            }
        });
        if (size > 36700160) {
            alert('All files cannot exceed 35MB');
            return false;
        }
        return true; // return false to cancel form action
    });

    // draft reupload files
    $('#reUploadFiles').click(function () {
        if (confirm('Files will be removed. Are you sure to continue?')) {
            $('#uploadedFiles').hide();
            $('#reuploadFiles').show();
            $('#removeFiles').val('yes');
        }
    });
    $('input[name="LaborQuoteTargetDate"').datepicker({});

    SetQuoteOrOrderNumberLabel($('input[name="LaborQuoteOrOrder"]:checked').val());
    $('input[name="LaborQuoteOrOrder"]').change(function () {
        SetQuoteOrOrderNumberLabel($(this).val());
    });

    SetDisplayLaborQuoteUnionVendorsSelect($('input[name="LaborQuoteProjectIs"]:checked').val());
    $('input[name="LaborQuoteProjectIs"]').change(function () {
        SetDisplayLaborQuoteUnionVendorsSelect($(this).val());
    });

    SetDisplayLaborQuoteDeliver($('input[name="LaborQuoteDeliver"]:checked').val());
    $('input[name="LaborQuoteDeliver"]').change(function () {
        SetDisplayLaborQuoteDeliver($(this).val());
    });

    SetDisplayLaborQuoteInstallation($('input[name="LaborQuoteInstallation"]:checked').val());
    $('input[name="LaborQuoteInstallation"]').change(function () {
        SetDisplayLaborQuoteInstallation($(this).val());
    });

    SetDisplayMasoniteQuestion($('#chkFloor'));
    $('#chkFloor').click(function () {
        SetDisplayMasoniteQuestion(this);
    });

    SetDisplayNoOfFlights($('input[name="LaborQuoteCarryUp:checked"]').val());
    $('input[name="LaborQuoteCarryUp"]').change(function () {
        SetDisplayNoOfFlights($(this).val());
    });

    SetDisplayElectricalRequiredDetails($('input[name="LaborQuoteElectricalRequired"]:checked').val());
    $('input[name="LaborQuoteElectricalRequired"]').change(function () {
        SetDisplayElectricalRequiredDetails($(this).val());
    });

    $('#CreateRevision').click(function (ev) {
        ev.preventDefault();
        $('#divCreateRevisionButton').css('display', 'none');
        $('#divSelectOriginalLaborQuoteNo').css('display', 'block');

        $.get("/laborquoterequestform/getoriginallaborquotes")
            .done(function (data) {
                if (data != null) {
                    data.map(function (option) {
                        $('#selectOriginalLaborQuotes').append($('<option>', {
                            value: option.id,
                            text: option.label
                        }));
                    });
                } else {
                    $('#selectOriginalLaborQuotes').append($('<option>', {
                        value: "",
                        text: "No data found"
                    }));
                }
            });
    });

    // Set Customer, Salesperson and Address when Project ID changes
    $('input[name="LaborQuoteHedbergProjectID"').blur(function () {
        $.ajax({
            type: "GET",
            url: '/laborquoterequestform/getProjectData',
            data: { projectId: $('input[name="LaborQuoteHedbergProjectID"').val() }
        }).done(function (data) {
            if (data != null) {
                $('select[name="LaborQuoteCustomer"] option[value="' + data.customer_no + '"]').prop('selected', true);
                $('input[name="LaborQuoteStreetAddress"').val(data.address_Line_1);
                $('input[name="LaborQuoteBRF"').val('---');
                $('input[name="LaborQuoteCity"').val(data.city);
                $('select[name="LaborQuoteSalesRep"] option[value="' + data.salesperson_id + '"]').prop('selected', true);
                console.log(data);
            } else {
                console.log('No project data');
            }
        }).fail(function (error) {
            console.log(error);
        });
    });

    $('#CancelRevision').click(function () {
        $('#divCreateRevisionButton').css('display', 'block');
        $('#divSelectOriginalLaborQuoteNo').css('display', 'none');
        $('.masoniteQuestion').css('display', 'none');
        $('.noOfFlights').css('display', 'none');
        SetQuoteOrOrderNumberLabel("Q");
        SetDisplayLaborQuoteUnionVendorsSelect("N");
    });

    if ($('#hdnSuccess').val() != "") {
        if ($('#hdnSuccess').val() == 'success') {
            alert('The Labor Quote was created successfully!');
        } else if ($('#hdnSuccess').val() == 'saved') {
            alert('The Labor Quote was saved successfully!');
        } else if ($('#hdnSuccess').val() == 'error') {
            alert('There was an error creating the Labor Quote.');
        }
    }

    $('textarea[name="LaborQuoteScopeOfWork"]').keydown(function (event) {
        if (event.which == 13) {
            event.stopPropagation();
        }
    });

    $('textarea[name="LaborQuoteAddInfo"]').keydown(function (event) {
        if (event.which == 13) {
            event.stopPropagation();
        }
    });

    // Product required workaround
    checkRequiredProduct();
    $('.LaborQuoteProduct').change(function () {
        checkRequiredProduct();
    });

    $(".projectIs").click(function () {
        if ($(this).val() == 'E') {
            $('#outOfStateEmailDiv').show();
            $("outOfStateEmail").prop('required', true);
        } else {
            $('#outOfStateEmailDiv').hide();
            $("outOfStateEmail").prop('required', false);
        }
    });

    if ($('.projectIs:checked').val() == 'E') {
        $('#outOfStateEmailDiv').show();
        $("outOfStateEmail").prop('required', true);
    };


});

function SetQuoteOrOrderNumberLabel(value) {
    if ($('input[name="LaborQuoteOrOrder"]').is(':checked')) {
        $('#divQONo').css('display', 'block');
    }
    if (value == "O") {
        $('#QuoteOrOrderNumberLabel').html('Order Number <span style="color: red">*</span>');
    } else {
        $('#QuoteOrOrderNumberLabel').html('Quote Number <span style="color: red">*</span>');
    }
}

function SetDisplayLaborQuoteUnionVendorsSelect(value) {
    if (value == "U" && $('#hdnCurrDealer').val() != 'osq') {
        $('#LaborQuoteUnionVendorsSelect').css('display', 'block');
        $('#LaborQuoteSupplyEmail').css('display', 'block');
        if ($('#hdnIsReadOnly').val() == 'False') {
            $('input[name="LaborQuoteSupplyRequestorEmail"]').val($('input[name="LaborQuoteRequestorEmail"]').val());
        }
    } else {
        $('#LaborQuoteUnionVendorsSelect').css('display', 'none');
        $('#LaborQuoteSupplyEmail').css('display', 'none');
        if ($('#hdnIsReadOnly').val() == 'False') {
            $('input[name="LaborQuoteSupplyRequestorEmail"]').val('');
        }
    }
}

function SetDisplayLaborQuoteDeliver(value) {
    if (value == "126") {
        $('#DeliverWeekLabel').html('Saturday/Sunday Time');
        $('.optionalDeliverTime').css('display', 'none');
    } else {
        $('#DeliverWeekLabel').html('Monday/Friday Time');
        $('.optionalDeliverTime').css('display', 'block');
    }
}

function SetDisplayLaborQuoteInstallation(value) {
    if (value == "179") {
        $('#InstallationWeekLabel').html('Saturday/Sunday Time');
        $('.optionalInstallationTime').css('display', 'none');
    } else {
        $('#InstallationWeekLabel').html('Monday/Friday Time');
        $('.optionalInstallationTime').css('display', 'block');
    }
}

function SetDisplayMasoniteQuestion(value) {
    if ($(value).is(':checked')) {
        $('.masoniteQuestion').css('display', 'block');
    } else {
        $('.masoniteQuestion').css('display', 'none');
    }
}

function SetDisplayNoOfFlights(value) {
    if (value == '1') {
        $('.noOfFlights').css('display', 'block');
    } else {
        $('.noOfFlights').css('display', 'none');
    }
}

function SetDisplayElectricalRequiredDetails(value) {
    if (value == '1') {
        $('.electricalReqAddDetails').css('display', 'block');
    } else {
        $('.electricalReqAddDetails').css('display', 'none');
    }
}

function addPrelFile() {
    let id = $('.filePrelDiv').last().attr('id').split('-');
    $("<div class='input-group filePrelDiv' id='filePrelDiv-" + (parseInt(id[1]) + 1) + "'><input class='form-control form-rounded mt-2 checkSize' type='file' name='filePrelPlan' /><button type='button' class='btn btn-danger btn-sm fas fa-trash' style='margin-top: 8px;' onclick='removePrelFile(" + (parseInt(id[1]) + 1) + ")'></button></div>").insertAfter('#filePrelDiv-' + id[1]);
}

function removePrelFile(id) {
    $('#filePrelDiv-' + id).remove();
}

function checkRequiredProduct() {
    let flag = false;
    $('.LaborQuoteProduct').each(function () {
        if ($(this).prop('checked')) {
            flag = true;
        }
    });
    if (flag) {
        $('.LaborQuoteProduct').each(function () {
            $(this).removeAttr('required');
        });
    } else {
        $('.LaborQuoteProduct').each(function () {
            $(this).attr('required', 'required');
        });
    }
}

function validate(evt) {
    var theEvent = evt || window.event;

    // Handle paste
    if (theEvent.type === 'paste') {
        key = event.clipboardData.getData('text/plain');
    } else {
        // Handle key press
        var key = theEvent.keyCode || theEvent.which;
        key = String.fromCharCode(key);
    }
    var regex = /[0-9]/;
    if (!regex.test(key)) {
        theEvent.returnValue = false;
        if (theEvent.preventDefault) theEvent.preventDefault();
    }
}