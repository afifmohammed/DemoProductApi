using Microsoft.EntityFrameworkCore;
using WebApp1.Models;

namespace WebApp1.Persistence;

public class ApiContext : DbContext
{
    public ApiContext(DbContextOptions<ApiContext> options) : base(options)
    {}
    public DbSet<Product> Products { get; set; }
}