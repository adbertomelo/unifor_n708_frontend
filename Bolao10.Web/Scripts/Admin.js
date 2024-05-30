

var Dia = function(dia, jogos)
{
    var self = this;

    self.Dia = ko.observable(dia);

    self.Jogos = ko.observableArray([]);

    self.JogoSelecionado = ko.observable();

    $(jogos).each(function (index, jogo) {
        self.Jogos.push(
            new Jogo(jogo.Id, jogo.Gols1, jogo.Gols2, jogo.Time1, jogo.Time2)
        );
    });

 }

var Jogo = function (id, gols1, gols2, time1, time2) {

    var self = this;

    self.Id = ko.observable(id);
    self.Gols1 = ko.observable(gols1);
    self.Gols2 = ko.observable(gols2);
    self.Time1 = ko.observable(time1);
    self.Time2 = ko.observable(time2);
    self.IdTime1 = ko.computed(function () {
        var id = "jogo_" + self.Id() + "_time1_" + self.Time1().Id;
        return (id);
    })
    self.IdTime2 = ko.computed(function () {
        return ("jogo_" + self.Id() + "_time2_" + self.Time2().Id);
    })
    self.Salvar = function () {

        var time1 = this.Time1().Nome;
        var time2 = this.Time2().Nome;

        self.Salvando(true);

        $.ajax({
            url: "/Jogo/Salvar",
            data: { jogoId: self.Id(), gols1: self.Gols1(), gols2: self.Gols2() },
            dataType: 'html',
            success: function (msg) {

                $("#results").append(msg);

                if (msg.indexOf("Erro:") < 0)
                {

                    $.ajax({
                        url: "/RealTime/AtualizarPaineis",
                        dataType: 'html',
                        success: function (msg) {
                            $("#results").append(msg);
                        },
                        error: function (jqXHR, textoStatus, errorThrown) {
                            alert('Analisar Erro');
                        }
                    });

                }

                self.Salvando(false);

            },
            error: function (jqXHR, textoStatus, errorThrown) {
                alert('Analisar Erro:' + errorThrown);
                self.Salvando(false);
            }
        });

    }

    self.Salvando = ko.observable(false);

    self.StatusSalvamento = ko.computed(function () {
        return (self.Salvando() == true ? "fa fa-spinner fa-spin" : "glyphicon glyphicon-floppy-disk");
    });

    self.Selecionado = ko.observable(false);

}

var viewModel = function (model) {

    var self = this;

    self.Dias = ko.observableArray([]);

    self.Index = ko.observable();

    self.DiaAnterior = function () {
        diaAnterior();
    };

    self.ProximoDia = function () {
        proximoDia();
    };

    $(model).each(function (index, dia_jogos) {

        var value = new Date(parseInt(dia_jogos.dia.substr(6)));

        var ret = value.getDate() + "/" + (value.getMonth() + 1);

        if (self.Index() == undefined) {
            if (dia_jogos.arealizar == true)
                self.Index(index);
        }

        self.Dias.push(
            new Dia(ret, dia_jogos.jogos)
        );

    });

    self.Dia = ko.computed( function () {
        return (self.Dias()[self.Index()]);
    } );

};

function proximoDia() {
    var index = window.jogosViewModel.Index();
    if (index < window.jogosViewModel.Dias().length - 1)
        window.jogosViewModel.Index(index + 1);

}

function diaAnterior() {
    var index = window.jogosViewModel.Index();
    if (index > 0)
        window.jogosViewModel.Index(index - 1);

}

$(function () {
    requisitarJogos();
});

function requisitarJogos() {
    $.ajax({
        url: "/Jogo/GetJogosDasRodadasFechadas",
        dataType: 'json',
        success: function (data) {
            window.jogosViewModel = new viewModel(data);
            ko.applyBindings(window.jogosViewModel);
            
        },
        error: function (jqXHR, textoStatus, errorThrown) {
            alert('analisar erro');
        }
    });
}
