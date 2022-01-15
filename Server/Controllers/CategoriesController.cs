using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        [DisableCors]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            List<Category> categories = await _db.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}