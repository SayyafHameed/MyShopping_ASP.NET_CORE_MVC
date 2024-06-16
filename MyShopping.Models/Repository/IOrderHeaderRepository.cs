using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShoping.Entities.Models;

namespace MyShoping.Models.Repository
{
    public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);

        void UpdateOrderStatus(int id,  string OrderStatus, string PaymentStatus);
    }
}
