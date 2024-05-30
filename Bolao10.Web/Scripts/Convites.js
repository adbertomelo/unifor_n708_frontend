
var CON_CONVITE_NAO_ENVIADO = 0;
var CON_CONVITE_SENDO_ENVIADO = 1;
var CON_CONVITE_ENVIADO = 2;
var CON_CONVITE_ERRO_AO_ENVIAR = 3;

var Convite = function(emailConvidado, nomeConvidado, situacaoConvite, selecionado, statusDoEnvio)
{
    var self = this;
    self.EmailConvidado = ko.observable(emailConvidado);
    self.NomeConvidado = ko.observable(nomeConvidado);
    self.Situacao = ko.observable(situacaoConvite);
    self.Selecionado = ko.observable(selecionado);
    self.StatusDoEnvio = ko.observable(statusDoEnvio);
    self.MensagemDeErro = ko.observable();
}

var viewModel = function (model) {
    var self = this;

    self.Convites = ko.observableArray([]);

    $(model).each(function (index, convite) {

        self.Convites.push(
            new Convite(convite[0], convite[1], convite[2], false, convite[3])
        );

    });

    self.enviarConvites = function (){ 

        $(self.Convites()).each(function (index, convite) {
            if (convite.Selecionado() == true && convite.StatusDoEnvio() != CON_CONVITE_SENDO_ENVIADO) {
                convite.StatusDoEnvio(CON_CONVITE_SENDO_ENVIADO);
                $.ajax({
                    type:"POST",
                    url: "/Convite/EnviarConvite",
                    data:{ email: convite.EmailConvidado, nome: convite.NomeConvidado },
                    success: function (data) {
                        if (data.trim().length > 0)                        {
                            convite.StatusDoEnvio(CON_CONVITE_ERRO_AO_ENVIAR);
                            convite.MensagemDeErro(data);
                        } else {
                            convite.Selecionado(false);
                            convite.StatusDoEnvio(CON_CONVITE_ENVIADO);
                        }
                        
                    },
                    error: function (jqXHR, textoStatus, errorThrown) {
                        convite.StatusDoEnvio(3);
                    }
                });
            }
        });
    }
};

$(function () {
    requisitarConvites();
});

function requisitarConvites() {
    $.ajax({
        url: "/Convite/GetHistoricoConvites",
        dataType: 'json',
        success: function (data) {
            window.convitesViewModel = new viewModel(data);
            ko.applyBindings(window.convitesViewModel);

        },
        error: function (jqXHR, textoStatus, errorThrown) {
            alert('analisar erro');
        }
    });
}

