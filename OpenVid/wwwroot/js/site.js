// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).on('click', '#loadMore', function () {
    loadMore($('#loadMore').attr('data-page'), $('#loadMore').attr('data-search-query'));
});

function loadMore(page, searchQuery) {
    $.ajax({
        type: 'GET',
        url: '/Playback/VideoList?searchString=' + searchQuery + '&pageNo=' + page,
        success: function (data) {
            $('#loadMore').replaceWith(data);
        },
        error: function (error) {
            $('#loadMore').replaceWith("<p>There was an error.</p>");
        }
    });
}

$(document).on('submit', '#searchBar', function (e) {
    e.preventDefault();
    searchString = $('#SearchString').val();
    if (searchString != "")
        window.location.href = $(this).attr('data-action') + searchString;
});