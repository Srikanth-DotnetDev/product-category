using AprilPractice.Domain.Entities;
using AprilPractice.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AprilPractice.Web.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Category> Categories { get; set; } = new List<Category>();

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();
    }
}
