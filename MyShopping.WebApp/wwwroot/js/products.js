var datatable;
$(document).ready(function () {
    loaddata();
});
function loaddata() {
    datatable = $("#myTable").DataTable({
        "ajax": { url: '/Admin/Product/GetData' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'description', "width": "15%" },
            { data: 'price', "width": "10%" },
            { data: 'category.name', "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <a href="/Admin/Product/Edit/${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                            <a onClick=DeleteItem("/Admin/Product/DeleteProduct/${data}") class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
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
