// Account page JavaScript code.
$(document).ready(function() {
    $('#Prefix').change(function() {
        var gender = $(this).find("option:selected").data('gender');
        $('#Gender').val(gender);
    });

    $('#Prefix').change();
});