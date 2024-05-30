using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bolao10.ViewModels
{
    public class PagSeguroModel
    {
        public string SessionId { get; set; }
        public string SenderHash { get; set; }
        public string CardToken { get; set; }
    }
}