using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Dto;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;
using TrackIt.Domain.Logic.Managers;
using Xunit;
using Xunit.Extensions;

namespace Test.TrackIt.Domain.Logic.xUnit
{
    public class DataPointManagerTest
    {
        #region Tests

        [Theory, MemberData("DeleteTestCases")]
        public void DeleteAsyncTest(
            string testCaseName,
            string criteriaJson,
            string expectedResultJson)
        {
            // Arguments are serialized so that test cases show as individual tests in Test Explorer
            var criteria = JsonConvert.DeserializeObject<Criteria<IEnumerable<string>>>(criteriaJson);
            var expectedResult = JsonConvert.DeserializeObject<Result<EmptyModel>>(expectedResultJson);

            // Arrange
            IRepository<DataPoint> repository = Substitute.For<IRepository<DataPoint>>();
            for (int i = 0; i < DefaultDataPoints.Length - 1; i++)
            {
                repository.DeleteAsync(DefaultDataPoints[i].Id).Returns(true);
                repository.GetAsync(DefaultDataPoints[i].Id).Returns(DefaultDataPoints[i]);
            }
            repository.DeleteAsync(DefaultDataPoints[DefaultDataPoints.Length - 1].Id).Returns(false);
            repository.DeleteAsync(InvalidId).Returns(false);
            repository.GetAsync(DefaultDataPoints[DefaultDataPoints.Length - 1].Id).Returns(DefaultDataPoints[DefaultDataPoints.Length - 1]);
            repository.GetAsync(InvalidId).Returns(null as DataPoint);
            var manager = new DataPointManager(repository);

            // Act
            var task = manager.DeleteAsync(criteria);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.Equal(expectedResult.Message, result.Message);
            Assert.Equal(expectedResult.Success, result.Success);
        }

        [Theory, MemberData("UpsertTestCases")]
        public void UpsertAsyncTest(
            string testCaseName,
            string criteriaJson,
            string expectedResultJson)
        {
            // Arguments are serialized so that test cases show as individual tests in the xUnit Test Explorer
            var criteria = JsonConvert.DeserializeObject<Criteria<IEnumerable<DataPoint>>>(criteriaJson);
            var expectedResult = JsonConvert.DeserializeObject<Result<List<DataPoint>>>(expectedResultJson);

            // Arrange
            IRepository<DataPoint> repository = Substitute.For<IRepository<DataPoint>>();
            repository.GetAsync(NewDataPoint.Id).Returns(null as DataPoint);
            repository.GetAsync(ValidModifiedDataPoint.Id).Returns(DefaultDataPoints[0]);
            repository.GetAsync(InvalidModifiedDataPoint.Id).Returns(DefaultDataPoints[1]);
            var correctedArg = Arg.Is<DataPoint>(d => d.Id == InvalidModifiedDataPoint.Id &&
                                                        d.CategoryId == CorrectedModifiedDataPoint.CategoryId &&
                                                        d.Stamp == InvalidModifiedDataPoint.Stamp &&
                                                        d.Value == InvalidModifiedDataPoint.Value);
            repository.UpsertAsync(correctedArg).Returns(CorrectedModifiedDataPoint);
            repository.UpsertAsync(Arg.Is<DataPoint>(d => d.Id == DefaultDataPoints[2].Id)).Returns(null as DataPoint);
            repository.UpsertAsync(Arg.Is<DataPoint>(d => d.Id == ValidModifiedDataPoint.Id)).Returns(ValidModifiedDataPoint);
            var manager = new DataPointManager(repository);

            // Act
            var task = manager.UpsertAsync(criteria);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.True(result.Message.StartsWith(expectedResult.Message)); // StartsWith used to cover long validation messages
            Assert.Equal(expectedResult.Success, result.Success);
            Assert.True(DataPointsAreEqual(result.Value, expectedResult.Value));
        }

        #endregion

        #region Test Cases

        public static IEnumerable<object[]> DeleteTestCases
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        "NullCriteriaFails" ,
                        String.Empty,   // null when serialized to json
                        NewDeleteResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        "NullCriteriaValueFails",
                        NewDeleteCriteriaJson(),
                        NewDeleteResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        "EmptyCriteriaValueFails",
                        NewDeleteCriteriaJson(new string[0]),
                        NewDeleteResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        "InvalidIdFails",
                        NewDeleteCriteriaJson(new string[] { DefaultDataPoints[0].Id, InvalidId }),
                        NewDeleteResultJson("Data points could not be found.")
                    },
                    new object[]
                    {
                        "DeleteFails",
                        NewDeleteCriteriaJson(new string[] { DefaultDataPoints.Last().Id }),
                        NewDeleteResultJson("Deleted 0 of 1 requested data points.")
                    },
                    new object[]
                    {
                        "PartialDeleteSuccess",
                        NewDeleteCriteriaJson(new string[] { DefaultDataPoints[0].Id, DefaultDataPoints.Last().Id }),
                        NewDeleteResultJson("Deleted 1 of 2 requested data points.", success: true)
                    },
                    new object[]
                    {
                        "FullDeleteSuccess",
                        NewDeleteCriteriaJson(new string[] { DefaultDataPoints[0].Id, DefaultDataPoints[1].Id }),
                        NewDeleteResultJson("Deleted 2 of 2 requested data points.", success: true)
                    }
                };
            }
        }

        public static IEnumerable<object[]> UpsertTestCases
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        "NullCriteriaFails" ,
                        String.Empty,   // null when serialized to json
                        NewUpsertResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        "NullCriteriaValueFails",
                        NewUpsertCriteriaJson(),
                        NewUpsertResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        "EmptyCriteriaValueFails",
                        NewUpsertCriteriaJson(new DataPoint[0]),
                        NewUpsertResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        "RepositoryUpsertFailureFails",
                        NewUpsertCriteriaJson(new DataPoint[] { DefaultDataPoints[2] }),
                        NewUpsertResultJson("Data points failed to save.")
                    },
                    new object[]
                    {
                        "PartialUpsertSuccess",
                        NewUpsertCriteriaJson(new DataPoint[] { DefaultDataPoints[1], ValidModifiedDataPoint }),
                        NewUpsertResultJson("Successfully upserted 1 of 2 requested data points.", true, new DataPoint[] { ValidModifiedDataPoint })
                    },
                    new object[]
                    {
                        "InvalidModifyCorrectedSuccess",
                        NewUpsertCriteriaJson(new DataPoint[] { InvalidModifiedDataPoint }),
                        NewUpsertResultJson("Successfully upserted 1 of 1 requested data points.", true, new DataPoint[] { CorrectedModifiedDataPoint })
                    },
                    new object[]
                    {
                        "ValidModifySuccess",
                        NewUpsertCriteriaJson(new DataPoint[] { ValidModifiedDataPoint }),
                        NewUpsertResultJson("Successfully upserted 1 of 1 requested data points.", true, new DataPoint[] { ValidModifiedDataPoint })
                    }
                };
            }
        }

        #endregion

        #region Static Utility and Plumbing

        private static bool DataPointsAreEqual(DataPoint dataPoint1, DataPoint dataPoint2)
        {
            if (dataPoint1 == null && dataPoint2 == null) { return true; }
            else if (dataPoint1 == null || dataPoint2 == null) { return false; }

            return dataPoint1.CategoryId == dataPoint2.CategoryId &&
                    dataPoint1.Id == dataPoint2.Id &&
                    dataPoint1.Stamp == dataPoint2.Stamp &&
                    dataPoint1.Value == dataPoint2.Value;
        }

        private static bool DataPointsAreEqual(IEnumerable<DataPoint> dataPoints1, IEnumerable<DataPoint> dataPoints2)
        {
            // If both collections are null or emtpy they're equal. Othwise if the collections don't match
            // in size they're not equal.
            if ((dataPoints1 == null || dataPoints1.Count() == 0) && 
                (dataPoints2 == null || dataPoints2.Count() == 0))
            {
                return true;
            }
            else if ((dataPoints1 == null && dataPoints2 != null) ||
                (dataPoints1 != null && dataPoints2 == null) ||
                (dataPoints1.Count() != dataPoints2.Count()))
            {
                return false;
            }

            // Match the two collections.
            return dataPoints1.Any(d1 => DataPointsAreEqual(d1, dataPoints2.FirstOrDefault(d2 => d2.Id == d1.Id)));
        }

        private static string NewDeleteCriteriaJson(IEnumerable<string> value = null)
        {
            return JsonConvert.SerializeObject(new Criteria<IEnumerable<string>>() { Value = value });
        }

        private static string NewDeleteResultJson(string message, bool success = false, EmptyModel value = null)
        {
            return JsonConvert.SerializeObject(new Result<EmptyModel>()
            {
                Message = message,
                Success = success,
                Value = value
            });
        }

        private static string NewUpsertCriteriaJson(IEnumerable<DataPoint> value = null)
        {
            return JsonConvert.SerializeObject(new Criteria<IEnumerable<DataPoint>>() { Value = value });
        }

        private static string NewUpsertResultJson(string message, bool success = false, IEnumerable<DataPoint> value = null)
        {
            return JsonConvert.SerializeObject(new Result<List<DataPoint>>()
            {
                Message = message,
                Success = success,
                Value = value?.ToList()
            });
        }


        static DataPointManagerTest()
        {
            InvalidModifiedDataPoint = new DataPoint()
            {
                CategoryId = "C2DD7E4A-6A05-4EEE-B378-0743F5CF00D8",
                Id = DefaultDataPoints[1].Id,
                Stamp = DefaultDataPoints[1].Stamp.AddDays(-1),
                Value = DefaultDataPoints[1].Value + 23
            };
            CorrectedModifiedDataPoint = new DataPoint()
            {
                CategoryId = DefaultCategoryId,
                Id = InvalidModifiedDataPoint.Id,
                Stamp = InvalidModifiedDataPoint.Stamp,
                Value = InvalidModifiedDataPoint.Value
            };
            NewDataPoint = new DataPoint()
            {
                CategoryId = DefaultCategoryId,
                Id = "A890D310-6125-4454-9220-93D9728C976B",
                Stamp = Now,
                Value = 23
            };
            ValidModifiedDataPoint = new DataPoint()
            {
                CategoryId = DefaultDataPoints[0].CategoryId,
                Id = DefaultDataPoints[0].Id,
                Stamp = DefaultDataPoints[0].Stamp.AddDays(-1),
                Value = DefaultDataPoints[0].Value + 1
            };
        }

        public static readonly string DefaultCategoryId = "A0BAA48B-9F0D-4147-8B07-8ADDB908C4BD";
        public static readonly DateTime Now = new DateTime(2016, 03, 06);
        public static readonly DataPoint[] DefaultDataPoints = new DataPoint[]
        {
            // Reserved for test of modify
            new DataPoint() { CategoryId = DefaultCategoryId, Id = "3B041E24-CF9A-4229-89CC-4BDFEFB08F15", Stamp = Now, Value = 1 },
            // Reserved for test of modify with an invalid property assignment, i.e. CategoryId
            new DataPoint() { CategoryId = DefaultCategoryId, Id = "E366D2E9-C12D-4F4A-9B35-2385C47A6941", Stamp = Now, Value = 2 },
            // Reserved for upsert failure test
            new DataPoint() { CategoryId = DefaultCategoryId, Id = "729B1C56-C790-4A04-A691-EC82480B924D", Stamp = Now, Value = 3 },
            new DataPoint() { CategoryId = DefaultCategoryId, Id = "40F180E7-1D17-452F-9E91-A8F5062001FA", Stamp = Now, Value = 4 }
        };

        public static readonly string InvalidId = "wrong";
        public static readonly DataPoint InvalidModifiedDataPoint;
        public static readonly DataPoint CorrectedModifiedDataPoint;
        public static readonly DataPoint NewDataPoint;
        public static readonly DataPoint ValidModifiedDataPoint;

        #endregion
    }
}
