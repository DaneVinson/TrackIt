using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Dto;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;
using TrackIt.Service.CoreWebApi.Criteria;

namespace TrackIt.Service.CoreWebApi.Controllers
{
#if !DEBUG
    [Authorize]
#endif
    [Route("api/[controller]")]
    public class CategoriesController : BaseController
    {
        public CategoriesController(ICategoryManager manager)
        {
            Manager = manager;
        }

        #region REST

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCategoryAsync([FromBody]Category category)
        {
            return await UpsertCategoryAsync(category);
        }

        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCatagoryAsync(string id)
        {
            var result = await Manager.DeleteAsync(new Criteria<IEnumerable<string>>()
            {
                PrimaryId = GetIdClaimValue(),
                Value = new string[] { id }
            });
            if (result.Success) { return Ok(true); }
            else { return BadRequest(result); }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            var result = await Manager.GetAllAsync(new Criteria<EmptyModel>() { PrimaryId = GetIdClaimValue() });
            if (result.Success) { return Ok(result.Value); }
            else { return BadRequest(result); }
        }

        [Authorize]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCategoryAsync(string id, [FromQuery]string from = null, [FromQuery]string to = null)
        {
            DateTime testDate;
            var dateRange = new DateRange();
            if (DateTime.TryParse(from, out testDate)) { dateRange.From = testDate; }
            if (DateTime.TryParse(to, out testDate)) { dateRange.To = testDate; }
            var result = await Manager.GetAsync(new Criteria<DateRange>()
            {
                PrimaryId = id,
                Value = dateRange
            });
            if (result.Success) { return Ok(result.Value); }
            else { return BadRequest(result); }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpsertCategoryAsync([FromBody]Category category)
        {
            var result = await Manager.UpsertAsync(new Criteria<IEnumerable<Category>>()
            {
                PrimaryId = GetIdClaimValue(),
                Value = new Category[] { category }
            });
            if (result.Success) { return Ok(result.Value); }
            else { return BadRequest(result); }
        }

        #endregion

        private readonly ICategoryManager Manager;
    }
}
