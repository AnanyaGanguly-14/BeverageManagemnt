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

        [HttpGet]
        public async Task<IList<BeverageCategory>> GetBeveragesList()
        {
          return await _beveragesBL.GetBeverages();
        }

        [HttpPost]
        [Route("addbeverages")]

        public async Task<IList<BeverageCategory>> AddBeverages([FromBody] BeverageCategory beverageCategory)
        {
            return await _beveragesBL.AddBeverages(beverageCategory);
        }


        [HttpPut]
        [Route("editbeverages")]
        public async Task<IList<BeverageCategory>> EditBeverages([FromBody] BeverageCategory beverageCategory)
        {
            return await _beveragesBL.EditBeverages(beverageCategory);
        }

        [HttpDelete]
        [Route("deletebeverages")]
        public async Task<IList<BeverageCategory>> DeleteBeverages([FromBody] BeverageCategory beverageCategory)
        {
            return await _beveragesBL.DeleteBeverages(beverageCategory);
        }
    }
}
