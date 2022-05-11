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

    //prevent multiple clicks on submit button
    $("#frmLaborQuote").submit(function () {
        $("#SendLaborQuote").hide();
        $("#SendLaborQuoteFake").show();
        return true;
    });

    // Receive/Deliver checkboxs exclude mutual
    SetDisplayLaborQuoteDeliver(true);
    $('#recDelMonFri').click(function () {
        if ($(this).prop("checked")) {
            $('#recDelWeekend').prop("checked", !$(this).prop("checked"));
        }
        SetDisplayLaborQuoteDeliver(false);
    })
    $('#recDelWeekend').click(function () {
        if ($(this).prop("checked")) {
            $('#recDelMonFri').prop("checked", !$(this).prop("checked"));
        }     
        SetDisplayLaborQuoteDeliver(false);
    })

    // Deliver Time checkbox exclude mutual
    $('#delTimeReg').click(function () {
        if ($(this).prop("checked")) {
            $('#delTimeOT').prop("checked", !$(this).prop("checked"));
            $('#delTimeDT').prop("checked", !$(this).prop("checked"));
        }
    })
    $('#delTimeOT').click(function () {
        if ($(this).prop("checked")) {
            $('#delTimeReg').prop("checked", !$(this).prop("checked"));
            $('#delTimeDT').prop("checked", !$(this).prop("checked"));
        }
    })
    $('#delTimeDT').click(function () {
        if ($(this).prop("checked")) {
            $('#delTimeOT').prop("checked", !$(this).prop("checked"));
            $('#delTimeReg').prop("checked", !$(this).prop("checked"));
        }
    })

    // Installation checkboxs exclude mutual
    SetDisplayLaborQuoteInstallation(true);
    $('#instMonFri').click(function () {
        if ($(this).prop("checked")) {
            $('#instWeekend').prop("checked", !$(this).prop("checked"));
        }
        SetDisplayLaborQuoteInstallation(false);
    })
    $('#instWeekend').click(function () {
        if ($(this).prop("checked")) {
            $('#instMonFri').prop("checked", !$(this).prop("checked"));
        }
        SetDisplayLaborQuoteInstallation(false);
    })

    // Installation Time checkbox exclude mutual
    $('#instReg').click(function () {
        if ($(this).prop("checked")) {
            $('#instOT').prop("checked", !$(this).prop("checked"));
            $('#instDT').prop("checked", !$(this).prop("checked"));
        }
    })
    $('#instOT').click(function () {
        if ($(this).prop("checked")) {
            $('#instReg').prop("checked", !$(this).prop("checked"));
            $('#instDT').prop("checked", !$(this).prop("checked"));
        }
    })
    $('#instDT').click(function () {
        if ($(this).prop("checked")) {
            $('#instOT').prop("checked", !$(this).prop("checked"));
            $('#instReg').prop("checked", !$(this).prop("checked"));
        }
    })

    SetQuoteOrOrderNumberLabel($('input[name="LaborQuoteOrOrder"]:checked').val());
    $('input[name="LaborQuoteOrOrder"').change(function () {
        SetQuoteOrOrderNumberLabel($(this).val());
    });

    SetDisplayLaborQuoteUnionVendorsSelect($('input[name="LaborQuoteProjectIs"]:checked').val());
    $('input[name="LaborQuoteProjectIs"').change(function () {
        SetDisplayLaborQuoteUnionVendorsSelect($(this).val());
    });

    SetDisplayLaborQuoteUnionVendorsADD($('input[value="ADD"]'));
    $('input[name="LaborQuoteUnionVendors"').change(function () {
        SetDisplayLaborQuoteUnionVendorsADD($(this));
    });

    SetDisplayMasoniteQuestion($('#chkFloor'));
    $('#chkFloor').click(function () {
        SetDisplayMasoniteQuestion(this);
    });

    SetDisplayNoOfFlights($('input[name="LaborQuoteCarryUp:checked"').val());
    $('input[name="LaborQuoteCarryUp"').change(function () {
        SetDisplayNoOfFlights($(this).val());
    });

    SetDisplayProjectManagement();
    $('input[name="LaborQuoteTypes"').change(function () {
        SetDisplayProjectManagement();
    });

    SetDisplayOutOfState($('input[name="LaborQuoteYulioPresentation"]:checked').val());
    $('input[name="LaborQuoteYulioPresentation"').change(function () {
        SetDisplayOutOfState($('input[name="LaborQuoteYulioPresentation"]:checked').val());
    });

    $('#CreateRevision').click(function (ev) {
        ev.preventDefault();
        $('#divCreateRevisionButton').css('display', 'none');
        $('#divSelectOriginalLaborQuoteNo').css('display', 'block');

        $.get("/onequoteform/getoriginallaborquotes")
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
            url: '/onequoteform/getProjectData',
            data: { projectId: $('input[name="LaborQuoteHedbergProjectID"').val() }
        }).done(function (data) {            
            $('select[name="LaborQuoteCustomer"] option[value="' + data.customer_no + '"]').prop('selected', true);
            $('input[name="LaborQuoteStreetAddress"').val(data.address_Line_1);
            $('input[name="LaborQuoteBRF"').val('---');
            $('input[name="LaborQuoteCity"').val(data.city);       
            $('select[name="LaborQuoteSalesRep"] option[value="' + data.salesperson_id + '"]').prop('selected', true);
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
    if (value == "U") {
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

function SetDisplayLaborQuoteUnionVendorsADD(input) {
    if (input.val() === 'ADD') {
        if (input.prop('checked')) {
            $('#LaborQuoteUnionVendorsADD').css('display', 'block');
        } else {
            $('#LaborQuoteUnionVendorsADD').css('display', 'none');
        }
    }
}

function SetDisplayLaborQuoteDeliver(firstLoad) {

    if ($('#recDelMonFri').prop('checked')) {

        $('#DeliverWeekLabel').html('Monday/Friday Time');
        $('.optionalDeliverTime').css('display', 'block');
        $('#recDelTimes').css('display', 'block');

    } else if ($('#recDelWeekend').prop('checked')) {

        $('#DeliverWeekLabel').html('Saturday/Sunday Time');
        $('.optionalDeliverTime').css('display', 'none');
        $('#recDelTimes').css('display', 'block');

    } else {
        
        $('#recDelTimes').css('display', 'none');

    }

    if (!firstLoad) {
        $('#delTimeReg').prop("checked", false);
        $('#delTimeOT').prop("checked", false);
        $('#delTimeDT').prop("checked", false);
    }    

}

function SetDisplayLaborQuoteInstallation(firstLoad) {

    if ($('#instMonFri').prop('checked')) {

        $('#InstallationWeekLabel').html('Monday/Friday Time');
        $('.optionalInstallationTime').css('display', 'block');
        $('#instTimes').css('display', 'block');

    } else if ($('#instWeekend').prop('checked')) {

        $('#InstallationWeekLabel').html('Saturday/Sunday Time');
        $('.optionalInstallationTime').css('display', 'none');
        $('#instTimes').css('display', 'block');

    } else {

        $('#instTimes').css('display', 'none');

    }

    if (!firstLoad) {
        $('#instReg').prop("checked", false);
        $('#instOT').prop("checked", false);
        $('#instDT').prop("checked", false);
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

function SetDisplayProjectManagement() {
    $('.projectManagement').css('display', 'none');
    $('input[name="LaborQuoteTypes"]:checked').each(function () {
        if ($(this).val() == "77") {
            $('.projectManagement').css('display', 'block');
        }
    });
}

function SetDisplayOutOfState(value) {
    if (value == 'true') {
        $('.outOfStateProject').css('display', 'block');
    } else {
        $('.outOfStateProject').css('display', 'none');
    }
}

function addInstFile() {
    let id = $('.fileFinalInstDiv').last().attr('id').split('-');
    $("<div class='input-group fileFinalInstDiv' id='fileFinalInstDiv-" + (parseInt(id[1]) + 1) + "'><input class='form-control form-rounded mt-2 checkSize' style='height: 44px;' type='file' name='fileFinalInstPlan' /><button type='button' class='btn btn-danger btn-sm fas fa-trash' style='margin-top: 8px;' onclick='removeInstFile(" + (parseInt(id[1]) + 1) + ")'></button></div>").insertAfter('#fileFinalInstDiv-' + id[1]);
}

function removeInstFile(id) {
    $('#fileFinalInstDiv-' + id).remove();
}

function addPrelFile() {
    let id = $('.filePrelDiv').last().attr('id').split('-');
    $("<div class='input-group filePrelDiv' id='filePrelDiv-" + (parseInt(id[1]) + 1) + "'><input class='form-control form-rounded mt-2' style='height: 44px;' type='file' name='filePrelPlan' /><button type='button' class='btn btn-danger btn-sm fas fa-trash' style='margin-top: 8px;' onclick='removePrelFile(" + (parseInt(id[1]) + 1) + ")'></button></div>").insertAfter('#filePrelDiv-' + id[1]);
}

function removePrelFile(id) {
    $('#filePrelDiv-' + id).remove();
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