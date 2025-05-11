using ai_it_wiki.Data;
using ai_it_wiki.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ai_it_wiki.Internal
{
  public class AuthService
  {
    private readonly ApplicationDbContext _context;
    private readonly IKeyService _keyService;

    public AuthService(ApplicationDbContext context, IKeyService keyService)
    {
      _context = context;
      _keyService = keyService;
    }

    public User Authenticate(string login, string password)
    {
      User user = _context.Users
         .Include(u => u.Dialogs)
         .ThenInclude(d => d.Settings)
         .Include(u => u.Dialogs)
         .ThenInclude(d => d.Messages)
         .FirstOrDefault(u => u.Username == login);

      if (user != null && user.PasswordHash == new PasswordHasher<string>().HashPassword(user.Username, password))
        return user;

      return null;
    }

    public User Register(string login, string password, string email = "")
    {
      User user = _context.Users
        .Include(u => u.Dialogs)
        .ThenInclude(d => d.Settings)
        .Include(u => u.Dialogs)
        .ThenInclude(d => d.Messages)
        .FirstOrDefault(u => u.Username == login);

      if (user != null) return user;

      user = new User()
      {
        Username = login,
        PasswordHash = new PasswordHasher<string>().HashPassword(login, password),
        Email = email,
        AccessKey = _keyService.GenerateKey()
      };

      _context.Users.Add(user);
      _context.SaveChanges();

      return user;
    }
  }
}
