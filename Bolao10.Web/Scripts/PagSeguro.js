
const ERRO_AO_DEFINIR_SESSION_ID = "Erro ao iniciar o processo de pagamento. Atualize a página, por favor.";
const MENSAGEM_AGUARDANDO_PROCESSAMENTO = "Aguarde enquanto o pagamento é processado.";
const CEP_NAO_ENCONTRADO = "O CEP digitado não foi encontrado. Verifique se o CEP está correto.";
const ERRO_AO_PESQUISAR_CEP = "Erro ao pesquisar o cep. Atualize a página e informe novamente.";
const ERRO_BANDEIRA_INVALIDA = "Não foi possível identificar a bandeira do cartão. Informe outro número de cartão.";

var Processamento = function ()
{
    self = this;

    self.DirecionarParaPaginaOk = function(params)
    {
        if (params === undefined || params === "" || params === null)
            location.href = "/pagseguro/processamentook"
        else
            location.href = "/pagseguro/processamentook?p=" + params;
    };

    self.MostrarMensagem = function (titulo, mensagem) {

        var modal = $('#myModal');

        modal.find('.modal-title').text(titulo);

        modal.find('.modal-body').find('p').html(mensagem);

        modal.modal('show');

    };

    self.DirecionarParaPaginaDeErro = function (errors)
    {

        var msgs = "";

        $.each(errors, function (idx, error) {

            var codErro = error.code;
            var msgErro = error.message;

            if (codErro == "10000")
                msgErro = ERRO_BANDEIRA_INVALIDA ;

            if (msgs == "")
                msgs = msgErro;
            else
                msgs = msgs + "<br/>" + msgErro;

        });

        self.MostrarMensagem("Erro", msgs);

    };


    self.MensagemDeErro = function(mensagem){
          
        self.MostrarMensagem("Erro", mensagem);
        //alert(mensagem);
    };

    self.AbrirTelaDeEspera = function (mensagem) {
        
        var msg = mensagem;

        if (msg === null || msg === undefined || msg === "")
            msg = MENSAGEM_AGUARDANDO_PROCESSAMENTO;

        waitingDialog.show(msg)

    };

    self.OcultarTelaDeEspera = function () {
        waitingDialog.hide();
    };
}

var MeuPagSeguro = function()
{
    var self = this;

    self.SessionId = "";

    self.SenderHash = "";

    self.CardToken = "";

    self.SetSenderHash = function()
    {
        self.SenderHash = PagSeguroDirectPayment.getSenderHash();
    }

    self.SetSessionId = function()
    {

        $.ajax({
            url: '/pagseguro/GetSessionAsync',
            type: 'POST',
            dataType: 'JSON',
            async: true,
            success: function (data) {

                self.SessionId = data;

                PagSeguroDirectPayment.setSessionId(self.SessionId);

            },
            error: function (jqXHR, textoStatus, errorThrown) {

                _processamento.MensagemDeErro(ERRO_AO_DEFINIR_SESSION_ID);
                document.getElementById("divTiposDePagamento").style.display = "none";
                document.getElementById("divMensagemErroSessionId").style.display = "block";
            }

        });
    }

    self.GetMeiosPagamentos = function () {

            PagSeguroDirectPayment.getPaymentMethods({
                amount: 100.00,
                success: function (response) {

                    return response.paymentMethods;

                },
                error: function (response) {
                    
                    _processamento.MensagemDeErro(response);

                }
            });
        
    };

    self.Params = function () {
        return {

            SessionId: self.SessionId,
            SenderHash: self.SenderHash,
            CardToken: self.CardToken
        };
    };
}

var InfoCartao = function(numero, validade, nomeTitular, codigoSeguranca)
{
    var self = this;

    self.Numero = ko.observable(numero);
    self.Brand = ko.observable("");
    self.Validade = ko.observable(validade);
    self.NomeTitular = ko.observable(nomeTitular).extend({ uppercase: true});
    self.CodigoSeguranca = ko.observable(codigoSeguranca);
    self.CVVSize = ko.observable("0");

    self.AnoValidade = ko.computed(function () {
       return self.Validade().substr(3, 4)
    }); 

    self.MesValidade = ko.computed(function () {
       return  self.Validade().substr(0, 2)
    });

    self.Params = function () {

        var infoCartaoParams = {
            Numero: self.Numero(),
            Validade: self.Validade(),
            NomeTitular: self.NomeTitular(),
            CodigoSeguranca: self.CodigoSeguranca()
        };

        return infoCartaoParams;
    };
}

var InfoPagador = function (cpf, dataNascimento, telefone, cep, endereco, numeroCasa, complemento, bairro) {
    var self = this;

    self.CPF = ko.observable(cpf);
    self.DataNascimento = ko.observable(dataNascimento);
    self.Telefone = ko.observable(telefone);
    self.CEP = ko.observable(cep);
    self.Endereco = ko.observable(endereco);
    self.NumeroCasa = ko.observable(numeroCasa);
    self.Complemento = ko.observable(complemento);
    self.Bairro = ko.observable(bairro);
    self.Cidade = ko.observable("");
    self.UF = ko.observable("");

    //

    self.CEPOk = ko.observable(false);

    self.PesquisouCEP = ko.observable(false);

    self.LimparEndereco = function () {
        self.Endereco("");
        self.Bairro("");
        self.Cidade("");
        self.UF("");
        self.NumeroCasa("");
        self.Complemento("");
        return;
    };

    self.PodePesquisarCEP = function (cep) {

        if (cep.indexOf("-") >= 0) {
            if (cep.length != 9) //informou 99999-999
                return false;
        } else {
            if (cep.length != 8) //informou 9999999
                return false;
        };

        return true;
    };

    self.CEP.subscribe(

        function (value) {

            var cep = value;

            self.LimparEndereco();

            if (!self.PodePesquisarCEP(cep))
                return;

            $.ajax({
                url: '/pagseguro/BuscarCEP?cep=' + cep,
                type: 'GET',
                dataType: 'JSON',
                success: function (data) {

                    var res = JSON.parse(data);

                    if (res.erro) {
                        _processamento.MensagemDeErro(CEP_NAO_ENCONTRADO);
                        self.CEPOk(false);
                        return;
                    }

                    self.Endereco(res.logradouro);
                    self.Bairro(res.bairro);
                    self.Cidade(res.localidade);
                    self.UF(res.uf);
                    self.CEPOk(true);

                },
                error: function (jqXHR, textoStatus, errorThrown) {
                    self.CEPOk(false);
                    _processamento.MensagemDeErro(ERRO_AO_PESQUISAR_CEP);
                },
                complete: function (response) {
                    self.PesquisouCEP(true);
                }

            });

        }

    );

    //

    
    self.Params = function () {
        return {
            CPF: self.CPF(),
            DataNascimento: self.DataNascimento(),
            Telefone: self.Telefone(),
            CEP: self.CEP(),
            Endereco: self.Endereco(),
            NumeroCasa: self.NumeroCasa(),
            Complemento: self.Complemento(),
            Bairro: self.Bairro(),
            Cidade: self.Cidade(),
            UF: self.UF()
        }
    };
};

var Boleto = function (infoPagador) {

    var self = this;

    self.InfoPagador = ko.observable(infoPagador);

    self.PaymentLink = ko.observable("");

    self.GerarBoleto = function () {

        _meuPagSeguro.SetSenderHash();

        var infoPagadorParams = self.InfoPagador().Params();

        var infoPagSeguroParams = _meuPagSeguro.Params();

        $.ajax({
            url: '/pagseguro/GerarBoleto',
            data: JSON.stringify({ infoPagador: infoPagadorParams, infoPagSeguro: infoPagSeguroParams }),
            contentType: "application/json; charset=ISO-8859-1",
            type: 'POST',
            dataType: 'JSON',
            async: true,
            beforeSend: function(jqXHR, settings){
                _processamento.AbrirTelaDeEspera();
            },
            success: function (data) {

                if (data.error)
                {
                    _processamento.DirecionarParaPaginaDeErro(data.errors);
                }
                else
                {
                    //self.PaymentLink(data.paymentLink);
                    _processamento.DirecionarParaPaginaOk(data.paymentLink);
                }
                

            },
            error: function (jqXHR, textoStatus, errorThrown) {

                _processamento.MensagemDeErro(errorThrown);
            },
            complete: function(jqXHR, textStatus)
            {
                _processamento.OcultarTelaDeEspera();
            }

        });

    };
 
    self.ProcessarPagamento = function () {


        var infoPagadorParams = self.InfoPagador().Params();

        $.ajax({
            url: '/pagseguro/validarinfoboleto',
            data: JSON.stringify({ infoPagador: infoPagadorParams }),
            contentType: "application/json; charset=ISO-8859-1",
            type: 'POST',
            dataType: 'JSON',
            async: true,
            success: function (data) {

                if (data.error) {
                    _processamento.DirecionarParaPaginaDeErro(data.errors)
                }
                else {
                    self.GerarBoleto();
                }

            },
            error: function (jqXHR, textoStatus, errorThrown) {

                _processamento.MensagemDeErro(errorThrown);

            }
        });
    }
}

var CartaoCredito = function(infoCartao, infoPagador)
{
    var self = this;

    self.InfoCartao = ko.observable(infoCartao);

    self.InfoPagador = ko.observable(infoPagador);

    self.UrlBrand = ko.observable("");

    self.InfoCartao().Numero.subscribe(
        function (value) {

            if (value.length >= 6)
            {
                var brand = value.substr(0, 6);

                if (brand != self.InfoCartao().Brand())
                    self.SetBrand(brand);
                
            } else {
                self.UrlBrand("");
                self.InfoCartao().Brand("");
            }
        }
    );


    self.Pagar = function() {

        _processamento.AbrirTelaDeEspera();

        _meuPagSeguro.SetSenderHash();

        var expMonth = self.InfoCartao().MesValidade();
        var expYear = self.InfoCartao().AnoValidade();

        var param = {

            cardNumber: self.InfoCartao().Numero(),
            cvv: self.InfoCartao().CodigoSeguranca(),
            expirationMonth: self.InfoCartao().MesValidade(),
            expirationYear: self.InfoCartao().AnoValidade(),

            success: function (response) {

                cardToken = response.card.token;

                var infoCartaoParams = self.InfoCartao().Params();

                var infoPagadorParams = self.InfoPagador().Params();

                var infoPagSeguroParams = {
                    SessionId: _meuPagSeguro.SessionId,
                    SenderHash: _meuPagSeguro.SenderHash,
                    CardToken: cardToken
                }

                $.ajax({
                    url: '/pagseguro/Pagar',
                    data: JSON.stringify({ infoCartao: infoCartaoParams, infoPagador: infoPagadorParams, infoPagSeguro: infoPagSeguroParams }),
                    contentType: "application/json; charset=ISO-8859-1",
                    type: 'POST',
                    dataType: 'JSON',
                    async: true,
                    success: function (data) {

                        if (data.error)
                        {
                            _processamento.DirecionarParaPaginaDeErro(data.errors)
                        }
                        else
                        {
                            _processamento.DirecionarParaPaginaOk();
                        }
                        

                    },
                    error: function (jqXHR, textoStatus, errorThrown) {

                        _processamento.MensagemDeErro(errorThrown);

                    },
                    complete: function (response) {

                        _processamento.OcultarTelaDeEspera();

                    }

                });

            },
            error: function (response) {

                _processamento.OcultarTelaDeEspera();

                if (response.error) {

                    $.each(response.errors, function (idx, error) {

                        var codErro = idx;
                        var msgErro = error;

                        if (codErro == "10000")
                            msgErro = "Não foi possível identificar a bandeira do cartão. Informe outro número de cartão.";

                        _processamento.MensagemDeErro(msgErro);

                    });


                };

            }
        }

        PagSeguroDirectPayment.createCardToken(param);
    };

    self.ProcessarPagamento = function () {


        var infoCartaoParams = self.InfoCartao().Params();

        var infoPagadorParams = self.InfoPagador().Params();

        $.ajax({
            url: '/pagseguro/validar',
            data: JSON.stringify({ infoCartaoCredito: infoCartaoParams, infoPagador: infoPagadorParams }),
            contentType: "application/json; charset=ISO-8859-1",
            type: 'POST',
            dataType: 'json',
            async: true,
            success: function (data) {

                if (data.error) {
                    _processamento.DirecionarParaPaginaDeErro(data.errors)
                }
                else {
                    self.Pagar();
                }

            },
            error: function (jqXHR, textoStatus, errorThrown) {

                _processamento.MensagemDeErro(errorThrown);

            }
        });

    };

    self.SetBrand = function (value) {

        PagSeguroDirectPayment.getBrand({

            cardBin: value,

            success: function (response) {
                var brand = response.brand;
                self.UrlBrand("https://stc.pagseguro.uol.com.br/public/img/payment-methods-flags/42x20/" + brand.name + ".png?_ga=2.144436469.1965432112.1525085866-592187741.1450109346");
                self.InfoCartao().Brand(value);
                self.InfoCartao().CVVSize(brand.cvvSize);
            },
            error: function (response) {
                
                self.UrlBrand("");
                self.InfoCartao().Brand("");
                self.InfoCartao().CVVSize("0");
                _processamento.MensagemDeErro("Não foi possível identificar a bandeira do seu cartão.");
                
            },
            complete: function (response) {
                //tratamento comum para todas chamadas
            }
        });

        
            
    };


}

var _meuPagSeguro = null;
var _processamento = null;

$(function () {

    ko.bindingHandlers.masked = {
        init: function (element, valueAccessor, allBindingsAccessor) {

            var mask = allBindingsAccessor().mask || {};

            $(element).mask(mask);

            ko.utils.registerEventHandler(element, 'focusout', function () {
                var observable = valueAccessor();
                observable($(element).val());
            });
        },
        update: function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            $(element).val(value);
        }
    };

    ko.components.register('endereco-component', {
        viewModel: function (params) {

            this.infoPagador = params.value;

        },
        template:

            '<div class="form-group">\
            <label for="inputCEP" class="col-sm-2 control-label">CEP</label>\
            <div class="col-sm-4">\
                <input type="text" class="form-control" id="inputCEP" maxlength="9" data-bind="masked: infoPagador().CEP, mask: \'99999-999\',  textInput: infoPagador().CEP">\
            </div>\
        </div>\
        <div data-bind="visible: infoPagador().PesquisouCEP()">\
            <div class="form-group">\
                <label for="inputEndereco" class="col-sm-2 control-label">Endereço</label>\
                <div class="col-sm-4">\
                    <input type="text" class="form-control" id="inputEndereco" data-bind="value: infoPagador().Endereco">\
                </div>\
            </div>\
            <div class="form-group">\
                <label for="inputBairro" class="col-sm-2 control-label">Bairro</label>\
                <div class="col-sm-4">\
                    <input type="text" class="form-control" id="inputBairro" data-bind="value: infoPagador().Bairro">\
                </div>\
            </div>\
            <div class="form-group">\
                <label for="inputNumero" class="col-sm-2 control-label">Número</label>\
                <div class="col-sm-4">\
                    <input type="text" class="form-control" id="inputNumero" data-bind="value: infoPagador().NumeroCasa">\
                </div>\
            </div>\
            <div class="form-group">\
                <label for="inputComplemento" class="col-sm-2 control-label">Complemento</label>\
                <div class="col-sm-4">\
                    <input type="text" class="form-control" id="inputComplemento" data-bind="value: infoPagador().Complemento">\
                </div>\
            </div>\
            <div data-bind="visible: infoPagador().PesquisouCEP() && !infoPagador().CEPOk()">\
                <div class="form-group">\
                    <label for="inputUF" class="col-sm-2 control-label">UF</label>\
                    <div class="col-sm-4">\
                        <input type="text" class="form-control" id="inputUF" maxlength="2" data-bind="value: infoPagador().UF">\
                    </div>\
                </div>\
                <div class="form-group">\
                    <label for="inputCidade" class="col-sm-2 control-label">Cidade</label>\
                    <div class="col-sm-4">\
                        <input type="text" class="form-control" id="inputCidade" data-bind="value: infoPagador().Cidade">\
                    </div>\
                </div>\
            </div>\
        </div>'

    });

    ko.extenders.uppercase = function (target, option) {
        target.subscribe(function (newValue) {
            target(newValue.toUpperCase());
        });
        return target;
    };

    _processamento = new Processamento();

    _meuPagSeguro = new MeuPagSeguro();

    _meuPagSeguro.SetSessionId();

    var infoCartao = new InfoCartao("", "", "", "");

    var infoPagador = new InfoPagador("", "", "", "", "", "", "", "");

    window.PagamentoViaCartao = new CartaoCredito(infoCartao, infoPagador);

    ko.applyBindings(window.PagamentoViaCartao, document.getElementById("cartao"));

    window.PagamentoViaBoleto = new Boleto(infoPagador);

    ko.applyBindings(window.PagamentoViaBoleto, document.getElementById("boleto"));

});

