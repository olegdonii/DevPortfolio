using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.Models;

namespace Server.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("api/[controller]")]
    [ApiController]    
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostsController(AppDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET api/posts
        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            List<Post> posts = await _db.Posts
                .Include(post => post.Category)
                .ToListAsync();
            return Ok(posts);
        }

        // GET api/posts/1
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Post post = await GetPostByPostId(id);

            return Ok(post);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Post postToCreate)
        {
            try
            {
                if (postToCreate == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (postToCreate.Published)
                {
                    //EU DateTime
                    postToCreate.PublishDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");
                }

                await _db.Posts.AddAsync(postToCreate);

                bool changesPersistedToDatabase = await PersistChangesToDatabase();

                if (changesPersistedToDatabase == false)
                {
                    return StatusCode(500, "Something went wrong on our side. Please contact the administrator.");
                }
                else
                {
                    return Created("Create", postToCreate);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Something went wrong on our side. Please contact the administrator. Error message: {e.Message}.");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] Post updatedPost)
        {
            try
            {
                if (id < 1
                || updatedPost == null
                || !ModelState.IsValid
                || id != updatedPost.PostId)
                {
                    return BadRequest(ModelState);
                }

                Post oldPost = await _db.Posts.FindAsync(id);
                if (oldPost == null)
                {
                    return NotFound();
                }

                if (!oldPost.Published
                    && updatedPost.Published)
                {
                    updatedPost.PublishDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");
                }

                // Detach oldPost from EF, else if can't be updated.
                _db.Entry(oldPost).State = EntityState.Detached;

                _db.Posts.Update(updatedPost);

                bool changesPersistedToDatabase = await PersistChangesToDatabase();

                if (!changesPersistedToDatabase)
                {
                    return StatusCode(500, "Something went wrong on our side. Please contact the administrator.");
                }
                else
                {
                    return Created("Created", updatedPost);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Something went wrong on our side. Please contact the administrator. Error message: {e.Message}.");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id < 1)
                {
                    return BadRequest(ModelState);
                }

                bool exists = await _db.Posts.AnyAsync(post => post.PostId == id);

                if (!exists)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Post postToDelete = await GetPostByPostId(id);

                if (postToDelete.ThumbnailImagePath != "uploads/placeholder.jpg")
                {
                    string fileName = postToDelete.ThumbnailImagePath.Split('/').Last();

                    System.IO.File.Delete($"{_webHostEnvironment.ContentRootPath}\\wwwroot\\uploads\\{fileName}");
                }

                _db.Posts.Remove(postToDelete);

                bool changesPersistedToDatabase = await PersistChangesToDatabase();

                if (!changesPersistedToDatabase)
                {
                    return StatusCode(500, "Something went wrong on our side. Please contact the administrator.");
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception e)
            {
                
                return StatusCode(500, $"Something went wrong on our side. Please contact the administrator. Error message: {e.Message}.");
            }
        }

        #region Utility methods

        [NonAction]
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<bool> PersistChangesToDatabase()
        {
            int amountOfChanges = await _db.SaveChangesAsync();

            return amountOfChanges > 0;
        }

        [NonAction]
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<Post> GetPostByPostId(int postId)
        {
            Post postToGet = await _db.Posts
                    .Include(post => post.Category)
                    .FirstAsync(post => post.PostId == postId);

            return postToGet;
        }

        #endregion
    }
}