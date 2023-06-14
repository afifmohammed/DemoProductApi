using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp1.Domain;
using WebApp1.Models;
using WebApp1.Persistence;

namespace WebApp1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Products : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly ProductCatalogue _catalogue;
        public Products(ApiContext context)
        {
            _context = context;
            _catalogue = new ProductCatalogue(_context);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _catalogue.ListProducts();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int? id)
        {
            if (!id.HasValue) return BadRequest();

            var product = await _catalogue.GetProductById(id.Value);

            if (product == null) return NotFound();

            return product;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int? id, Product product)
        {
            if (!id.HasValue) return BadRequest();
            
            var result = await _catalogue.Update(id.Value, product);

            if (!result.Success)
                return result.Error switch
                {
                    ProductCatalogue.ProductNotFound _ => NotFound(),
                    ProductCatalogue.CannotUpdateWhenIdMismatch _ => BadRequest(),
                    _ => BadRequest()
                };

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            var result = await _catalogue.Add(product);
            if (!result.Success)
                return BadRequest();
            
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (!id.HasValue) return BadRequest();
            
            var result = await _catalogue.Remove(id.Value);

            if (!result.Success)
                return result.Error switch
                {
                    ProductCatalogue.ProductNotFound _ => NotFound(),
                    _ => BadRequest()
                };
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
