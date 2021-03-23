$(function () {
    var $csharp = $(".language-csharp"),
        $fsharp = $(".language-fsharp"),
        $vb = $(".language-vb");

    $("input[name=language]")
        .change(function (e) {
            var target = $(e.target).data("selector");

            if (target === ".language-csharp")
                $csharp.removeClass("hidden");
            else
                $csharp.addClass("hidden");

            if (target === ".language-fsharp")
                $fsharp.removeClass("hidden");
            else
                $fsharp.addClass("hidden");

            if (target === ".language-vb")
                $vb.removeClass("hidden");
            else
                $vb.addClass("hidden");
        });
});