

function getProximoPlacar(codIndicadorAtual)
{

    var statboxId = "#statbox" + codIndicadorAtual;

    var valIndicadorId = "#val" + codIndicadorAtual;

    var jogoAtual = $(valIndicadorId + " span:nth-child(2)").text();

    $(statboxId).slideUp(200).delay(100);
    
    $.ajax({
        url: "/Home/GetProximoPlacar",
        data: "jogoAtual=" + jogoAtual,
        dataType: 'json',
        cache:false,
        success: function (data) {
            var nome = data.Nome;
            var cod = data.Cod;
            var indicVal = "";
            var indicCompl = "";

            for (var i = 0; i < data.Valores.length; i++) {

                indicVal = data.Valores[i].Val;
                indicCompl = "";
                var posCompl = indicVal.indexOf(":");

                if (posCompl > 0) {
                    indicVal = data.Valores[i].Val.substring(0, posCompl);
                    indicCompl = data.Valores[i].Val.substring(posCompl + 1);
                }

            }

            $(statboxId).fadeIn(0);

            $(statboxId).removeClass();

            var statboxClassNewName = "statbox " + cod;

            $(statboxId).addClass(statboxClassNewName);

            $(statboxId).attr("id", "statbox" + cod);

            var valIndicadorId = "#val" + codIndicadorAtual;

            $(valIndicadorId + " span:nth-child(1)").text(indicVal);

            $(valIndicadorId + " span:nth-child(2)").text(indicCompl);

            $(valIndicadorId).attr("id", "val" + cod);

            var nomeIndicadorId = "#nome" + codIndicadorAtual;

            $(nomeIndicadorId).text(nome);

            $(nomeIndicadorId).attr("id", "nome" + cod);

            var linkIndicadorId = "#link" + codIndicadorAtual;

            var newFunc = "javacript:getProximoPlacar('" + cod + "')";

            $(linkIndicadorId).attr("onclick", newFunc);

            $(linkIndicadorId).attr("id", "link" + cod);

        },
        error: function (jqXHR, textoStatus, errorThrown) {
            $('.ajax-error').css('display', '');
            $('#loading-wall-messages').css('display', 'none');
        }
    });
}

function getProximoIndicador(codIndicadorAtual)
{
    var statboxId = "#statbox" + codIndicadorAtual;

    $(statboxId).slideUp(200).delay(100);

    $.ajax({
        url: "/Home/GetProximoIndicador",
        data: "codIndicadorAtual=" + codIndicadorAtual,
        dataType: 'json',
        cache:false,
        success: function (data) {
            var nome = data.Nome;
            var cod = data.Cod;
            var indicVal = "";
            var indicCompl = "";

            for(var i = 0; i < data.Valores.length; i++)
            {

                indicVal = data.Valores[i].Val;
                indicCompl = "";
                var posCompl = indicVal.indexOf(":");

                if (posCompl > 0)
                {
                    indicVal = data.Valores[i].Val.substring(0, posCompl);
                    indicCompl = data.Valores[i].Val.substring(posCompl + 1);
                }

            }

            $(statboxId).fadeIn(0);

            $(statboxId).removeClass();

            var statboxClassNewName = "statbox " + cod;            

            $(statboxId).addClass(statboxClassNewName);

            $(statboxId).attr("id", "statbox" + cod);

            var valIndicadorId = "#val" + codIndicadorAtual;

            $(valIndicadorId + " span:nth-child(1)").text(indicVal);

            $(valIndicadorId + " span:nth-child(2)").text(indicCompl);

            $(valIndicadorId).attr("id", "val" + cod);

            var nomeIndicadorId = "#nome" + codIndicadorAtual;

            var infoCompl = "<a id='info' class='myTooltip' href='#' style='color:white;margin-left:5px;' data-toggle='tooltip' title='' data-original-title='Indica a melhor posição possível no dia, levando em conta os placares em tempo real'><span class='glyphicon glyphicon-info-sign'></span></a>";

            $(nomeIndicadorId).html(nome + (cod == "MelhorPosicaoDia" ? infoCompl : ""));

            $(nomeIndicadorId).attr("id", "nome" + cod);

            var linkIndicadorId = "#link" + codIndicadorAtual;

            var newFunc = "javacript:getProximoIndicador('" + cod + "')";

            $(linkIndicadorId).attr("onclick", newFunc);

            $(linkIndicadorId).attr("id", "link" + cod);

            $(".myTooltip").tooltip();

        },
        error: function (jqXHR, textoStatus, errorThrown) {
            $('.ajax-error').css('display', '');
            $('#loading-wall-messages').css('display', 'none');
        }
    });

}


