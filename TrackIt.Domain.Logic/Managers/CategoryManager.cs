using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Dto;
using TrackIt.Domain.Model.Extensions;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Domain.Logic.Managers
{
    public class CategoryManager : GraphManager<Category, DataPoint>, ICategoryManager
    {
        public CategoryManager(
            IRepository<Category> repository,
            IRepository<DataPoint> childRepository) : 
            base(repository, childRepository)
        { }


        public async Task<Result<EmptyModel>> DeleteAsync(Criteria<IEnumerable<string>> idsCriteria)
        {            
            // Validate criteria.
            if (idsCriteria?.Value == null || idsCriteria.Value.Count() == 0)
            {
                return new Result<EmptyModel>() { Message = "Invalid criteria" };
            }

            // Get the Ids for the account's current categories.
            var currentCategoryIds = (await GetAllAsync(new Criteria<EmptyModel>() { PrimaryId = idsCriteria.PrimaryId }))?
                                                .Value?
                                                .Select(d => d.Id)
                                                .ToArray();
            if (currentCategoryIds == null) { currentCategoryIds = new string[0]; }

            // Validate that the requested deletes exist.
            if (idsCriteria.Value.Any(m => !currentCategoryIds.Any(c => c == m)))
            {
                return new Result<EmptyModel>() { Message = "Invalid delete request." };
            }

            // Delete all requested.
            var tasks = new List<Task<bool>>();
            idsCriteria.Value
                        .ToList()
                        .ForEach(c => { tasks.Add(Repository.DeleteAsync(c)); });
            var deleted = (await Task.WhenAll(tasks))?.Where(d => d == true).ToArray();

            // Handle results.
            var result = new Result<EmptyModel>();
            if (deleted.Count() > 0)
            {
                result.Success = true;
                result.Message = $"Deleted {deleted.Count()} of {idsCriteria.Value.Count()} requested categories.";
            }
            else { result.Message = "Requested deletes were unsuccessful."; }
            return result;
        }

        /// <summary>
        /// Get a list of all <see cref="Category"/>  for the Account (no child DataPoint).
        /// </summary>
        public async Task<Result<Category[]>> GetAllAsync(Criteria<EmptyModel> accountIdCriteria)
        {
            // Validate criteria
            if (String.IsNullOrWhiteSpace(accountIdCriteria?.PrimaryId))
            {
                return new Result<Category[]>() { Message = "Invalid criteria" };
            }

            // Query all Category objects for the Account.
            var query = Repository.GetQueryable().Where(d => d.AccountId == accountIdCriteria.PrimaryId);
            var result = new Result<Category[]>();
            result.Value = (await Repository.GetAsync(query))?.OrderBy(c => c.Description).ToArray();

            // Handle results.
            if (result.Value != null)
            {
                result.Message = $"The account has {result.Value.Count()} categories.";
                result.Success = true;
            }
            else { result.Message = "The attempt to get the account's categories failed."; }

            return result;
        }

        public async Task<Result<Category>> GetAsync(Criteria<DateRange> dateRangeCriteria)
        {
            // Validate criteria.
            if (dateRangeCriteria == null) { return new Result<Category>() { Message = "Invalid criteria" }; }
            if (dateRangeCriteria.Value == null) { dateRangeCriteria.Value = new DateRange(); }
            if (dateRangeCriteria.Value.From > dateRangeCriteria.Value.To) { return new Result<Category>() { Message = "Invalid criteria" }; }

            // Fetch the Category.
            var category = await Repository.GetAsync(dateRangeCriteria.PrimaryId);
            if (category == null)
            {
                return new Result<Category>() { Message = "The category could not be found." };
            }

            // Finding the Category means success regardless of DataPoint DateRange query result.
            var result = new Result<Category>();
            result.Message = $"{nameof(Category)} '{category.ToString()}' found";
            result.Success = true;
            result.Value = category;

            // Fetch the DataPoints for the Category with DateRange if requested.
            var dataPointsQuery = ChildRepository.GetQueryable().Where(d => d.CategoryId == category.Id);
            if (dateRangeCriteria.Value.To > DateTime.MinValue)
            {
                dataPointsQuery = dataPointsQuery.Where(d => d.Stamp >= dateRangeCriteria.Value.From &&
                                                                d.Stamp <= dateRangeCriteria.Value.To);
            }
            result.Value.DataPoints = (await ChildRepository.GetAsync(dataPointsQuery))?
                                                            .OrderBy(d => d.Stamp)
                                                            .ToList();

            return result;
        }

        public async Task<Result<Category[]>> UpsertAsync(Criteria<IEnumerable<Category>> categoryCriteria)
        {
            // Validate the criteria.
            if (categoryCriteria?.Value == null || categoryCriteria.Value.Count() == 0)
            {
                return new Result<Category[]>() { Message = "Invalid criteria" };
            }

            // Validate models.
            var validationErrors = new List<string>();
            var categories = categoryCriteria.Value.ToList();
            categories.ForEach(m =>
            {
                string[] messages;
                if (!m.TryValidateModel(out messages)) { validationErrors.AddRange(messages); }
            });
            if (validationErrors.Count > 0) { return new Result<Category[]>() { Message = $"Model validation errors: {String.Join("; ", validationErrors)}" }; }

            // Validate unique Id values.
            if (categories.Count != categories.Select(d => d.Id).Distinct().Count())
            {
                return new Result<Category[]>() { Message = "Id values must be unique." };
            }

            // Validate that descriptions are unique.
            if (categories.Count != categories.Select(d => d.Description).Distinct().Count())
            {
                return new Result<Category[]>() { Message = "Descriptions must be unique." };
            }

            // Get the account's current categories.
            var currentCategories = (await GetAllAsync(new Criteria<EmptyModel>() { PrimaryId = categoryCriteria.PrimaryId })).Value;

            // Validate that the new descriptions are unique for the account.
            var matches = categories.Where(d => currentCategories.Any(c => c.Description == d.Description && c.Id != d.Id))
                                    .Select(d => d.Description)
                                    .ToArray();
            if (matches.Length > 0)
            {
                return new Result<Category[]>() { Message = $"Descriptions already defined: {String.Join(",", matches)}" };
            }

            // Get tasks to upsert each new model then await results for all.
            List<Task<Category>> tasks = new List<Task<Category>>();
            categories.ForEach(m =>
            {
                var category = currentCategories.FirstOrDefault(c => c.Id == m.Id);
                if (category == null)
                {
                    category = m;
                    category.AccountId = categoryCriteria.PrimaryId;
                    category.DataPoints = null;
                    category.Id = Guid.NewGuid().ToString();
                }
                else
                {
                    category.Description = m.Description;
                    category.Units = m.Units;
                }
                tasks.Add(Repository.UpsertAsync(category));
            });

            var result = new Result<Category[]>();
            result.Value = (await Task.WhenAll(tasks))?.Where(d => d != null).ToArray();

            // Handle results.
            if (result.Value == null || result.Value.Count() == 0)
            {
                result.Message = "Categories failed to save.";
            }
            else
            {
                result.Message = $"Successfully upserted {result.Value.Count()} of {categoryCriteria.Value.Count()} requested categories.";
                result.Success = true;
            }
            return result;
        }
    }
}
