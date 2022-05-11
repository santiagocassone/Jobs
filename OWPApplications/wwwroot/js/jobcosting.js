$(document).ready(function () {

    $('.autoN').each(function (index, elem) {
        new AutoNumeric(elem);
    });

    $('.autoNCur').each(function (index, elem) {
        new AutoNumeric(elem, AutoNumeric.getPredefinedOptions().dollar);
    });

    $('.autoNPct').each(function (index, elem) {
        new AutoNumeric(elem, AutoNumeric.getPredefinedOptions().percentageUS2dec);
    })

    $('#jobCostingTable').DataTable({
        "scrollY": '80vh',
        "scrollX": true,
        "paging": true
    });    

    $('#fromDate').datepicker({
        defaultDate: new Date(),
        todayHighlight: true,
        autoclose: true
    });

    $('#toDate').datepicker({
        defaultDate: new Date(),
        todayHighlight: true,
        autoclose: true
    });

    $('#jobcostingform').on('submit', function () {
        ShowLoading();
    });    

    //Download Excel

    $('#export-excel').click(function () {
        $('#export-excel').html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');
        var jobCostFilters = {
            fromDate: $('#fromDate').val(),
            toDate: $('#toDate').val(),
            projectid: $('#projectid').val(),
            orderno: $('#orderno').val(),
            customerid: $('#customerid').val(),
            warehoseid: $('#warehoseid').val(),
            leadid: $('#leadid').val()
        }
        $.ajax({
            type: "POST",
            url: '/jobcosting/exportexcel',
            data: jobCostFilters
        }).done(function (result) {
            if (result === "success") {
                window.open('../files/JobCostingReport.xls', '_blank');
            } else if (result === "empty") {
                alert("There is no data for the current search. Please change the filters and try again. ");
            } else {
                alert(result);
            }
            $('#export-excel').html('Download Excel');
        }).fail(function (error) {
            alert(error);
            $('#export-excel').html('Download Excel');
        });
        
    });

    $('.jcbudtotal').change(function () {
        let proj = $(this).attr('id').split('-')[1];
        const quoteBudget = AutoNumeric.getAutoNumericElement($('#QuoteBudget-' + proj)[0]);
        const additionalCost = AutoNumeric.getAutoNumericElement($('#AdditionalCost-' + proj)[0]);
        const changeOrder = AutoNumeric.getAutoNumericElement($('#ChangeOrder-' + proj)[0]);
        const inputTotal = AutoNumeric.getAutoNumericElement($('#jcbudtotal-' + proj)[0]);
        inputTotal.set(parseFloat(quoteBudget.get()) + parseFloat(additionalCost.get()) + parseFloat(changeOrder.get()));
        const jccosttotal = AutoNumeric.getAutoNumericElement($('#jccosttotal-' + proj)[0]);
        updateGP(parseFloat(inputTotal.get()), parseFloat(jccosttotal.get()), proj);
        adjustTable();
    })

    $('.jccosttotal').change(function () {
        let proj = $(this).attr('id').split('-')[1];
        const jclaborcost = AutoNumeric.getAutoNumericElement($('#jclaborcost-' + proj)[0]);
        const jcvehiclecost = AutoNumeric.getAutoNumericElement($('#jcvehiclecost-' + proj)[0]);
        const additionalExpenses = AutoNumeric.getAutoNumericElement($('#AdditionalExpenses-' + proj)[0]);
        const inputTotal = AutoNumeric.getAutoNumericElement($('#jccosttotal-' + proj)[0]);
        inputTotal.set(parseFloat(jclaborcost.get()) + parseFloat(jcvehiclecost.get()) + parseFloat(additionalExpenses.get()));
        const jcbudtotal = AutoNumeric.getAutoNumericElement($('#jcbudtotal-' + proj)[0]);
        updateGP(parseFloat(jcbudtotal.get()), parseFloat(inputTotal.get()), proj);
        adjustTable();
    })

    $('.hourRate').change(function () {
        let proj = $(this).attr('id').split('-')[1];
        const regRate = AutoNumeric.getAutoNumericElement($('#RegHrsRate-' + proj)[0]);
        const otRate = AutoNumeric.getAutoNumericElement($('#OTHrsRate-' + proj)[0]);
        const dtRate = AutoNumeric.getAutoNumericElement($('#DTHrsRate-' + proj)[0]);
        const pwRegRate = AutoNumeric.getAutoNumericElement($('#PWRegHrsRate-' + proj)[0]);
        const pwOtRate = AutoNumeric.getAutoNumericElement($('#PWOTHrsRate-' + proj)[0]);
        const pwDtRate = AutoNumeric.getAutoNumericElement($('#PWDTHrsRate-' + proj)[0]);
        const regHs = AutoNumeric.getAutoNumericElement($('#RegHs-' + proj)[0]);
        const otHs = AutoNumeric.getAutoNumericElement($('#OTHs-' + proj)[0]);
        const dtHs = AutoNumeric.getAutoNumericElement($('#DTHs-' + proj)[0]);
        const pwRegHs = AutoNumeric.getAutoNumericElement($('#PWRegHs-' + proj)[0]);
        const pwOtHs = AutoNumeric.getAutoNumericElement($('#PWOTHs-' + proj)[0]);
        const pwDtHs = AutoNumeric.getAutoNumericElement($('#PWDTHs-' + proj)[0]);
        let laborCost = parseFloat(regRate.get() * regHs.get()) + parseFloat(otRate.get() * otHs.get()) + parseFloat(dtRate.get() * dtHs.get()) + parseFloat(pwRegRate.get() * pwRegHs.get()) + parseFloat(pwOtRate.get() * pwOtHs.get()) + parseFloat(pwDtRate.get() * pwDtHs.get());
        const jclaborcost = AutoNumeric.getAutoNumericElement($('#jclaborcost-' + proj)[0]);
        jclaborcost.set(laborCost);
        $('.jccosttotal').trigger('change');
    });

    setTimeout(function () { $('#loadingTable').hide(); $('#loadedTable').show(); adjustTable(); }, 3000);    
    
});

function updateGP(budget, cost, proj) {
    const jcgpdollar = AutoNumeric.getAutoNumericElement($('#jcgpdollar-' + proj)[0]);
    jcgpdollar.set(budget - cost);
    const jcgppct = AutoNumeric.getAutoNumericElement($('#jcgppct-' + proj)[0])
    if (budget != 0) {
        jcgppct.set(cost * 100 / budget);
    } else {
        jcgppct.set(0);
    }
    
}

function adjustTable() {
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
};