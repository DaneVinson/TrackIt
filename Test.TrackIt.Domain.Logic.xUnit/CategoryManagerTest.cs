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
    public class CategoryManagerTest
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
            IRepository<Category> repository = Substitute.For<IRepository<Category>>();
            IRepository<DataPoint> childRepository = Substitute.For<IRepository<DataPoint>>();
            switch (testCaseName)
            {
                case AttemptToDeleteInvalidIdFails:
                    repository.GetQueryable().Returns((new Category[0]).AsQueryable());
                    repository.GetAsync((new Category[0]).AsQueryable()).ReturnsForAnyArgs(null as Category[]);
                    break;
                case AttemptToDeleteAnyInvalidIdFails:
                    repository.GetQueryable().Returns((new Category[0]).AsQueryable());
                    repository.GetAsync((new Category[0]).AsQueryable()).ReturnsForAnyArgs(new Category[] { CategoryAlpha, CategoryBravo });
                    break;
                case NoSucessfulDeletesFails:
                    repository.GetQueryable().Returns((new Category[0]).AsQueryable());
                    repository.GetAsync((new Category[0]).AsQueryable()).ReturnsForAnyArgs(new Category[] { CategoryAlpha, CategoryBravo });
                    repository.DeleteAsync(CategoryAlphaId).Returns(false);
                    repository.DeleteAsync(CategoryBravoId).Returns(false);
                    break;
                case AnySucessfulDeleteIsSuccess:
                    repository.GetQueryable().Returns((new Category[0]).AsQueryable());
                    repository.GetAsync((new Category[0]).AsQueryable()).ReturnsForAnyArgs(new Category[] { CategoryAlpha, CategoryBravo });
                    repository.DeleteAsync(CategoryAlphaId).Returns(true);
                    repository.DeleteAsync(CategoryBravoId).Returns(false);
                    break;
            }
            var manager = new CategoryManager(repository, childRepository);

            // Act
            var task = manager.DeleteAsync(criteria);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.Equal(expectedResult.Message, result.Message);
            Assert.Equal(expectedResult.Success, result.Success);
        }

        [Theory, MemberData("GetAllTestCases")]
        public void GetAllAsyncTest(
            string testCaseName,
            string criteriaJson,
            string expectedResultJson)
        {
            // Arguments are serialized so that test cases show as individual tests in Test Explorer
            var criteria = JsonConvert.DeserializeObject<Criteria<EmptyModel>>(criteriaJson);
            var expectedResult = JsonConvert.DeserializeObject<Result<List<Category>>>(expectedResultJson);

            // Arrange
            IRepository<Category> repository = Substitute.For<IRepository<Category>>();
            IRepository<DataPoint> childRepository = Substitute.For<IRepository<DataPoint>>();
            if (testCaseName.Equals(NullResultFails))
            {
                repository.GetQueryable().Returns((new Category[0]).AsQueryable());
                repository.GetAsync((new Category[0]).AsQueryable()).ReturnsForAnyArgs(null as Category[]);
            }
            else
            {
                repository.GetQueryable().Returns((new Category[0]).AsQueryable());
                repository.GetAsync((new Category[0]).AsQueryable()).ReturnsForAnyArgs(new Category[0]);
            }

            var manager = new CategoryManager(repository, childRepository);

            // Act
            var task = manager.GetAllAsync(criteria);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.Equal(expectedResult.Message, result.Message);
            Assert.Equal(expectedResult.Success, result.Success);
        }

        [Theory, MemberData("GetTestCases")]
        public void GetAsyncTest(
            string testCaseName,
            string criteriaJson,
            string expectedResultJson)
        {
            // Arguments are serialized so that test cases show as individual tests in Test Explorer
            var criteria = JsonConvert.DeserializeObject<Criteria<DateRange>>(criteriaJson);
            var expectedResult = JsonConvert.DeserializeObject<Result<Category>>(expectedResultJson);

            // Arrange
            IRepository<Category> repository = Substitute.For<IRepository<Category>>();
            IRepository<DataPoint> childRepository = Substitute.For<IRepository<DataPoint>>();
            repository.GetAsync(CategoryAlphaId).Returns(CategoryAlpha);
            repository.GetAsync(CategoryBravoId).Returns(CategoryBravo);
            childRepository.GetQueryable().Returns(GetEmptyQueryable<DataPoint>());
            childRepository.GetAsync(GetEmptyQueryable<DataPoint>()).ReturnsForAnyArgs(new DataPoint[] { DataPointAlpha, DataPointBravo });
            var manager = new CategoryManager(repository, childRepository);

            // Act
            var task = manager.GetAsync(criteria);
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
            // Arguments are serialized so that test cases show as individual tests in Test Explorer
            var criteria = JsonConvert.DeserializeObject<Criteria<IEnumerable<Category>>>(criteriaJson);
            var expectedResult = JsonConvert.DeserializeObject<Result<Category[]>>(expectedResultJson);

            // Arrange
            IRepository<Category> repository = Substitute.For<IRepository<Category>>();
            IRepository<DataPoint> childRepository = Substitute.For<IRepository<DataPoint>>();
            repository.GetQueryable().Returns((new Category[0]).AsQueryable());
            repository.GetAsync((new Category[0]).AsQueryable()).ReturnsForAnyArgs(new Category[] { CategoryAlpha, CategoryBravo });
            switch (testCaseName)
            {
                case RepositoryUpsertFailureFails:
                    repository.UpsertAsync(Arg.Is<Category>(d => d.Id == CategoryAlphaId)).Returns(null as Category);
                    break;
                case PartialUpsertSuccess:
                    repository.UpsertAsync(Arg.Is<Category>(d => d.Id == CategoryAlphaId)).Returns(CategoryAlpha);
                    repository.UpsertAsync(Arg.Is<Category>(d => d.Id == CategoryBravoId)).Returns(null as Category);
                    break;
            }
            var manager = new CategoryManager(repository, childRepository);

            // Act
            var task = manager.UpsertAsync(criteria);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.True(result.Message.StartsWith(expectedResult.Message)); // StartsWith used to cover long validation messages
            Assert.Equal(expectedResult.Success, result.Success);
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
                        NullCriteriaFails,
                        String.Empty,   // null when serialized to json
                        NewDeleteResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        NullCriteriaValueFails,
                        NewDeleteCriteriaJson(AccountAlphaId, null),
                        NewDeleteResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        EmptyCriteriaValueFails,
                        NewDeleteCriteriaJson(AccountAlphaId, new string[0]),
                        NewDeleteResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        AttemptToDeleteInvalidIdFails,
                        NewDeleteCriteriaJson(AccountAlphaId, new string[] { UnknownId }),
                        NewDeleteResultJson("Invalid delete request.")
                    },
                    new object[]
                    {
                        AttemptToDeleteAnyInvalidIdFails,
                        NewDeleteCriteriaJson(AccountAlphaId, new string[] { CategoryAlphaId, UnknownId }),
                        NewDeleteResultJson("Invalid delete request.")
                    },
                    new object[]
                    {
                        NoSucessfulDeletesFails,
                        NewDeleteCriteriaJson(AccountAlphaId, new string[] { CategoryAlphaId, CategoryBravoId }),
                        NewDeleteResultJson("Requested deletes were unsuccessful.")
                    },
                    new object[]
                    {
                        AnySucessfulDeleteIsSuccess,
                        NewDeleteCriteriaJson(AccountAlphaId, new string[] { CategoryAlphaId, CategoryBravoId }),
                        NewDeleteResultJson("Deleted 1 of 2 requested categories.", true)
                    }
                };
            }
        }

        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        NullCriteriaFails,
                        String.Empty,   // null when serialized to json
                        NewGetAllResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        NullPrimaryIdFails,
                        NewGetAllCriteriaJson(null),
                        NewGetAllResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        EmptyPrimaryIdFails,
                        NewGetAllCriteriaJson(String.Empty),
                        NewGetAllResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        NullResultFails,
                        NewGetAllCriteriaJson(CategoryAlpha.AccountId),
                        NewGetAllResultJson("The attempt to get the account's categories failed.")
                    },
                    new object[]
                    {
                        EmptyResultIsSuccess,
                        NewGetAllCriteriaJson(AccountBravoId),
                        NewGetAllResultJson("The account has 0 categories.", true)
                    }
                };
            }
        }

        public static IEnumerable<object[]> GetTestCases
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        NullCriteriaFails,
                        String.Empty,   // null when serialized to json
                        NewGetResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        InvalidDateRangeFails,
                        NewGetCriteriaJson(Date2, Date1, CategoryAlphaId),
                        NewGetResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        InvalidCategoryIdFails,
                        NewGetCriteriaJson(Date1, Date2, UnknownId),
                        NewGetResultJson("The category could not be found.")
                    },
                    new object[]
                    {
                        FoundCategoryIsSuccess,
                        NewGetCriteriaJson(Date1, Date2, CategoryAlphaId),
                        NewGetResultJson($"{nameof(Category)} '{CategoryAlpha.ToString()}' found", true, CategoryAlpha)
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
                        NullCriteriaFails,
                        String.Empty,   // null when serialized to json
                        NewUpsertResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        NullCriteriaValueFails,
                        NewUpsertCriteriaJson(AccountAlphaId),
                        NewUpsertResultJson("Invalid criteria")
                    },
                    new object[]
                    {
                        InvalidModelFails,
                        NewUpsertCriteriaJson(AccountAlphaId, new Category[] { new Category()}),
                        NewUpsertResultJson("Model validation errors:")
                    },
                    new object[]
                    {
                        AnyInvalidModelFails,
                        NewUpsertCriteriaJson(AccountAlphaId, new Category[] { CategoryAlpha, new Category()}),
                        NewUpsertResultJson("Model validation errors:")
                    },
                    new object[]
                    {
                        DuplicateIdFails,
                        NewUpsertCriteriaJson(AccountAlphaId, new Category[] { CategoryAlpha, CategoryAlpha2 }),
                        NewUpsertResultJson("Id values must be unique.")
                    },
                    new object[]
                    {
                        DuplicateDescriptionFails,
                        NewUpsertCriteriaJson(AccountAlphaId, new Category[] { CategoryAlpha, CategoryDriving }),
                        NewUpsertResultJson("Descriptions must be unique.")
                    },
                    new object[]
                    {
                        ReservedDescriptionFails,
                        NewUpsertCriteriaJson(AccountAlphaId, new Category[] { CategoryDriving }),
                        NewUpsertResultJson("Descriptions already defined:")
                    },
                    new object[]
                    {
                        RepositoryUpsertFailureFails,
                        NewUpsertCriteriaJson(AccountAlphaId, new Category[] { CategoryAlpha }),
                        NewUpsertResultJson("Categories failed to save.")
                    },
                    new object[]
                    {
                        PartialUpsertSuccess,
                        NewUpsertCriteriaJson(AccountAlphaId, new Category[] { CategoryAlpha, CategoryBravo }),
                        NewUpsertResultJson("Successfully upserted 1 of 2 requested categories.", true, new Category[] { CategoryAlpha })
                    }
                };
            }
        }

        #endregion

        #region Static Utility

        private static IQueryable<T> GetEmptyQueryable<T>()
        {
            return (new T[0]).AsQueryable();
        }

        private static string NewDeleteCriteriaJson(
            string primaryId,
            string[] ids)
        {
            return JsonConvert.SerializeObject(new Criteria<IEnumerable<string>>()
            {
                PrimaryId = primaryId,
                Value = ids
            });
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

        private static string NewGetAllCriteriaJson(string primaryId)
        {
            return JsonConvert.SerializeObject(new Criteria<EmptyModel>()
            {
                PrimaryId = primaryId
            });
        }

        private static string NewGetAllResultJson(
            string message, 
            bool success = false, 
            IEnumerable<Category> value = null)
        {
            return JsonConvert.SerializeObject(new Result<List<Category>>()
            {
                Message = message,
                Success = success,
                Value = value?.ToList()
            });
        }

        private static string NewGetCriteriaJson(
            DateTime from,
            DateTime to,
            string primaryId)
        {
            return JsonConvert.SerializeObject(new Criteria<DateRange>()
            {
                PrimaryId = primaryId,
                Value = new DateRange()
                {
                    From = from,
                    To = to
                }
            });
        }

        private static string NewGetResultJson(
            string message,
            bool success = false,
            Category value = null)
        {
            return JsonConvert.SerializeObject(new Result<Category>()
            {
                Message = message,
                Success = success,
                Value = value
            });
        }

        private static string NewUpsertCriteriaJson(
            string primaryId,
            IEnumerable<Category> value = null)
        {
            return JsonConvert.SerializeObject(new Criteria<IEnumerable<Category>>()
            {
                PrimaryId = primaryId,
                Value = value
            });
        }

        private static string NewUpsertResultJson(
            string message,
            bool success = false,
            Category[] value = null)
        {
            return JsonConvert.SerializeObject(new Result<Category[]>()
            {
                Message = message,
                Success = success,
                Value = value
            });
        }

        #endregion

        #region Plumbing

        static CategoryManagerTest()
        {
            Date1 = new DateTime(2016, 10, 31, 5, 2, 3);
            Date2 = new DateTime(2016, 11, 1, 5, 2, 3);
            CategoryAlpha = new Category()
            {
                AccountId = AccountAlphaId,
                DataPoints = null,
                Description = Driving,
                Id = CategoryAlphaId,
                Units = "km"
            };
            CategoryAlpha2 = new Category()
            {
                AccountId = AccountAlphaId,
                DataPoints = null,
                Description = "Flying",
                Id = CategoryAlphaId,
                Units = "Miles"
            };
            CategoryBravo = new Category()
            {
                AccountId = AccountAlphaId,
                DataPoints = null,
                Description = "Distance data bravo",
                Id = CategoryBravoId,
                Units = "m"
            };
            DataPointAlpha = new DataPoint()
            {
                CategoryId = CategoryAlphaId,
                Id = DataPointAlphaId,
                Stamp = Date1,
                Value = 1
            };
            DataPointBravo = new DataPoint()
            {
                CategoryId = CategoryAlphaId,
                Id = DataPointBravoId,
                Stamp = Date2,
                Value = 2
            };
            CategoryDriving = new Category()
            {
                AccountId = AccountAlphaId,
                DataPoints = null,
                Description = Driving,
                Id = CategoryDrivingId,
                Units = "km"
            };
        }

        // Test case names
        private const string AnyInvalidModelFails = "AnyInvalidModelFails";
        private const string AnySucessfulDeleteIsSuccess = "AnySucessfulDeleteIsSuccess";
        private const string AttemptToDeleteAnyInvalidIdFails = "AttemptToDeleteAnyInvalidIdFails";
        private const string AttemptToDeleteInvalidIdFails = "AttemptToDeleteInvalidIdFails";
        private const string DuplicateDescriptionFails = "DuplicateDescriptionFails";
        private const string DuplicateIdFails = "DuplicateIdFails";
        private const string EmptyCriteriaValueFails = "EmptyCriteriaValueFails";
        private const string EmptyPrimaryIdFails = "EmptyPrimaryIdFails";
        private const string EmptyResultIsSuccess = "EmptyResultIsSuccess";
        private const string FoundCategoryIsSuccess = "FoundCategoryIsSuccess";
        private const string InvalidDateRangeFails = "InvalidDateRangeFails";
        private const string InvalidCategoryIdFails = "InvalidCategoryIdFails";
        private const string InvalidModelFails = "InvalidModelFails";
        private const string NoSucessfulDeletesFails = "NoSucessfulDeletesFails";
        private const string NullCriteriaFails = "NullCriteriaFails";
        private const string NullCriteriaValueFails = "NullCriteriaValueFails";
        private const string NullPrimaryIdFails = "NullPrimaryIdFails";
        private const string NullResultFails = "NullResultFails";
        private const string PartialUpsertSuccess = "PartialUpsertSuccess";
        private const string RepositoryUpsertFailureFails = "RepositoryUpsertFailureFails";
        private const string ReservedDescriptionFails = "ReservedDescriptionFails";

        private const string AccountAlphaId = "E2154A1A-B228-4853-8D74-C926D3618C4C";
        private const string AccountBravoId = "39CF87DD-5CBE-42B4-B14F-D0BE73525A76";
        private const string DataPointAlphaId = "9A46EBE3-7019-434A-88B6-9D059620E15F";
        private const string DataPointBravoId = "5D05F468-B0D8-4EC1-B727-133F770E5C3B";
        private const string CategoryAlphaId = "8EFAEA42-DC44-4831-8A60-09B17C39004E";
        private const string CategoryBravoId = "473C3873-D4F3-4823-9A7B-93274DA08B06";
        private const string CategoryDrivingId = "5A8C4DC6-0221-4AB3-AF0B-7F42DF887534";
        private const string Driving = "Driving";
        private const string UnknownId = "14285757-50ED-4388-B977-66F02A4D44D2";

        private static readonly Category CategoryAlpha;
        private static readonly Category CategoryAlpha2;
        private static readonly Category CategoryBravo;
        private static readonly Category CategoryDriving;
        private static readonly DataPoint DataPointAlpha;
        private static readonly DataPoint DataPointBravo;
        private static readonly DateTime Date1;
        private static readonly DateTime Date2;

        #endregion
    }
}
