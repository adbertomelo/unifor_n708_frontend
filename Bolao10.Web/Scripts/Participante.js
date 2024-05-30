
function getPosicoes(texto) {
    var posicoes = [];
    var fPodeSair = false;
    var pos1 = 0;
    var pos2 = 0;

    while (fPodeSair == false) {

        pos1 = texto.indexOf("(", pos2 + 1);

        pos2 = texto.indexOf(")", pos1)

        var posicao = texto.substr(pos1 + 1, pos2 - pos1 - 1);

        posicoes.push(posicao);

        fPodeSair = (texto.indexOf("(", pos2 + 1) < 0);

    }

    return (posicoes);

}

function setEstrelas(elemId, texto) {
    var posicoes = getPosicoes(texto);

    for (var idx = 0; idx <= posicoes.length ; idx++) {
        var cor = "";

        if (posicoes[idx] == 1) {
            cor = "#FFD700";
        }
        else if (posicoes[idx] == 2) {
            cor = "#c0c0c0";
        }
        else if (posicoes[idx] == 3) {
            cor = "#cd7f32";
        }

        if (cor != "") {
            var id = "#spanEstrelas" + elemId.substr(8);
            $(id).append("<small><span class='glyphicon glyphicon-star' style='color:" + cor + ";  top: -4px;'></span></small>");
        }


    }

}


$(function () {


    $("img").click(function () {

        var newSrc = "";

        if ($(this).attr("src").indexOf("up.png") > -1) {
            newSrc = $(this).attr("src").replace("up.png", "down.png")
            $(this).attr("src", newSrc);
            $(this).parent().find("div").addClass("collapse");
        } else {
            newSrc = $(this).attr("src").replace("down.png", "up.png")
            $(this).attr("src", newSrc);
            $(this).parent().find("div").removeClass("collapse");
        }


    });

    $(".myTooltip").tooltip();

    $("a").each(function (array, elem) {

        var texto = $(elem).attr("data-original-title");
        var id = $(elem).attr("id");

        if (texto != undefined) {
            setEstrelas(id, texto);
        }
    });

});


