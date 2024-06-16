using MyShoping.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShoping.Entities.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> shoppingCarts {  get; set; }

        public double TotalCarts { get; set; }

        public OrderHeader OrderHeader { get; set; }
    }
}
