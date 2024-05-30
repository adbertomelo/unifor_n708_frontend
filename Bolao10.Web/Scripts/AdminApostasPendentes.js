
var CON_NAO_ENVIADO = 0;
var CON_SENDO_ENVIADO = 1;
var CON_ENVIADO = 2;
var CON_ERRO_AO_ENVIAR = 3;

var ApostaPendente = function(fase, rodada, participante, email, numJogos, apostasFeitas, apostasPendentes, rodadaId)
{
    var self = this;
    self.Fase = ko.observable(fase);
    self.Rodada = ko.observable(rodada);
    self.Participante = ko.observable(participante);
    self.Email = ko.observable(email);
    self.NumJogos = ko.observable(numJogos);
    self.ApostasFeitas = ko.observable(apostasFeitas);
    self.ApostasPendentes = ko.observable(apostasPendentes);
    self.RodadaId = ko.observable(rodadaId);
    self.StatusDoEnvio = ko.observable(0);
    self.MensagemDeErro = ko.observable();
    self.Selecionado = ko.observable();
}

var viewModel = function (model) {

    var self = this;

    self.ApostasPendentes = ko.observableArray([]);

    $(model).each(function (index, apostaPendente) {

        self.ApostasPendentes.push(
            new ApostaPendente(apostaPendente[0], apostaPendente[1], apostaPendente[2], apostaPendente[3], apostaPendente[4], apostaPendente[5], apostaPendente[6], apostaPendente[7])
        );

    });

    self.enviarEmails = function (){ 

        $(self.ApostasPendentes()).each(function (index, apostaPendente) {
            if (apostaPendente.Selecionado() == true && apostaPendente.StatusDoEnvio() != CON_SENDO_ENVIADO) {
                apostaPendente.StatusDoEnvio(CON_SENDO_ENVIADO);
                $.ajax({
                    type:"POST",
                    url: "/Admin/EnviarEmail",
                    data:{ email: apostaPendente.Email(), nome: apostaPendente.Participante(), rodadaId: apostaPendente.RodadaId() },
                    success: function (data) {
                        if (data.trim().length > 0)                        {
                            apostaPendente.StatusDoEnvio(CON_ERRO_AO_ENVIAR);
                            apostaPendente.MensagemDeErro(data);
                        } else {
                            apostaPendente.Selecionado(false);
                            apostaPendente.StatusDoEnvio(CON_ENVIADO);
                        }
                        
                    },
                    error: function (jqXHR, textoStatus, errorThrown) {
                        apostaPendente.StatusDoEnvio(3);
                    }
                });
            }
        });
    }
};

$(function () {
    requisitarApostasPendentes();
});

function requisitarApostasPendentes() {
    $.ajax({
        url: "/Admin/GetApostasPendentes",
        dataType: 'json',
        success: function (data) {
            window.apostasPendentesViewModel = new viewModel(data);
            ko.applyBindings(window.apostasPendentesViewModel);

        },
        error: function (jqXHR, textoStatus, errorThrown) {
            alert('analisar erro');
        }
    });
}

