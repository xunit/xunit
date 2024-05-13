$(function () {
    var $stable = $(".version-stable"),
        $pre = $(".version-pre"),
        $ci = $(".version-ci");

    $("input[name=version]")
        .change(function (e) {
            var target = $(e.target).data("selector");

            if (target === ".version-stable")
                $stable.removeClass("hidden");
            else
                $stable.addClass("hidden");

            if (target === ".version-pre")
                $pre.removeClass("hidden");
            else
                $pre.addClass("hidden");

            if (target === ".version-ci")
                $ci.removeClass("hidden");
            else
                $ci.addClass("hidden");
        });
});
