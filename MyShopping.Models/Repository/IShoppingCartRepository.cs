using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShoping.Entities.Models;

namespace MyShoping.Models.Repository
{
    public interface IShoppingCartRepository: IGenericRepository<ShoppingCart>
    {
        int IncreaseCount(ShoppingCart shoppingCart, int count);

        int DecreaseCount(ShoppingCart shoppingCart, int count);
    }
}
