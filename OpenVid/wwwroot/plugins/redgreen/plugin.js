var Plugin = {
    Times: [],
    Selected: [],

    Init: function () {
        $('video').on('play', function () {
            Plugin.Interval = setInterval(Plugin.OnVideoUpdate, 500);
        });
        $('video').on('pause', function () {
            clearInterval(Plugin.Interval);
        });

        Plugin.RenderPanel();
        $(document).on('change', '#plugin input', Plugin.SelectChallenge)
    },

    RenderPanel: function () {
        var challenge = JSON.parse($("textarea#Meta").val())["plugin"];
        Times = challenge;

        $('.container div').last('div').after('<div id="plugin";"></div>');

        $.each(challenge, function (key, value) {
            $('#plugin').append('<p><label><input type=radio name=plugin value="' + key + '">' + key + '</label></p>');
        });
    },

    OnVideoUpdate: function (e) {
        var time = $('video')[0].currentTime;

        var hours = Math.floor(time / 3600);
        var minutes = Math.floor((time - (hours * 3600)) / 60);
        var seconds = Math.floor(time - (hours * 3600) - (minutes * 60))
        var timestamp = hours + ':' + minutes + ':' + seconds;

        if (Plugin.Selected[timestamp] !== undefined)
            $('body').removeClass();
            $('body').addClass(Plugin.Selected[timestamp]);
    },

    SelectChallenge: function () {
        var selected = $('input[name=plugin]:checked').val();
        Plugin.Selected = Times[selected];
    }
}