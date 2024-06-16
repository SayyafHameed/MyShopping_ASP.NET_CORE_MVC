using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShoping.Entities.Models;

namespace MyShoping.Models.Repository
{
    public interface IProductRepository: IGenericRepository<Product>
    {
        void Update(Product product);
    }
}
