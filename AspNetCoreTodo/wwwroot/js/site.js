$(document).ready(function () {
    //	Wire	up	all	of	the	checkboxes	to	run	markCompleted()				
    $('.done-checkbox').on('click', function (e) {
        markCompleted(e.target);
    });
});
function markCompleted(checkbox) {
    checkbox.disabled = true;

    var row = checkbox.closest('tr');
    $(row).addClass('done');

    var form = checkbox.closest('form');
    form.submit();
}

$(document).on('change', ':radio[name="Adrs"]', function () {
    var arOfVals = $(this).parent().nextAll().map(function () {
        return $(this).text();
    }).get();
    document.getElementById("addressEntered").value = arOfVals;

    document.getElementById("LatLongEntered").value =
    document.getElementById("latLng").value;
});

function AddressPartial() {
    var addressString =
        document.getElementById("addressStr").value + " " +
        document.getElementById("addressN").value + " " +
        document.getElementById("addressArea").value;

    $.ajax(
        {
            type: "POST",
            url: "../Todo/AddressPartial",
            data: { address: addressString },
            success: function (data) { $('.modal-body').html(data); },
            error: function (data) { $('.modal-body').html("Please complete the address field!"); }
        });
}

document.getElementById('addTodoBtn').addEventListener("click", function () {
    if(document.getElementById("addressEntered").value != "")
    { 
    document.getElementById('formAddTodo').submit();
    }else
    {
        this.innerHTML ="Seleccione una dirección!";
    }
});

$(document).ready(function () {
    $('.sorter-btn').on('click', function (e) {
        this.id = "Asc";
    });
});
