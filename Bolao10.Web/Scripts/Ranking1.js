
var Aposta = function(jogo, gols1, gols2, pontos, posicao)
{
    var self = this;

    self.Jogo = ko.observable(jogo);
    self.Gols1 = ko.observable(gols1);
    self.Gols2 = ko.observable(gols2);
    self.Pontos = ko.observable(pontos);
    self.Posicao = ko.observable(posicao);

    self.Placar = ko.computed(function () {
        return(self.Jogo().Gols1 + " X " + self.Jogo().Gols2)
    });

    self.Palpite = ko.computed(function () {
        return (self.Gols1() + " X " + self.Gols2())
    });

}

var Ranking = function(rank, codigo, nome, pontos, pts3, pts2, pts1, pts0, ativo, marcado)
{
    var self = this;

    self.Rank = ko.observable(rank);
    self.Codigo = ko.observable(codigo);
    self.Nome = ko.observable(nome);
    self.Pontos = ko.observable(pontos);
    self.Pts3 = ko.observable(pts3);
    self.Pts2 = ko.observable(pts2);
    self.Pts1 = ko.observable(pts1);
    self.Pts0 = ko.observable(pts0);
    self.Ativo = ko.observable(ativo);
    //self.Marcado = ko.observable(marcado);
    //self.Visivel = ko.observable(marcado);
    //self.Destacado = ko.observable(false);
    //self.Filtrado = ko.observable(true);
    self.Apostas = ko.observableArray([]);

    self.IdDivChart = ko.computed(function () {
        return("chart" + self.Codigo() );
    });

    self.TemApostas = ko.computed(function () {
        return (self.Apostas().length > 0);
    });

    self.NoChart = ko.observable(true);

    self.Expandido = ko.observable(0);

    Chart = function(data, event)
    {
        var codigoParticipante = data.Codigo();
        var idDiv = "chart" + codigoParticipante;

        if (!data.NoChart()) {
            document.getElementById(idDiv).innerText = "";
            data.NoChart(true);
            return;
        }

        data.NoChart(false);
        var nome = data.Nome();

        $.ajax({
            url: "/ranking/getDataChart",
            dataType: 'json',
            data: 'codigoParticipante=' + codigoParticipante,
            success: function (result) {


                var tdata = new google.visualization.DataTable();

                tdata.addColumn('string', 'Jogo');
                tdata.addColumn('number', 'Você');
                tdata.addColumn('number', nome);

                for (var i = 0; i < result.length; i++) {
                    var row = result[i];
                    tdata.addRow([row[1], row[2], row[3]]);
                }

                var options = {
                    title: "Posição no ranking",
                    vAxis: { minValue: 1 }
                };

                var chart = new google.visualization.AreaChart(document.getElementById(idDiv));
                chart.draw(tdata, options);
            },
            error: function (jqXHR, textoStatus, errorThrown) {
                alert('analisar erro');
            }
        });

    }

    ExibirJogos = function (data, event) {

        var eu = data;

        if (data.Expandido() == 1)
        {
            data.Expandido(0);
            data.Apostas.removeAll();
            return;
        }

        data.Expandido(2);
        data.NoChart(true);

        var current = data;

        var codigoParticipante = data.Codigo();

        $.ajax({
            url: "/Ranking/GetApostas",
            data: 'codigoParticipante=' + codigoParticipante,
            dataType: 'json',
            success: function (data) {
                ko.utils.arrayForEach(data, function (item) {
                    var aposta = new Aposta(item.Jogo, item.Gols1, item.Gols2, item.Pontos, item.Posicao);
                    current.Apostas.push(aposta);
                });
                eu.Expandido(1);
            },
            error: function (jqXHR, textoStatus, errorThrown) {
                alert('analisar erro');
            }
        });
    }

    //MouseOver = function (data, event) {
    //    data.Visivel(true);
    //};

    //MouseOut = function (data, event) {

    //    if (data.Marcado() == false)
    //        data.Visivel(false);
    //};

    //Destacar = function (data, event) {
    //    if (data.Marcado() == false)
    //        data.Destacado(true);

    //};

    //DesfazDestacacao = function (data, event) {

    //    data.Destacado(false);

    //}

    //self.Marcar = function () {
    //    if (self.Marcado() == true)
    //    {
    //        self.Marcado(false);

    //        $.ajax({
    //            type: "POST",
    //            url: "/Ranking/Desmarcar",
    //            data: { codigoParticipanteMarcado: self.Codigo },
    //            success: function (data) {

    //            },
    //            error: function (jqXHR, textoStatus, errorThrown) {

    //            }
    //        });
    //    }            
    //    else
    //    {
    //        self.Marcado(true);

    //        $.ajax({
    //            type: "POST",
    //            url: "/Ranking/Marcar",
    //            data: { codigoParticipanteMarcado: self.Codigo },
    //            success: function (data) {

    //            },
    //            error: function (jqXHR, textoStatus, errorThrown) {
                    
    //            }
    //        });
    //    }
            
    //}

    //self.Star = ko.computed(function () {
    //    return (self.Marcado() == true ? self.Destacado() == true ? 'fa fa-star amarelo destacado' : 'fa fa-star amarelo' : self.Destacado() == true ? 'fa fa-star cinza destacado' : 'fa fa-star cinza');
    //});

}

var viewModel = function (data) {

    var self = this;
    
    self.Ranking = ko.observableArray();

    //var filtrado = false;

    //Filtrar = function () {

    //    if (filtrado == false)
    //    {
    //        ko.utils.arrayForEach(self.Ranking(), function (item) {

    //            if (!item.Marcado())
    //            {
    //                item.Filtrado(false);
    //            }

    //        });

    //        filtrado = true;
    //    }
    //    else
    //    {
    //        ko.utils.arrayForEach(self.Ranking(), function (item) {

    //            if (!item.Marcado()) {
    //                item.Filtrado(true);
    //            }

    //        });

    //        filtrado = false;
    //    }
            


    //};


    ko.utils.arrayForEach(data.ranking, function (item) {

        var ativo = item.Codigo == data.participante ? true : false;

        var participante = new Ranking(item.Rank, item.Codigo, item.Nome, item.PT, item.PT3, item.PT2, item.PT1, item.PT0, ativo, item.Marcado)

        self.Ranking.push(participante)

    });
};

$(function () {
    requisitarRanking();
});


function requisitarRanking() {
    $.ajax({
        url: "/Ranking/GetRanking",
        dataType: 'json',
        success: function (data) {
            window.rankingViewModel = new viewModel(data);
            ko.applyBindings(window.rankingViewModel);
            
        },
        error: function (jqXHR, textoStatus, errorThrown) {
            alert('analisar erro');
        }
    });
}

function exibirGrafico()
{

}