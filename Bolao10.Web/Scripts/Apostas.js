

$(document).ready(
  function()
  {
      $("#form1").submit
      (
        function(e)
        {
            e.preventDefault();         
          
            var URL = "update";

            var formdata = $("#form1").serialize();

            $.ajax(
            {
                type: "POST",
                url: URL,
                data: formdata,
                dataType: "html",
                async: true,
                cache: false,
                beforeSend: function () {
              
                    $("#btnSalvar").val("Aguarde...");
                    $("#btnSalvar").attr("disabled", "disabled")
              
                },
                success: function (data) {
              
                    $("#results").empty();

                    if (data == "Timeout") {
                        var msg = "Suas apostas NÃO foram salvas, pois sessão expirou. Por favor, clique em sair e efetue o login novamente.";
                        $("#results").append(msg);
                    } else {                        
                        $("#results").append(data);
                    }
              
                },
                error: function (request, status, error) {
              
                    $("#results").empty();
                    $("#results").append(request.responseText);
              
                },
                complete: function(){

                    $("#btnSalvar").val("Salvar");
                    $("#btnSalvar").removeAttr("disabled");

                }
            });
          
        }
      )
  });