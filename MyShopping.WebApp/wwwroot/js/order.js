var datatable;
$(document).ready(function () {
    loaddata();
});
function loaddata() {
    datatable = $("#myTable").DataTable({
        "ajax": { url: '/Admin/Order/GetData' },
        "columns": [
            { data: 'id', "width": "25%" },
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'applicationUser.email', "width": "10%" },
            { data: 'orderStatus', "width": "15%" },
            { data: 'totalPriced', "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <a href="/Admin/Order/Details?orderid=${data}" class="btn btn-warning mx-2"> <i class="bi bi-pencil-square"></i> Details</a>
                            `;
                },
                "width": "25%"
            }
        ]
    });
}

function DeleteItem(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        datatable.ajax.reload();
                        toaster.success(data.message);
                    } else {
                        toaster.error(data.message);
                    }
                }
            })
            Swal.fire({
                title: "Deleted!",
                text: "Your file has been deleted.",
                icon: "success"
            });
        }
    });
}
