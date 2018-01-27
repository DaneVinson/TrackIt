using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Dto;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Service.CoreWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class DataPointsController : Controller
    {
        public DataPointsController(IDataPointManager manager)
        {
            Manager = manager;
        }

        #region REST

        [HttpPost]
        public async Task<IActionResult> CreateDataPointsAsync([FromBody]IEnumerable<DataPoint> dataPoints)
        {
            return await UpsertDataPointsAsync(dataPoints);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteDataPointAsync(string id)
        {
            var result = await Manager.DeleteAsync(new Criteria<IEnumerable<string>>() { Value = new string[] { id } });
            if (result.Success) { return Ok(result.Value); }
            else { return BadRequest(result); }
        }

        [HttpPut]
        public async Task<IActionResult> UpsertDataPointsAsync([FromBody]IEnumerable<DataPoint> dataPoints)
        {
            var result = await Manager.UpsertAsync(new Criteria<IEnumerable<DataPoint>>() { Value = dataPoints });
            if (result.Success) { return Ok(result.Value); }
            else { return BadRequest(result); }
        }

        #endregion

        private readonly IDataPointManager Manager;
    }
}
