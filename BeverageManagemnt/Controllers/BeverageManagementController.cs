using BeverageManagemnt.BusinessLayer;
using BeverageManagemnt.Exception;
using DalLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        //[HttpPost]
       // [Route("add")] 
        //public async Task<IList<BeverageCategory>> AddProducts([FromBody] BeverageCategory)
       // {
        //   
        //}
    }
}
