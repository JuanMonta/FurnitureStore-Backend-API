using API.FornitureStore.Data;
using API.FornitureStore.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.FornitureStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {

        private readonly APIFornitureStoreContext _context;

        public ProductCategoriesController(APIFornitureStoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<ProductCategory>> Get()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ProductCategory category)
        {
            await _context.ProductCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction("Post", category.Id, category);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(ProductCategory category)
        {
            if (category == null) NotFound();
            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
