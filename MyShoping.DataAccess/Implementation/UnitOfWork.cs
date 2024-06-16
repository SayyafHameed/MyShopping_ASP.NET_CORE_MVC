using MyShoping.Entities.Repository;
using MyShoping.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShoping.DataAccess.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public ICategoryRepository CategoryRepos { get; private set; }

        public IProductRepository ProductRepos { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IOrderHeaderRepository OrderHeaderRepos { get; private set; }

        public IOrderDetailRepository OrderDetailRepos { get; private set; }

        public IAppUserRepository AppUserRepos { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            CategoryRepos = new CategoryRepository(context);
            ProductRepos = new ProductRepository(context);
            ShoppingCart = new ShoppingCartRepository(context);
            OrderHeaderRepos = new OrderHeaderRepository(context);
            OrderDetailRepos = new OrderDetailRepository(context);
            AppUserRepos = new AppUserRepository(context);
        }

        public void Complete()
        {
           _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
