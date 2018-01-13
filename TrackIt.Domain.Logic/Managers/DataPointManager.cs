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
    public class DataPointManager : Manager<DataPoint>, IDataPointManager
    {
        public DataPointManager(
            IRepository<DataPoint> repository,
            IRepository<Category> parentRepository) :
            base(repository)
        {
            ParentRepository = parentRepository;
        }


        public async Task<Result<EmptyModel>> DeleteAsync(Criteria<IEnumerable<string>> idsCriteria)
        {
            // Validate criteria.
            if (idsCriteria?.Value == null || idsCriteria.Value.Count() == 0)
            {
                return new Result<EmptyModel>() { Message = "Invalid criteria" };
            }

            // Don't attempt to delete the same data point more than once.
            idsCriteria.Value = idsCriteria.Value.Distinct().ToArray();

            // Get the current DataPoints.
            var dataPointTasks = new List<Task<DataPoint>>();
            idsCriteria.Value
                        .ToList()
                        .ForEach(c => 
                        {
                            dataPointTasks.Add(Repository.GetAsync(c));
                        });
            var dataPoints = await Task.WhenAll(dataPointTasks);

            // Validate that all the requested deletes have matches.
            if (dataPoints.Any(d => d == null))
            {
                return new Result<EmptyModel>() { Message = "Data points could not be found." };
            }

            // Delete the requested DataPoints.
            var deleteTasks = new List<Task<bool>>();
            idsCriteria.Value
                        .ToList()
                        .ForEach(c => { deleteTasks.Add(Repository.DeleteAsync(c)); });
            var deleted = await Task.WhenAll(deleteTasks);

            // Handle results.
            var result = new Result<EmptyModel>();
            result.Success = deleted.Any(b => b == true);
            result.Message = $"Deleted {deleted.Where(b => b == true).Count()} of {idsCriteria.Value.Count()} requested data points.";
            return result;
        }

        public async Task<Result<DataPoint[]>> UpsertAsync(Criteria<IEnumerable<DataPoint>> dataPointsCriteria)
        {
            // Validate the criteria.
            if (dataPointsCriteria?.Value == null || dataPointsCriteria.Value.Count() == 0)
            {
                return new Result<DataPoint[]>() { Message = "Invalid criteria" };
            }

            // Validate models.
            var validationErrors = new List<string>();
            var dataPoints = dataPointsCriteria.Value.ToList();
            dataPoints.ForEach(m =>
            {
                string[] messages;
                if (!m.TryValidateModel(out messages)) { validationErrors.AddRange(messages); }
            });
            if (validationErrors.Count > 0) { return new Result<DataPoint[]>() { Message = $"Model validation errors: {String.Join("; ", validationErrors)}" }; }

            // Ensure all CategoryId values are valid.
            List<Task<Category>> categoryTasks = new List<Task<Category>>();
            dataPoints.GroupBy(d => d.CategoryId)
                    .Select(g => g.Key)
                    .ToList()
                    .ForEach(id => categoryTasks.Add(ParentRepository.GetAsync(id)));
            var categories = await Task.WhenAll(categoryTasks);
            if (categories.Any(c => c == null))
            {
                return new Result<DataPoint[]>() { Message = "Data point references missing category." };
            }

            // Get tasks to search for current matching DataPoints then await all.
            List<Task<DataPoint>> tasks = new List<Task<DataPoint>>();
            dataPoints.ForEach(m =>
            {
                tasks.Add(Repository.GetAsync(m.Id));
            });
            var currentDataPoints = (await Task.WhenAll(tasks)).Where(d => d != null).Where(m => m != null).ToList();

            // For all new DataPoints ensure we have a valid new Id.
            dataPoints.Where(d => !currentDataPoints.Any(c => c.Id == d.Id))
                    .ToList()
                    .ForEach(d => { d.Id = Guid.NewGuid().ToString(); });

            // Control which fields are updated.
            currentDataPoints.ForEach(c =>
            {
                var dataPoint = dataPoints.First(d => d.Id == c.Id);
                dataPoint.CategoryId = c.CategoryId;    // Ensure parent is never changed
            });

            // Get tasks to upsert each new model then await all.
            tasks = new List<Task<DataPoint>>();
            dataPoints.ForEach(m =>
            {
                tasks.Add(Repository.UpsertAsync(m));
            });
            var result = new Result<DataPoint[]>();
            result.Value = (await Task.WhenAll(tasks))?.Where(d => d != null).ToArray();

            // Handle results.
            if (result.Value == null || result.Value.Count() == 0)
            {
                result.Message = "Data points failed to save.";
            }
            else
            {
                result.Message = $"Successfully upserted {result.Value.Count()} of {dataPointsCriteria.Value.Count()} requested data points.";
                result.Success = true;
            }
            return result;
        }


        private readonly IRepository<Category> ParentRepository;
    }
}
