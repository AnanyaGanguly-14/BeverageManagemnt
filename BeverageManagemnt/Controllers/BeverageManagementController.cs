using BeverageManagemnt.BusinessLayer;
using BeverageManagemnt.Exception;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace BeverageManagemnt.Controllers
{
    [Route("api/beveragemanagemnt")]
    [ApiController]
    public class BeverageManagementController : ControllerBase
    {

        private readonly BeveragesBL _beveragesBL;

        public BeverageManagementController(BeveragesBL beveragesBL)
        {
           
            _beveragesBL = beveragesBL;
        }

        #region Beverage Category
        [TypeFilter(typeof(AdminOnlyFilter))]
        [HttpGet]
        [Route("beveragecategory")]
        public async Task<IList<BeverageCategory>> GetBeveragesList()
        {
          return await _beveragesBL.GetBeverages();
        }

        [TypeFilter(typeof(AdminOnlyFilter))]
        [HttpPost]
        [Route("addbeverages")]

        public async Task<IList<BeverageCategory>> AddBeverages([FromBody] BeverageCategory beverageCategory)
        {
            return await _beveragesBL.AddBeverages(beverageCategory);
        }

        [TypeFilter(typeof(AdminOnlyFilter))]
        [HttpPut]
        [Route("editbeverages")]
        public async Task<IList<BeverageCategory>> EditBeverages([FromBody] BeverageCategory beverageCategory)
        {
            return await _beveragesBL.EditBeverages(beverageCategory);
        }
        [TypeFilter(typeof(AdminOnlyFilter))]
        [HttpDelete]
        [Route("deletebeverages")]
        public async Task<IList<BeverageCategory>> DeleteBeverages([FromBody] BeverageCategory beverageCategory)
        {
            return await _beveragesBL.DeleteBeverages(beverageCategory);
        }

        #endregion

        #region Beverage Details
        [HttpGet]
        [Route("beveragedetails")]
        public async Task<IList<BeverageDetails>> GetBeveragesDetailsList()
        {
            return await _beveragesBL.GetBeverageDetails();
        }

        [HttpPost]
        [Route("addbeverageDetails")]

        public async Task<IList<BeverageDetails>> AddBeverageDetails([FromBody] BeverageDetails beverageDetails)
        {
            return await _beveragesBL.AddBeverageDetails(beverageDetails);
        }

        [HttpPut]
        [Route("editbeveragedetails")]
        public async Task<IList<BeverageDetails>> EditBeverageDetails([FromBody] BeverageDetails beverageDetails)
        {
            return await _beveragesBL.EditBeverageDetails(beverageDetails);
        }
        [HttpDelete]
        [Route("deletebeveragedetails")]
        public async Task<IList<BeverageDetails>> DeleteBeverageDetails([FromBody] BeverageDetails beverageDetails)
        {
            return await _beveragesBL.DeleteBeverageDetails(beverageDetails);
        }
        #endregion
    }
}
