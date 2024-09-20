using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ssd_authorization_solution.DTOs;
using ssd_authorization_solution.Entities;

namespace MyApp.Namespace;

[Route("api/[controller]")]
[ApiController]
public class ArticleController : ControllerBase
{
    private readonly AppDbContext ctx;

    public ArticleController(AppDbContext ctx)
    {
        this.ctx = ctx;
    }

    // Requires no authentication policy, as anyone can read an article
    [HttpGet]
    public IEnumerable<ArticleDto> Get()
    {
        return ctx.Articles.Include(x => x.Author).Select(ArticleDto.FromEntity);
    }

    // Requires no authentication policy, as anyone can read an article
    [HttpGet(":id")]
    public ArticleDto? GetById(int id)
    {
        return ctx
            .Articles.Include(x => x.Author)
            .Where(x => x.Id == id)
            .Select(ArticleDto.FromEntity)
            .SingleOrDefault();
    }

    [Authorize(Policy = "CanCreateEditOwnArticles")]
    [HttpPost]
    public ArticleDto Post([FromBody] ArticleFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var author = ctx.Users.Single(x => x.UserName == userName);
        var entity = new Article
        {
            Title = dto.Title,
            Content = dto.Content,
            Author = author,
            CreatedAt = DateTime.Now
        };
        var created = ctx.Articles.Add(entity).Entity;
        ctx.SaveChanges();
        return ArticleDto.FromEntity(created);
    }

    [Authorize(Policy = "CanCreateEditOwnArticles")]
    [HttpPut(":id")]
    public ArticleDto Put(int id, [FromBody] ArticleFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var entity = ctx
            .Articles
            .Include(x => x.Author)
            .Single(x => x.Id == id);
        entity.Title = dto.Title;
        entity.Content = dto.Content;
        var updated = ctx.Articles.Update(entity).Entity;
        ctx.SaveChanges();
        return ArticleDto.FromEntity(updated);
    }
    
    [Authorize(Policy = "CanEditDeleteArticles")]
    [HttpDelete(":id")]
    public IActionResult Delete(int id)
    {
        var entity = ctx.Articles.Single(x => x.Id == id);
        ctx.Articles.Remove(entity);
        ctx.SaveChanges();
        return NoContent();
    }
}