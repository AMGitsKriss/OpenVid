var importVideo = function () {

    function bindEvents() {
        $(document).on('click', '#uploadBtn', function (e) {
            $('#uploadFile').click();
        });

        $(document).on('change', '#uploadFile', function (e) {
            var fileUpload = $("#uploadFile").get(0);
            var files = fileUpload.files;
            var multipleFiles = files.length > 1;
            uploadVideos(files, multipleFiles, 0)
        });

        $(document).on('click', '#queueBtn', queueVideos);
    }

    function uploadVideos(files, multipleFiles, i) {
        var formData = new FormData();
        formData.append('file', files[i]);
        formData.append('multipleFiles', multipleFiles);

        $.ajax({
            type: 'POST',
            url: uploadUrl,
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {
                $('#updateForm').append(data);
                if (i < files.length - 1) {
                    uploadVideos(files, multipleFiles, i + 1);
                }
                else {
                    location.reload(true);
                }
            },
            error: function (error) {
                $('#updateForm').append("<p>AJAX Error!</p>");
            }
        });
    }

    function queueVideos() {
        $(this).prop('disabled', true);
        $(this).tooltip('hide');

        $.ajax({
            type: 'POST',
            url: queueUrl,
            success: function (data) {
                location.reload(true);
            },
            error: function (error) {
                alert(error.responseText);
            }
        });
    }

    return {
        init: function () {
            bindEvents();
        }
    };
}();