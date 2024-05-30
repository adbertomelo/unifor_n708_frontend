//var userInfo;

var Comentario = function (id, texto, participante, criadoEm, replies, curtiram) {
    var self = this;

    self.id = ko.observable(id);
    self.texto = ko.observable(texto);
    self.participante = ko.observable(participante);
    self.criadoEm = ko.observable(criadoEm);
    self.curtiram = ko.observableArray(curtiram);
    self.novoComentario = ko.observable('');
    self.itemToAdd = ko.observable("");
    self.isTypingComment = false;


    self.tempoDecorrido = ko.computed(function () {
        var ellapsed = '';

        var timeStampDiff = (new Date()).getTime() - parseInt(self.criadoEm().substring(6, 19));
        var s = parseInt(timeStampDiff / 1000);
        var m = parseInt(s / 60);
        var h = parseInt(m / 60);
        var d = parseInt(h / 24);

        ellapsed = getEllapsedTime(timeStampDiff);

        return ellapsed;
    });

    self.curtir = function () {
        bolaoHubClient.server.enviarCurtirParaServidor(self.id());
    }.bind(self);

    self.descurtir = function () {
        bolaoHubClient.server.enviarDescurtirParaServidor(self.id());
    }.bind(self);

    self.curtidoPorEsteUsuario = ko.computed(function () {
        var liked = false;
        $(self.curtiram()).each(function (array, user) {
            if (user.id == userInfo.Id) {
                liked = true;
            }
        });
        return liked;
    });

    self.sumarioDeCurtidas = ko.computed(function () {
        var summary = '';

        var sortedCurtiram = self.curtiram().sort(function (a, b) {
            var expA = (a.Id == userInfo.Id ? -1 : 1);
            var expB = (b.Id == userInfo.Id ? -1 : 1);
            return expA < expB ? -1 : 1;
        })

        $(sortedCurtiram).each(function (index, usuario) {
            if (summary.length > 0) {
                if (index == curtiram.length - 1) {
                    summary += ' e ';
                }
                else {
                    summary += ', ';
                }
            }

            if (usuario.nome == userInfo.Nome) {
                summary += 'Você';
            }
            else {
                summary += usuario.nome;
            }
        });
        if (self.curtiram().length > 1) {
            summary += ' curtiram isto';
        }
        else if (self.curtiram().length > 0) {
            summary += ' curtiu isto';
        }
        return summary;
    });

    self.comentarios = ko.observableArray([]);

    $(replies).each(function (index, reply) {

        var lk = [];

        $(reply.Curtiram).each(function (array, like) {
            lk.push({ id: like.Id, nome: like.Usuario.Nome });
        });

        self.comentarios.push(
            new Comentario(reply.Id, reply.Texto, reply.Participante, reply.CriadoEm, [], lk)
        );
    });

    self.adicionarNovoComentario = function (id, comentario, participante, criadoEm) {
        self.comentarios.push(new Comentario(id, comentario, participante, criadoEm, [], []));
    }.bind(self);

    self.commentKeypress = function (msg, event) {
        if (event.keyCode) {
            if (event.keyCode === 13) {
                if (self.novoComentario().trim().length > 0) {
                    bolaoHubClient.server.enviarComentarioParaServidor(self.id(), self.novoComentario());
                    self.novoComentario('');
                }
                return false;
            }
        }
        return true;
    }


}

var viewModel = function (model) {
    var self = this;
    self.comentarios = ko.observableArray([]);
    self.isSignalREnabled = ko.observable(model.isSignalREnabled);
    self.novoComentario = ko.observable('');
    self.isTypingComment = false;

    $(model.Comentarios).each(function (index, comentario) {

        var curtiram = [];

        $(comentario.Curtiram).each(function (array, like) {
            curtiram.push({ id: like.Id, nome: like.Usuario.Nome });
        });

        self.comentarios.push(
            new Comentario(comentario.Id, comentario.Texto, comentario.Participante, comentario.CriadoEm, comentario.Respostas, curtiram)
        );
    });

    self.findComentarioAndAct = function (comentarioId, parent, action) {
        $(parent.comentarios()).each(function (index, comentario) {
            if (comentario.id() == comentarioId) {
                action(comentario);
                return false;
            }

            $(comentario.comentarios()).each(function (index, comentario) {
                if (comentario.id() == comentarioId) {
                    action(comentario);
                    return false;
                }
            });
        });
    }

    self.adicionarNovoComentario = function (id, comentario, participante, criadoEm) {
        self.comentarios.unshift(new Comentario(id, comentario, participante, criadoEm, [], []));
    }.bind(self);

    self.commentKeypress = function (msg, event) {
        if (event.keyCode) {
            if (event.keyCode === 13) {
                if (self.novoComentario().trim().length > 0) {
                    bolaoHubClient.server.enviarComentarioParaServidor(null, self.novoComentario());
                    self.novoComentario('');
                }
                return false;
            }
        }
        return true;
    }



};

$(function () {
    window.isSignalREnabled = false;
    requisitarComentariosDoMural();
});

function requisitarComentariosDoMural() {

    $.ajax({
        url: "/Home/PegarComentariosDoMural",
        dataType: 'json',
        success: function (data) {
        
            setTimeout(function () {
                data.isSignalREnabled = true;
                window.muralViewModel = new viewModel(data);
                ko.applyBindings(window.muralViewModel);
                
            }, 50);
        },
        error: function (jqXHR, textoStatus, errorThrown) {
            $('.ajax-error').css('display', '');
            $('#loading-wall-messages').css('display', 'none');
        }
    });
}

function getEllapsedTime(timeStampDiff) {
    var ellapsed;
    var s = parseInt(timeStampDiff / 1000);
    var m = parseInt(s / 60);
    var h = parseInt(m / 60);
    var d = parseInt(h / 24);

    if (d > 1) {
        ellapsed = 'Há ' + d + ' dias';
    }
    else if (d == 1) {
        ellapsed = 'Há ' + d + ' dia';
    }
    else if (h > 1) {
        ellapsed = 'Há ' + h + ' horas';
    }
    else if (h == 1) {
        ellapsed = 'Há ' + h + ' hora';
    }
    else if (m > 1) {
        ellapsed = 'Há ' + m + ' minutos';
    }
    else if (m == 1) {
        ellapsed = 'Há ' + m + ' minuto';
    }
    else if (s > 10) {
        ellapsed = 'Há ' + s + ' segundos';
    }
    else {
        ellapsed = 'neste momento';
    }
    return ellapsed;
}

