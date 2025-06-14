using BeverageManagemnt.Exception;
using DalLayer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Model;
using System.Linq.Expressions;

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
            catch (BeverageServiceException ex)
            {
                throw new BeverageServiceException(null);
            }
            return beverages;
        }

        public async Task<IList<BeverageCategory>> AddBeverages(BeverageCategory beverageCategory)
        {
            IList<BeverageCategory> result = new List<BeverageCategory>();
            if (beverageCategory != null)
            {
                try
                {
                    if (!IsValidBeverageType(beverageCategory.BEVERAGE_TYPE))
                    {
                        throw new BeverageServiceException("Err_004");
                    }

                    await _context.BeverageCategories.AddAsync(beverageCategory);
                    await _context.SaveChangesAsync();

                    return await _context.BeverageCategories.ToListAsync();
                }

                catch (BeverageServiceException ex)
                {
                    return ExceptionDetails(beverageCategory, result, ex, null);
                }
                catch (SqlException ex)
                {

                    return ExceptionDetails(beverageCategory, result, null, ex);
                }
            }

            else
            {
                throw new BeverageServiceException(null);
            }
        }

        public static bool IsValidBeverageType(string beverageType)
        {

            if (string.IsNullOrWhiteSpace(beverageType) || beverageType.Trim().ToLower() == "string" ||
                beverageType.StartsWith(" ") || beverageType.EndsWith(" "))
                { 
                  return false;
                }

            // Regex: only letters and single spaces between words
            var pattern = @"^[A-Za-z]+( [A-Za-z]+)*$";
            return System.Text.RegularExpressions.Regex.IsMatch(beverageType, pattern);
        }
        private static IList<BeverageCategory> ExceptionDetails(BeverageCategory beverageCategory, IList<BeverageCategory> result
                                    , BeverageServiceException ex, SqlException sqlException)
        {
            if (ex != null)
            {
                beverageCategory.ExceptionDetails = new ExceptionDetails
                {
                    Code = ex.ErrorCode,
                    Message = ex.Message
                };
                result.Add(beverageCategory);
            }
            else
            {

                if (sqlException != null)

                {
                    var sqlExceptionRes = new BeverageServiceException("Err_003", ex);
                    beverageCategory.ExceptionDetails = new ExceptionDetails
                    {
                        Code = sqlExceptionRes.Message
                    };
                    result.Add(beverageCategory);
               
                }
            }

            return result;
        }

        public async Task<IList<BeverageCategory>> EditBeverages(BeverageCategory beverageCategory)
        {
            IList<BeverageCategory> result = new List<BeverageCategory>();
            if (beverageCategory != null)
            {
                try
                {
                    if (!IsValidBeverageType(beverageCategory.BEVERAGE_TYPE))
                    {
                        throw new BeverageServiceException("Err_004");
                    }
                    var existingBeverage = await _context.BeverageCategories.FindAsync(beverageCategory.BEVERAGE_CATEGORY_ID);
                    if (existingBeverage == null)
                    {
                        throw new BeverageServiceException("Err_001");
                    }

                    existingBeverage.BEVERAGE_TYPE = beverageCategory.BEVERAGE_TYPE;
                    _context.BeverageCategories.Update(existingBeverage);
                    await _context.SaveChangesAsync();

                    return await _context.BeverageCategories.ToListAsync();
                }
                catch(BeverageServiceException ex)
                {
                    return ExceptionDetails(beverageCategory, result, ex, null);
                }
                catch (SqlException ex)
                {
                    return ExceptionDetails(beverageCategory, result, null, ex);
                }
            }
            else
            {
                throw new BeverageServiceException(null);
            }
        }

        public async Task<IList<BeverageCategory>> DeleteBeverages(BeverageCategory beverageCategory)
        {
            IList<BeverageCategory> result = new List<BeverageCategory>();
            if (beverageCategory != null)
            {
                try
                {
                    var existingBeverage = await _context.BeverageCategories.FindAsync(beverageCategory.BEVERAGE_CATEGORY_ID);
                    if (existingBeverage == null)
                    {
                        throw new BeverageServiceException("Err_005");
                    }
                    _context.BeverageCategories.Remove(existingBeverage);
                    await _context.SaveChangesAsync();
                    return await _context.BeverageCategories.ToListAsync();
                }
                catch (BeverageServiceException ex)
                {
                    return ExceptionDetails(beverageCategory, result, ex, null);
                }
                catch (SqlException ex)
                {
                    return ExceptionDetails(beverageCategory, result, null, ex);
                }
            }
            else
            {
                throw new BeverageServiceException(null);
            }
            }
        }
    }

