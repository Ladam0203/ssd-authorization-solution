using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ssd_authorization_solution.DTOs;
using ssd_authorization_solution.Entities;

namespace MyApp.Namespace;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly AppDbContext ctx;

    public CommentController(AppDbContext ctx)
    {
        this.ctx = ctx;
    }

    [HttpGet]
    public IEnumerable<CommentDto> Get([FromQuery] int? articleId)
    {
        var query = ctx.Comments.Include(x => x.Author).AsQueryable();
        if (articleId.HasValue)
            query = query.Where(c => c.ArticleId == articleId);
        return query.Select(CommentDto.FromEntity);
    }

    [HttpGet(":id")]
    public CommentDto? GetById(int id)
    {
        return ctx
            .Comments.Include(x => x.Author)
            .Select(CommentDto.FromEntity)
            .SingleOrDefault(x => x.Id == id);
    }

    [HttpPost]
    public CommentDto Post([FromBody] CommentFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var author = ctx.Users.Single(x => x.UserName == userName);
        var article = ctx.Articles.Single(x => x.Id == dto.ArticleId);
        var entity = new Comment
        {
            Content = dto.Content,
            Article = article,
            Author = author,
        };
        var created = ctx.Comments.Add(entity).Entity;
        ctx.SaveChanges();
        return CommentDto.FromEntity(created);
    }

    [HttpPut(":id")]
    public CommentDto Put(int id, [FromBody] CommentFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var entity = ctx
            .Comments.Include(x => x.Author)
            .Where(x => x.Author.UserName == userName)
            .Single(x => x.Id == id);
        entity.Content = dto.Content;
        var updated = ctx.Comments.Update(entity).Entity;
        ctx.SaveChanges();
        return CommentDto.FromEntity(updated);
    }
}
