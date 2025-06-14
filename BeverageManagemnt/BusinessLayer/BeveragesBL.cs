using BeverageManagemnt.Exception;
using DalLayer;
using Microsoft.EntityFrameworkCore;
using Model;

namespace BeverageManagemnt.BusinessLayer
{
    public class BeveragesBL
    {

        private readonly AppDbContext _context;
        public BeveragesBL(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IList<BeverageCategory>> GetBeverages()
        {
            List<BeverageCategory> beverages;
            try
            {
                beverages = await _context.BeverageCategories.ToListAsync();
            }
            catch (BeverageException ex)
            {
                throw new BeverageException(null);
            }
            return beverages;
        }
    }
}
