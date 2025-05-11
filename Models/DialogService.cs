using ai_it_wiki.Data;

using Microsoft.EntityFrameworkCore;

namespace ai_it_wiki.Models
{
  public class DialogService
  {
    private readonly ApplicationDbContext _context;

    public DialogService(ApplicationDbContext context)
    {
      _context = context;
    }

    public List<Dialog> GetDialogsForUser(string userId)
    {
      return _context.Dialogs
                     .Include(d => d.Messages)
                     .Include(d => d.Settings)
                     .Where(d => d.UserId == userId)
                     .ToList();
    }

    public List<Dialog> GetDialogsForUser(User user)
    {
      return _context.Dialogs
                     .Include(d => d.Messages)
                     .Include(d => d.Settings)
                     .Where(d => d.UserId == user.UserId)
                     .ToList();
    }

    public Dialog GetDialogById(string dialogId)
    {
      return _context.Dialogs
                     .Include(d => d.Messages)
                     .Include(d => d.Settings)
                     .SingleOrDefault(d => d.DialogId == dialogId);
    }

    public void SaveDialog(Dialog dialog)
    {
      if (_context.Dialogs.Any(d => d.DialogId == dialog.DialogId))
      {
        _context.Dialogs.Update(dialog);
      }
      else
      {
        _context.Dialogs.Add(dialog);
      }
      _context.SaveChanges();
    }

    public void DeleteDialog(string dialogId)
    {
      var dialog = _context.Dialogs.SingleOrDefault(d => d.DialogId == dialogId);
      if (dialog != null)
      {
        _context.Dialogs.Remove(dialog);
        _context.SaveChanges();
      }
    }
  }
}
