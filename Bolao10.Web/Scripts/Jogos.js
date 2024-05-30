Array.prototype.insere = function (ob) {

    for (var x = 0; x < this.length; x++) {
        if (ob.Valor == this[x].Valor) {
            this[x].Contar++;
            return;
        }
    }

    this[this.length] = ob;
}

Array.prototype.sortDesc = function () {

    var x, y, z;

    y = this.length - 2;

    while (y >= 0) {
        x = 0;
        z = -1;
        while (x <= y) {
            if (this[x].Contar < this[x + 1].Contar) {
                inverte(this, x);
                z = x - 1;
            }
            x++;
        }
        y = z;
    }
}

function inverte(a, i) {
    var t = a[i];
    a[i] = a[i + 1];
    a[i + 1] = t;
}

Array.prototype.getSoma = function () {

    var s = 0;

    for (var x = 0; x < this.length; x++) s += this[x].Contar;
    return s;
}

function Contador(valor) {
    this.Valor = valor;
    this.Contar = 1;
}

function getResultado(time1, time2, gols1, gols2) {


    if (gols1 == null || gols2 == null) {
        return "Nenhum";
    }

    if (gols1 > gols2) {
        return time1;
    } else if (gols1 < gols2) {
        return time2;
    } else {
        return "Empate";
    }


}

var Resumo = function (nome, valor, percent) {
    var self = this;
    self.Nome = ko.observable(nome);
    self.Valor = ko.observable(valor);
    self.Percent = ko.observable(percent);
}

var Aposta = function(participante, meusgols1, meusgols2, pontos, resultado, selecionado, ativo)
{
    var self = this;
    self.Participante = ko.observable(participante);
    self.MeusGols1 = ko.observable(meusgols1);
    self.MeusGols2 = ko.observable(meusgols2);
    self.Pontos = ko.observable(pontos);
    self.Resultado = ko.observable(resultado);
    self.Selecionado = ko.observable(selecionado);
    self.Ativo = ko.observable(ativo);

    self.Placar = ko.computed(function () {

        var meusGols1 = "";
        var meusGols2 = "";

        if (self.MeusGols1() != null)
            meusGols1 = self.MeusGols1().toString();

        if (self.MeusGols2() != null)
            meusGols2 = self.MeusGols2().toString();

        return(meusGols1 + "X" + meusGols2)
    });
}

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

    self.RequisitarDadosParaAsEstatisticas = function () {

        if (self.JogoSelecionado() != undefined) {
            self.JogoSelecionado().Selecionado(false);
        }

        this.Pesquisando(true);
        this.Selecionado(true);

        var time1 = this.Time1().Nome;
        var time2 = this.Time2().Nome;

        var jogo = this;

        var arrPlacares = new Array();
        var arrPontos = new Array();
        var arrResultados = new Array();

        jogo.ResumoPontos.removeAll();
        jogo.ResumoPlacar.removeAll();
        jogo.ResumoResultado.removeAll();
        jogo.Apostas.removeAll();

        $.ajax({
            url: "/Jogo/RequisitarDadosParaAsEstatisticas",
            data: 'jogoId=' + this.Id(),
            dataType: 'json',
            success: function (data) {

                var participanteAtivo = data.participante;

                $(data.apostas).each(function (index, aposta) {

                    var ativo = aposta.Participante.Codigo == participanteAtivo ? true : false;

                    var resultado = getResultado(time1, time2, aposta.Gols1, aposta.Gols2);

                    jogo.Apostas.push(new Aposta(aposta.Participante, aposta.Gols1, aposta.Gols2, aposta.Pontos, resultado, true, ativo));

                    var placar = "";

                    if ((aposta.Gols1 == null) || (aposta.Gols1 == null))
                    {
                        placar = "X"
                    } else {
                        placar = aposta.Gols1 + "X" + aposta.Gols2;
                    }

                    arrPlacares.insere(new Contador(placar));

                    arrPontos.insere(new Contador(aposta.Pontos));

                    arrResultados.insere(new Contador(resultado));

                });

                arrPlacares.soma = arrPlacares.getSoma();
                arrPlacares.sortDesc();

                var resumo = new Resumo("Todos", null, null);
                jogo.ResumoPlacar.push(resumo);

                for (var i = 0; i < arrPlacares.length; i++) {
                    var item = arrPlacares[i];
                    var percent = ((item.Contar / arrPlacares.soma) * 100).toFixed(2);
                    resumo = new Resumo(item.Valor, item.Contar, percent);
                    jogo.ResumoPlacar.push(resumo);
                }

                arrPontos.soma = arrPontos.getSoma();
                arrPontos.sortDesc();

                for (var i = 0; i < arrPontos.length; i++) {
                    var item = arrPontos[i];
                    var percent = ((item.Contar / arrPontos.soma) * 100).toFixed(2);
                    var resumo = new Resumo(item.Valor, item.Contar, percent);
                    jogo.ResumoPontos.push(resumo);
                }

                arrResultados.soma = arrResultados.getSoma();
                arrResultados.sortDesc();

                for (var i = 0; i < arrResultados.length; i++) {
                    var item = arrResultados[i];
                    var percent = ((item.Contar / arrResultados.soma) * 100).toFixed(2);
                    var resumo = new Resumo(item.Valor, item.Contar, percent);
                    jogo.ResumoResultado.push(resumo);
                }

                self.JogoSelecionado(jogo);

                jogo.Pesquisando(false);

                jogo.PlacaresSelecionados.push("Todos");

            },
            error: function (jqXHR, textoStatus, errorThrown) {
                alert('analisar erro');
                this.Pesquisando(false);
            }
        });

    }

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

        $.ajax({
            url: "/Jogo/Salvar",
            data: { jogoId: self.Id(), gols1: self.Gols1(), gols2: self.Gols2() },
            dataType: 'html',
            success: function (msg) {
                $("#results").append(msg);

                $.ajax({
                    url: "/Home/AtualizarPaineis",
                    dataType: 'html',
                    success: function(msg)
                    {
                        $("#results").append(msg);
                    },
                    error: function (jqXHR, textoStatus, errorThrown) {
                        alert('analisar erro');
                    }
                });
            },
            error: function (jqXHR, textoStatus, errorThrown) {
                alert('analisar erro');
            }
        });

    }

    self.Pesquisando = ko.observable(false);

    self.StatusPesquisa = ko.computed(function () {
        return (self.Pesquisando() == true ? "fa fa-spinner fa-spin" : "glyphicon glyphicon-search");
    });

    self.Selecionado = ko.observable(false);

    self.PlacaresSelecionados = ko.observableArray();

    self.PontosSelecionados = ko.observableArray();

    self.ResultadosSelecionados = ko.observableArray();

    self.Apostas = ko.observableArray([]);

    self.ResumoPontos = ko.observableArray([]);

    self.ResumoPlacar = ko.observableArray([]);

    self.ResumoResultado = ko.observableArray([]);

    self.FiltrarPontosSelecionados = ko.computed(function () {

        if (self.PontosSelecionados().length == 0)
            return;

        self.PlacaresSelecionados.removeAll();
        self.ResultadosSelecionados.removeAll();

        var pontosSelecionados = self.PontosSelecionados();

        var apostas = self.Apostas();

        ko.utils.arrayForEach(apostas, function (aposta) {
            var marcado = false;
            ko.utils.arrayForEach(pontosSelecionados, function (pontoSelecionado) {

                if ((aposta.Pontos() == pontoSelecionado)) {
                    marcado = true;
                }

            });
            aposta.Selecionado(marcado);
        });

    });

    self.FiltrarResultadosSelecionados = ko.computed(function () {

        if (self.ResultadosSelecionados().length == 0)
            return;

        self.PlacaresSelecionados.removeAll();
        self.PontosSelecionados.removeAll();

        var resultadosSelecionados = self.ResultadosSelecionados();

        var apostas = self.Apostas();

        ko.utils.arrayForEach(apostas, function (aposta) {
            var marcado = false;
            ko.utils.arrayForEach(resultadosSelecionados, function (resultadoSelecionado) {

                if ((aposta.Resultado() == resultadoSelecionado)) {
                    marcado = true;
                }
            });
            aposta.Selecionado(marcado);
        });

    });

    self.FiltrarPlacaresSelecionados = ko.computed(function () {

        if (self.PlacaresSelecionados().length == 0)
            return;

        self.PontosSelecionados.removeAll();
        self.ResultadosSelecionados.removeAll();

        var placaresSelecionados = self.PlacaresSelecionados();

        var apostas = self.Apostas();

        ko.utils.arrayForEach(apostas, function (aposta) {
            var marcado = false;
            ko.utils.arrayForEach(placaresSelecionados, function (placarSelecionado) {

                if ((aposta.Placar() == placarSelecionado) || (placarSelecionado == "Todos")) {
                    marcado = true;
                }

            });
            aposta.Selecionado(marcado);
        });

    });
    


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

            if (window.jogosViewModel.Index() == undefined)
                window.jogosViewModel.Index(window.jogosViewModel.Dias().length-1);

            ko.applyBindings(window.jogosViewModel);
            
        },
        error: function (jqXHR, textoStatus, errorThrown) {
            alert('analisar erro');
        }
    });
}
