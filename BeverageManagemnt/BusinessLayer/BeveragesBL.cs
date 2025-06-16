using BeverageManagemnt.Exception;
using BeverageManagemnt.Interface;
using Common;
using DalLayer;
using Microsoft.Data.SqlClient;
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
        #region Beverage Category
        public async Task<IList<BeverageCategory>> GetBeverages()
        {
            List<BeverageCategory> beverages;
            try
            {
                beverages = await _context.BeverageCategories.ToListAsync();
            }
            catch (BeverageServiceException ex)
            {
                throw new BeverageServiceException(ex.Message);
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
                    if (!IsValidBeverageTypeandCategory(beverageCategory.BEVERAGE_TYPE))
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


        public async Task<IList<BeverageCategory>> EditBeverages(BeverageCategory beverageCategory)
        {
            IList<BeverageCategory> result = new List<BeverageCategory>();
            if (beverageCategory != null)
            {
                try
                {
                    if (!IsValidBeverageTypeandCategory(beverageCategory.BEVERAGE_TYPE))
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
        #endregion

        #region Beverage Details
        public async Task<IList<BeverageDetails>> GetBeverageDetails()
        {
            List<BeverageDetails> beverageDetails;
            try
            {

                beverageDetails = await(from details in _context.BeverageDetails
                                           join category in _context.BeverageCategories
                                           on details.BEVERAGE_CATEGORY_ID equals category.BEVERAGE_CATEGORY_ID
                                           select new BeverageDetails
                                           {
                                               BEVERAGE_DETAILS_ID = details.BEVERAGE_DETAILS_ID,
                                               BEVERAGE_SIZE = details.BEVERAGE_SIZE,
                                               BEVERAGE_PRICE = details.BEVERAGE_PRICE,
                                               BEVERAGE_CATEGORY_ID = details.BEVERAGE_CATEGORY_ID,
                                               beverageCategory = category
                                           }).ToListAsync();
            }
            catch (BeverageServiceException ex)
            {
                throw new BeverageServiceException(null);
            }
            return beverageDetails;
        }

        public async Task<IList<BeverageDetails>> AddBeverageDetails(BeverageDetails beverageDetails)
        {
            List<BeverageDetails> beverageDetailsResult = new List<BeverageDetails>();
            if (beverageDetails != null)
            {
                try
                {
                    if (!IsValidBeverageTypeandCategory(beverageDetails.BEVERAGE_SIZE))
                    {
                        throw new BeverageServiceException("Err_004");
                    }

                    var findCategory = await _context.BeverageCategories.FindAsync(beverageDetails.BEVERAGE_CATEGORY_ID);
                    if (findCategory == null)
                    {
                        throw new BeverageServiceException("Err_005");
                    }

   
                    await _context.BeverageDetails.AddAsync(beverageDetails);
                    await _context.SaveChangesAsync();
                    return await _context.BeverageDetails.ToListAsync();
                }

                catch (BeverageServiceException ex)
                {
                    return ExceptionDetails(beverageDetails, beverageDetailsResult, ex, null);
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
                {
                    if ( sqlEx.Message.Contains("UQ_BEVERAGE_CATEGORY_SIZE"))
                    {
                        var duplicateEx = new BeverageServiceException("Err_DUPLICATE");
                        return ExceptionDetails(beverageDetails, beverageDetailsResult, duplicateEx, sqlEx);
                    }

                    return ExceptionDetails(beverageDetails, beverageDetailsResult, null, sqlEx);
                }
            }
            else
            {
                throw new BeverageServiceException(null);
            }
        }

        public async Task<IList<BeverageDetails>> EditBeverageDetails(BeverageDetails beverageDetails)
        {
            IList<BeverageDetails> result = new List<BeverageDetails>();
            if (beverageDetails != null)
            {
                try
                {
                    if (!IsValidBeverageTypeandCategory(beverageDetails.BEVERAGE_SIZE))
                    {
                        throw new BeverageServiceException("Err_004");
                    }
                    var existingBeverage = await _context.BeverageDetails.FindAsync(beverageDetails.BEVERAGE_DETAILS_ID);
                    if (existingBeverage == null)
                    {
                        throw new BeverageServiceException("Err_001");
                    }

                    existingBeverage.BEVERAGE_SIZE = beverageDetails.BEVERAGE_SIZE;
                    existingBeverage.BEVERAGE_PRICE = beverageDetails.BEVERAGE_PRICE;
                    _context.BeverageDetails.Update(existingBeverage);
                    await _context.SaveChangesAsync();

                    return await _context.BeverageDetails.ToListAsync();
                }
                catch (BeverageServiceException ex)
                {
                    return ExceptionDetails(beverageDetails, result, ex, null);
                }
                catch (SqlException ex)
                {
                    return ExceptionDetails(beverageDetails, result, null, ex);
                }
            }
            else
            {
                throw new BeverageServiceException(null);
            }
        }

        public async Task<IList<BeverageDetails>> DeleteBeverageDetails(BeverageDetails beverageDetails)
        {
            IList<BeverageDetails> result = new List<BeverageDetails>();
            if (beverageDetails != null)
            {
                try
                {
                    var existingBeverage = await _context.BeverageDetails.FindAsync(beverageDetails.BEVERAGE_DETAILS_ID);
                    if (existingBeverage == null)
                    {
                        throw new BeverageServiceException("Err_005");
                    }
                    _context.BeverageDetails.Remove(existingBeverage);
                    await _context.SaveChangesAsync();
                    return await _context.BeverageDetails.ToListAsync();
                }
                catch (BeverageServiceException ex)
                {
                    return ExceptionDetails(beverageDetails, result, ex, null);
                }
                catch (SqlException ex)
                {
                    return ExceptionDetails(beverageDetails, result, null, ex);
                }
            }
            else
            {
                throw new BeverageServiceException(null);
            }
        }
        #endregion

        #region Common Functions
        private static IList<T> ExceptionDetails<T>(T beverageCategory, IList<T> result
                           , BeverageServiceException? ex, SqlException? sqlException)
                where T : IHasExceptionDetails
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
                        Code = sqlExceptionRes.Message,
                        Message = ex.Message
                    };
                    result.Add(beverageCategory);

                }
            }

            return result;
        }


        public static bool IsValidBeverageTypeandCategory(string? beverage)
        {

            if (string.IsNullOrWhiteSpace(beverage) || beverage.Trim().ToLower() == "string" ||
                beverage.StartsWith(" ") || beverage.EndsWith(" "))
            {
                return false;
            }

            // Regex: only letters and single spaces between words
            var pattern = @"^[A-Za-z]+( [A-Za-z]+)*$";
            return System.Text.RegularExpressions.Regex.IsMatch(beverage, pattern);
        }
        #endregion


    }
}

