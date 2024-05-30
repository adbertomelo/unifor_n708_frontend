using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bolao10.ViewModels
{
    public class CartaoCreditoModel
    {
        
        public Int64 Numero { get; set; }
        public string Validade { get; set; }
        public string NomeTitular { get; set; }
        public int CodigoSeguranca { get; set; }

    }
}