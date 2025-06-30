using Azure.Storage.Blobs;
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

        private readonly string _connectionString;
        private readonly string _containerName;


        public BeverageManagementController(BeveragesBL beveragesBL, IConfiguration configuration)
        {
           
            _beveragesBL = beveragesBL;

            _connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"];

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

        #region Order Placement
        [HttpPost]
        [Route("orderplacement")]
        public async Task<string> PlaceOrders(Orders orders)
        {
            return await _beveragesBL.PlaceOrders(orders);
        }


        #endregion

        #region FileUpload

        [HttpPost("file")]

        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

        

            Directory.CreateDirectory("Uploads");


            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Create the container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }


            return Ok(new { file.FileName, file.Length });


        }
        #endregion
    }
    }
