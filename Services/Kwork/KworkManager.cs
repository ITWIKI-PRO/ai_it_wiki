using ai_it_wiki.Data;
using ai_it_wiki.Services.Kwork.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kwork
{
  public class KworkManager
  {
    ApplicationDbContext _dbContext;
    public KworkManager()
    {

    }

    //получение списка предложений из базы данных
    public async Task<List<Proposal>> GetProposals()
    {
      _dbContext ??= new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      return await _dbContext.Proposals
          .Include(p => p.User)
          // .ThenInclude(u => u.Achievements)
          .ToListAsync();
    }

    internal async Task<int> UpdateProposals(List<Proposal> proposals)
    {
      _dbContext ??= new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());

      foreach (var proposal in proposals)
      {
        var existingProposal = await _dbContext.Proposals
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == proposal.Id);

        if (existingProposal != null)
        {
          _dbContext.Entry(existingProposal).CurrentValues.SetValues(proposal);

          var existingUser = await _dbContext.KworkUsers
              .FirstOrDefaultAsync(u => u.Id == proposal.UserId);

          if (existingUser == null)
          {
            new KworkUser() { Id = proposal.UserId, Name = proposal.Username };
            await _dbContext.KworkUsers.AddAsync(existingUser);
            await _dbContext.SaveChangesAsync();
          }
        }
        else
        {
          var existingUser = await _dbContext.KworkUsers
              .FirstOrDefaultAsync(u => u.Id == proposal.UserId);

          if (existingUser == null)
          {
            KworkUser kworkUser = new KworkUser() { Id = proposal.UserId, Name = proposal.Username };
            await _dbContext.KworkUsers.AddAsync(kworkUser);
          }
          else
          {
            existingUser.Name = proposal.Username;

            await _dbContext.SaveChangesAsync();
          }

          await _dbContext.Proposals.AddAsync(proposal);
        }
      }

      var changes = await _dbContext.SaveChangesAsync();
      return changes;
    }
  }


}