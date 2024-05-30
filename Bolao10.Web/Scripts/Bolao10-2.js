var bolaoHubClient;

$(function () {
    setupHubClientBase();
});

function setupHubClientBase() {

    $.connection.hub.logging = true;

    bolaoHubClient = $.connection.bolaoHub;

    bolaoHubClient.client.showUsersOnLine = function (numUsuarios, listUsers) {

        var numUsers = listUsers.filter(function(user){ if (user.Codigo != "") { return user } }).length;

        $("#span-users").text(numUsers);

        if (window.location.pathname.indexOf("Participante/List") > 0) {

            $("#tableParticipantes > tbody tr").each(function () {

                var span = $(this).find(':nth-child(1) span span')[0];

                //var span = $(this).find(':nth-child(2) span')[0];

                var codigo = $(this).attr("id");

                for (var i = 0; i < listUsers.length; i++) {
                    
                    if (listUsers[i].Codigo == codigo) {
                        span.setAttribute("class","text-online");
                        break;
                    } else {
                        span.setAttribute("class", "text-offline");
                        //span.setAttribute("class", "text-online");
                    }

                }


            });
        }
    };

    bolaoHubClient.client.adicionarComentario = function (parentComentarioId, newComentarioId, comment, participante) {
        if (parentComentarioId) {
            window.muralViewModel.findComentarioAndAct(parentComentarioId, muralViewModel, function (parentComentario) {
                parentComentario.adicionarNovoComentario(
                    newComentarioId,
                    comment,
                    participante,
                    '/Date(' + (new Date()).getTime() + ')/'
                );
            });
        }
        else {
            window.muralViewModel.adicionarNovoComentario(
                newComentarioId,
                comment,
                participante,
                '/Date(' + (new Date()).getTime() + ')/'
            );
        }
    };

    bolaoHubClient.client.atualizarDescurtidas = function (comentarioId, pessoasQueCurtiram) {
        window.muralViewModel.findComentarioAndAct(comentarioId, muralViewModel, function (comentario) {
            for (var i = comentario.curtiram().length - 1; i >= 0; i--) {
                if (comentario.curtiram()[i].id == pessoasQueCurtiram.Id) {
                    comentario.curtiram.splice(i, 1);
                    break;
                }
            }
        });
    };

    bolaoHubClient.client.atualizarCurtidas = function (comentarioId, pessoasQueCurtiram) {
        window.muralViewModel.findComentarioAndAct(comentarioId, muralViewModel, function (comentario) {
            comentario.curtiram.push({
                id: pessoasQueCurtiram.Id,
                nome: pessoasQueCurtiram.Nome
            });
        });
    };

    bolaoHubClient.client.atualizarPaineisRealTime = function (codigoParticipante, posicaoTempoReal, posicaoAtual, numPontos) {

        if (window.userInfo == undefined)
            return;

        if (codigoParticipante == window.userInfo.Codigo) {
            if (posicaoTempoReal != null) {

                EmiteSom();

                $("#statboxPosicaoTempoReal").slideUp(300).delay(800).fadeIn(400);

                $("#valPosicaoTempoReal span:nth-child(1)").text(posicaoTempoReal);

            } else if (posicaoAtual != null) {

                if ($("#statboxPosicaoAtual").length > 0)
                {
                    $("#statboxPosicaoAtual").slideUp(300).delay(800).fadeIn(400);
                    $("#valPosicaoAtual" + " span:nth-child(1)").text(posicaoAtual);

                }

                if (("#statboxNumPontos").length > 0)
                {
                    $("#statboxNumPontos").slideUp(300).delay(800).fadeIn(400);
                    $("#valNumPontos span:nth-child(1)").text(numPontos);

                }

            }
        }

        

    };

    $.connection.hub.start(function () {
    }).done(function () {
        
    }).fail(function () {
        alert('Conexão SignalR falhou!');
    });

    $(window).on('unload', function () { $.connection.hub.stop(); });

}

function EmiteSom() {

    document.getElementById("som").play();

}

