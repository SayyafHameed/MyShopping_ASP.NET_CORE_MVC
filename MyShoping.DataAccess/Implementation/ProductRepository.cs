﻿using MyShoping.Entities.Models;
using MyShoping.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShoping.DataAccess.Implementation
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var ProductInDb = _context.Products.FirstOrDefault(x => x.Id == product.Id);
            if (ProductInDb != null)
            {
                ProductInDb.Name = product.Name;
                ProductInDb.Description = product.Description;
                ProductInDb.Price = product.Price;
                ProductInDb.Image = product.Image;
                ProductInDb.CategoryId = product.CategoryId;
            }
        }
    }
}
