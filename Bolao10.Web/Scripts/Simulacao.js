
var _apostas;
var _participante;

var Time = function (id, nome, sigla, figura, especial) {
    this.Id = id;
    this.Nome = nome;
    this.Sigla = sigla;
    this.Figura = figura;
    this.Especial = especial;
}

var Dia = function(dia, jogos)
{
    var self = this;
    self.Dia = ko.observable(dia);
    self.Jogos = ko.observableArray([]);

    $(jogos).each(function (index, jogo) {

        var Time1 = new Time(jogo.Time1_Id, jogo.Nome_Time1, jogo.Sigla_Time1, jogo.Figura_Time1, jogo.Especial);
        var Time2 = new Time(jogo.Time2_Id, jogo.Nome_Time2, jogo.Sigla_Time2, jogo.Figura_Time2, jogo.Especial);

        self.Jogos.push(
            new Jogo(jogo.Jogo_Id, jogo.Gols1, jogo.Gols2, jogo.Meus_Gols1, jogo.Meus_Gols2, Time1, Time2)
        );

    });

}

var Jogo = function (id, gols1, gols2, meusgols1, meusgols2, time1, time2) {

    var self = this;

    self.Id = ko.observable(id);
    self.Gols1 = ko.observable(gols1);
    self.Gols2 = ko.observable(gols2);
    self.Time1 = ko.observable(time1);
    self.Time2 = ko.observable(time2);
    self.MeusGols1 = ko.observable(meusgols1);
    self.MeusGols2 = ko.observable(meusgols2);

    self.IdTime1 = ko.computed(function () {
        var id = "jogo_" + self.Id() + "_time1_" + self.Time1().Id;
        return (id);
    })
    self.IdTime2 = ko.computed(function () {
        return ("jogo_" + self.Id() + "_time2_" + self.Time2().Id);
    })

    self.MeuPlacar = ko.computed(function () {
        return("Meu palpite: " + self.MeusGols1() + " X " + self.MeusGols2());
    });
    
    self.PreencherPlacar = function () {
        self.Gols1(self.MeusGols1());
        self.Gols2(self.MeusGols2());
        gerarRanking();
    };

    self.LimparPlacar = function () {
        self.Gols1("");
        self.Gols2("");
        gerarRanking();
    };
}

function placarValido(gols1, gols2)
{
    if (IsValid(gols1) && IsValid(gols2))
    {
        return (true);
    } else {
        if (gols1 == "" || gols2 == "" || gols1 == null || gols2 == null)
            return (true);
    }
        
    return (false);

}
function IsValid(val)
{
    var numbers = "0123456789";

    if (val == null || val == "")
        return (false);

    var stringVal = val.toString();

    for (var i = 0; i < stringVal.length; i++)
    {
        var c = stringVal.substr(i, 1);

        if (numbers.indexOf(c) >= 0)
            return true;
    
    }

    return (false);

}
var viewModel = function (model) {
    var self = this;

    self.Dias = ko.observableArray([]);
    self.Index = ko.observable();
    self.GerarRanking = function (data, event) {

        if (placarValido(data.Gols1(), data.Gols2()))
        {
            gerarRanking();
        }

        return true;
    };

    self.DiaAnterior = function (){
        diaAnterior();
    };

    self.ProximoDia = function () {
        proximoDia();
    };

    $(model).each(function (index, dia_jogos) {

        var value = new Date(parseInt(dia_jogos.dia.substr(6)));
        var ret = value.getDate() + "/" + (value.getMonth() + 1);
        
        if (self.Index() == undefined)
        {
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
    requisitarApostas();
});

function requisitarApostas() {
    $.ajax({
        url: "/Simulacao/GetApostas",
        dataType: 'json',
        success: function (data) {
            _apostas = data;
            requisitarJogos();
        },
        error: function (jqXHR, textoStatus, errorThrown) {
            alert('analisar erro');
        }
    });
}

function requisitarJogos() {


    $.ajax({
        url: "/Simulacao/GetTodosOsJogos",
        dataType: 'json',
        success: function (data) {
            window.jogosViewModel = new viewModel(data.Jogos);

            if (window.jogosViewModel.Index() == undefined)
                window.jogosViewModel.Index(window.jogosViewModel.Dias().length - 1);

            ko.applyBindings(window.jogosViewModel);
            _participante = data.Participante;
            gerarRanking();
            
        },
        error: function (jqXHR, textoStatus, errorThrown) {
            alert('analisar erro');
        }
    });
}

function Participante(id, nome) {
    this.Id = id;
    this.Nome = nome;
}

function MeuJogo(id, time1, time2, gols1, gols2) {
    this.Id = id;
    this.Time1 = time1;
    this.Time2 = time2;
    this.Gols1 = gols1;
    this.Gols2 = gols2;
}

function Time(id) {
    this.Id = id;
}

function Pontos(p0, p1, p2, p3) {

    this.P0 = p0;
    this.P1 = p1;
    this.P2 = p2;
    this.P3 = p3;

    this.PT = this.P1 + this.P2 * 2 + this.P3 * 3;
}

function Ranking(participante, pontos) {

    this.Participante = participante;

    this.Pontos = pontos;

}

Ranking.prototype.AdicionarPontos = function (p) {

    this.Pontos.P0 += p.P0;
    this.Pontos.P1 += p.P1;
    this.Pontos.P2 += p.P2;
    this.Pontos.P3 += p.P3;
    this.Pontos.PT += p.PT;

}

var ranking;

var CON_APOSTA_ID = 0;
var CON_PARTICIPANTE_ID = 1;
var CON_BOLAO_ID = 2;
var CON_NOME_USUARIO = 3;
var CON_JOGO_ID = 4;
var CON_TIME1_ID = 5;
var CON_GOLS1 = 6;
var CON_MEUS_GOLS1 = 7;
var CON_TIME2_ID = 8;
var CON_GOLS2 = 9;
var CON_MEUS_GOLS2 = 10;

function gerarRanking() {

    ranking = [];

    var idJogoAtual = 0;

    for (var item = 0; item < _apostas.length; item++) {

        var obj = _apostas[item];

        var participante = new Participante(obj[CON_PARTICIPANTE_ID], obj[CON_NOME_USUARIO]);

        var time1 = new Time(obj[CON_TIME1_ID]);

        var time2 = new Time(obj[CON_TIME2_ID]);

        //var jogoID = "jogo_" + obj[CON_JOGO_ID];

        var meusgols1 = obj[CON_MEUS_GOLS1];

        var meusgols2 = obj[CON_MEUS_GOLS2];

        var meujogo = new MeuJogo(obj[CON_JOGO_ID], time1, time2, meusgols1, meusgols2);

        var palpite;

        if (idJogoAtual != obj[CON_JOGO_ID])
        {
            palpite = getPalpite(meujogo);
        }            

        var pontos = calcularPontos(meujogo, palpite);

        idJogoAtual = obj[CON_JOGO_ID];

        adicionarAoRanking(participante, pontos);

    }

    ordenarRanking();

    document.getElementById("divRanking").innerHTML = getHTMLRanking();

}

function getPalpite(jogo) {

    var id1 = "jogo_" + jogo.Id + "_time1_" + jogo.Time1.Id;

    var palpite1 = document.getElementById(id1).value;

    var id2 = "jogo_" + jogo.Id + "_time2_" + jogo.Time2.Id;

    var palpite2 = document.getElementById(id2).value;

    var palpite = new Palpite(palpite1, palpite2);

    return palpite;
}

function ordenarRanking() {

    ranking.sort(compararPontos);

}

function compararPontos(a, b) {

    if (a.Pontos.PT > b.Pontos.PT) {
        return -1;
    }

    if (a.Pontos.PT < b.Pontos.PT) {
        return 1;
    }

    if (a.Pontos.P3 > b.Pontos.P3) {
        return -1;
    }

    if (a.Pontos.P3 < b.Pontos.P3) {
        return 1;
    }

    if (a.Pontos.P2 > b.Pontos.P2) {
        return -1;
    }

    if (a.Pontos.P2 < b.Pontos.P2) {
        return 1;
    }

    if (a.Pontos.P1 > b.Pontos.P1) {
        return -1;
    }

    if (a.Pontos.P1 < b.Pontos.P1) {
        return 1;
    }

    if (a.Participante.Nome < b.Participante.Nome) {
        return -1;
    }

    if (a.Participante.Nome > b.Participante.Nome) {
        return 1;
    }

    return 0;

}

function adicionarAoRanking(participante, pontos) {

    if (pontos == null)
        return;

    var index = -1;

    for (var i = 0; i < ranking.length; i++) {
        if (ranking[i].Participante.Id == participante.Id) {
            index = i;
            break;
        }
    }

    if (index <= -1) {
        ranking[ranking.length] = new Ranking(participante, pontos);
    } else {
        ranking[index].AdicionarPontos(pontos);
    }
}

function Palpite(gols1, gols2) {

    this.Gols1 = parseInt(gols1, 10);
    this.Gols2 = parseInt(gols2, 10);

}

function calcularPontos(jogo, palpite) {

    var meusgols1 = jogo.Gols1;

    var meusgols2 = jogo.Gols2;

    var ptEspecial = 0;

    if (palpite.Gols1 == null || palpite.Gols2 == null || meusgols1 == null || meusgols2 == null) {
        return new Pontos(1, 0, 0, 0);
    }

    if ((palpite.Gols1 == meusgols1) && (palpite.Gols2 == meusgols2)) {

        return new Pontos(0, 0, 0, 1);

    } else if ((palpite.Gols1 - palpite.Gols2) == (meusgols1 - meusgols2)) {

        return new Pontos(0, 0, 1, 0);

    } else if ((((palpite.Gols1 - palpite.Gols2) > 0) && ((meusgols1 - meusgols2) > 0)) || (((palpite.Gols1 - palpite.Gols2) < 0) && ((meusgols1 - meusgols2) < 0))) {

        return new Pontos(0, 1, 0, 0);

    } else {

        return new Pontos(1, 0, 0, 0);

    }

    return (null);
}

function getHTMLRanking() {

    var rows = "";
    var header = "<thead><tr><th>Pos</th><th>Nome</th><th>PT</th><th class='visible-md visible-lg'>3Pts</th><th class='visible-md visible-lg'>2Pts</th><th class='visible-md visible-lg'>1Pt</th><th class='visible-md visible-lg'>0Pt</th></tr></thead>";
    var rank;
    var ponto1, ponto2;
    var style;
    var user;

    rank = 0;

    ponto2 = new Pontos(null, null, null, null, null);

    user = "";//document.getElementById("usuario").value;

    for (var i = 0; i < ranking.length; i++) {

        ponto1 = ranking[i].Pontos;

        if ((ponto1.PT == ponto2.PT) && (ponto1.P3 == ponto2.P3) && (ponto1.P2 == ponto2.P2) && (ponto1.P1 == ponto2.P1) && (ponto1.PTBR == ponto2.PTBR)) {
            rank = rank;
        } else {
            rank += 1;
        }

        ponto2 = ponto1;

        var style = "";
        var classUserActive = "";

        if (ranking[i].Participante.Id.toString() == _participante.Id.toString())
        {
            classUserActive = " class='user-active'";
        }

        if (rank <= 3)
        {
            style = "active";
        }

        rows += "<tr class='" + style + "'>" + "<td>" + rank.toString() + "</td>" + "<td>" + "<span" + classUserActive + ">" + ranking[i].Participante.Nome + "</span></td>" + "<td>" + ranking[i].Pontos.PT.toString() + "</td>" + "<td class='visible-md visible-lg'>" + ranking[i].Pontos.P3.toString() + "</td>" + "<td class='visible-md visible-lg'>" + ranking[i].Pontos.P2.toString() + "</td>" + "<td class='visible-md visible-lg'>" + ranking[i].Pontos.P1.toString() + "</td>" + "<td class='visible-md visible-lg'>" + ranking[i].Pontos.P0.toString() + "</td>" + "</tr>"
    }

    return "<table class='table table-hover'>" + header + "<tbody>" + rows + "</tbody>" + "</table>";
}

function preencheMeusPlacaresHoje() {

    preenchePlacarOficial();

    var colINPUT = document.getElementsByTagName("INPUT");
    var oINPUT;

    for (var i = 0; i < colINPUT.length; i++) {

        oINPUT = colINPUT[i];

        if ((oINPUT.getAttribute("hoje") == "S") && (oINPUT.value == "")) {

            var a = oINPUT.getAttribute("meusgols");

            if (a != null) {
                oINPUT.value = oINPUT.getAttribute("meusgols");
            }
        }
    }
}

function preenchePlacarOficial() {

    var colINPUT = document.getElementsByTagName("INPUT");
    var oINPUT;

    for (var i = 0; i < colINPUT.length; i++) {
        oINPUT = colINPUT[i];

        var a = oINPUT.getAttribute("golsoficial");

        if (a != null) {
            oINPUT.value = oINPUT.getAttribute("golsoficial");
        }
    }
}

function preenchePlacarNosJogosNaoRealizados() {

    var colINPUT = document.getElementsByTagName("INPUT");
    var oINPUT;

    for (var i = 0; i < colINPUT.length; i++) {
        oINPUT = colINPUT[i];

        if (oINPUT.value == "") {
            var a = oINPUT.getAttribute("meusgols");

            if (a != null) {
                oINPUT.value = oINPUT.getAttribute("meusgols");
            }
        }

    }
}

function getApostaJogo(jogoID, time1ID, time2ID) {

    var id1 = "jogo_" + jogoID + "_time1_" + time1ID;
    var input1 = document.getElementById(id1);
    input1.value = input1.getAttribute("meusgols");

    var id2 = "jogo_" + jogoID + "_time2_" + time2ID;
    var input2 = document.getElementById(id2);
    input2.value = input2.getAttribute("meusgols");

    gerarRanking();

}
