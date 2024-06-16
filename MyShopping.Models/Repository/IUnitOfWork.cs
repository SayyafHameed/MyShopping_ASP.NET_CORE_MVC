using MyShoping.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShoping.Entities.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository CategoryRepos { get; }

        IProductRepository ProductRepos { get; }

        IShoppingCartRepository ShoppingCart { get; }

        IOrderHeaderRepository OrderHeaderRepos { get; }

        IOrderDetailRepository OrderDetailRepos { get; }

        IAppUserRepository AppUserRepos { get; }

        void Complete();
    }
}
