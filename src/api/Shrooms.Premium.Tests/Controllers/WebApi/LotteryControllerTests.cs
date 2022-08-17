using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Premium.Presentation.Api.Controllers.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;
using Shrooms.Premium.Tests.ModelMappings;
using Shrooms.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.Controllers.WebApi
{
    [TestFixture]
    public class LotteryControllerTests
    {
        private LotteryController _sut;

        private ILotteryService _lotteryService;
        private ILotteryExportService _lotteryExportService;

        [SetUp]
        public void TestInitializers()
        {
            _lotteryService = Substitute.For<ILotteryService>();
            _lotteryExportService = Substitute.For<ILotteryExportService>();

            _sut = new LotteryController(ModelMapper.Create(), _lotteryService, _lotteryExportService);
            _sut.SetUpControllerForTesting();
        }

        [Test]
        public async Task BuyLotteryTicket_InvalidLotteryId_ReturnsBadRequest()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = -1,
                TicketCount = -10
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_LotteryIdNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                TicketCount = -10
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_ReceiversExistButDoesNotContainAnyEntries_ReturnsBadRequest()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = -10,
                Receivers = new LotteryTicketReceiverViewModel[0]
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_ReceiversExistButReceiverDoesNotContainUserId_ReturnsBadRequest()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = -10,
                Receivers = new LotteryTicketReceiverViewModel[]
                {
                    new LotteryTicketReceiverViewModel
                    {
                        TicketCount = 1
                    }
                }
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_ReceiversExistButReceiverContainsInvalidTicketCount_ReturnsBadRequest()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = -10,
                Receivers = new LotteryTicketReceiverViewModel[]
                {
                    new LotteryTicketReceiverViewModel
                    {
                        UserId = Guid.NewGuid().ToString(),
                        TicketCount = -10
                    }
                }
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }


        [Test]
        public async Task BuyLotteryTicket_ReceiversExistButThereAreDuplicateReceiverUserIds_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = -10,
                Receivers = new LotteryTicketReceiverViewModel[]
                {
                    new LotteryTicketReceiverViewModel
                    {
                        UserId = userId,
                        TicketCount = 10
                    },

                    new LotteryTicketReceiverViewModel
                    {
                        UserId = userId,
                        TicketCount = 10
                    }
                }
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_ValidValuesWithoutReceivers_ReturnsOk()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = 10,
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.OK;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_ValidValuesWithReceivers_ReturnsOk()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = 10,
                Receivers = new LotteryTicketReceiverViewModel[]
                {
                    new LotteryTicketReceiverViewModel
                    {
                        UserId = "id",
                        TicketCount = 1
                    },

                    new LotteryTicketReceiverViewModel
                    {
                        UserId = "id 2",
                        TicketCount = 1
                    }
                }
            };

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.OK;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_LotteryExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = 10,
                Receivers = new LotteryTicketReceiverViewModel[]
                {
                    new LotteryTicketReceiverViewModel
                    {
                        UserId = "id",
                        TicketCount = 1
                    },

                    new LotteryTicketReceiverViewModel
                    {
                        UserId = "id 2",
                        TicketCount = 1
                    }
                }
            };

            _lotteryService
                .BuyLotteryTicketsAsync(Arg.Any<BuyLotteryTicketsDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new LotteryException("Error"));

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task BuyLotteryTicket_ValidationExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var buyViewModel = new BuyLotteryTicketsViewModel
            {
                LotteryId = 1,
                TicketCount = 10,
                Receivers = new LotteryTicketReceiverViewModel[]
                {
                    new LotteryTicketReceiverViewModel
                    {
                        UserId = "id",
                        TicketCount = 1
                    },

                    new LotteryTicketReceiverViewModel
                    {
                        UserId = "id 2",
                        TicketCount = 1
                    }
                }
            };

            _lotteryService
                .BuyLotteryTicketsAsync(Arg.Any<BuyLotteryTicketsDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            _sut.Validate(buyViewModel);

            var expected = HttpStatusCode.BadRequest;

            // Act
            var httpActionResult = await _sut.BuyLotteryTicket(buyViewModel);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expected, actual.StatusCode);
        }

        [Test]
        public async Task GetAllLotteries_ValidValues_ReturnsOk()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.OK;

            // Act
            var httpActionResult = await _sut.GetAllLotteries();
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task GetPagedLotteries_ListingArgsAreNull_ReturnsOk()
        {
            // Arrange
            LotteryListingArgsViewModel args = null;

            var expectedStatus = HttpStatusCode.OK;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.GetPagedLotteries(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task GetPagedLotteries_ValidValues_ReturnsOk()
        {
            // Assert
            var args = new LotteryListingArgsViewModel();

            var expectedStatus = HttpStatusCode.OK;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.GetPagedLotteries(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task GetPagedLotteries_NegativePage_ReturnsBadRequest()
        {
            // Assert
            var args = new LotteryListingArgsViewModel
            {
                Page = -1
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.GetPagedLotteries(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task GetPagedLotteries_NegativePageSize_ReturnsBadRequest()
        {
            // Assert
            var args = new LotteryListingArgsViewModel
            {
                Page = 1,
                PageSize = -10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.GetPagedLotteries(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task GetLottery_ValidValues_ReturnsOk()
        {
            // Assert
            const int id = 1;
            var expectedStatus = HttpStatusCode.OK;

            _lotteryService
                .GetLotteryDetailsAsync(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<UserAndOrganizationDto>())
                .Returns(new LotteryDetailsDto());

            // Act
            var httpActionResult = await _sut.GetLottery(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task GetLottery_InvalidId_ReturnsNotFound()
        {
            // Assert
            const int id = 1;
            var expectedStatus = HttpStatusCode.NotFound;

            _lotteryService
                .GetLotteryDetailsAsync(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<UserAndOrganizationDto>())
                .Returns((LotteryDetailsDto)null);

            // Act
            var httpActionResult = await _sut.GetLottery(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_ValidValues_ReturnsOk()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Status = LotteryStatus.Started,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.OK;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_WhenLotteryExceptionIsThrown_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Status = LotteryStatus.Started,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _lotteryService
                .CreateLotteryAsync(Arg.Any<LotteryDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new LotteryException("Error"));

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_TitleNotPresent_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Description = "desc",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Status = LotteryStatus.Started,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_DescriptionOverCharacterLimit_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = new string('d', 1000),
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Status = LotteryStatus.Started,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_EndDateLessThanPresentDate_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EndDate = DateTime.UtcNow.AddDays(-1),
                EntryFee = 1,
                Status = LotteryStatus.Started,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_EndDateNotPresent_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EntryFee = 1,
                Status = LotteryStatus.Started,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_StatusNotPresent_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_StatusIsNotStartedAndNotDraft_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Status = LotteryStatus.Ended,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_NoImages_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Status = LotteryStatus.Started,
                GiftedTicketLimit = 0
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task CreateLottery_GiftedLimitLessThanZero_ReturnsBadRequest()
        {
            var args = new CreateLotteryViewModel
            {
                Title = "title",
                Description = "desc",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 1,
                Status = LotteryStatus.Started,
                Images = new ImagesCollection(),
                GiftedTicketLimit = -10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _sut.Validate(args);

            // Act
            var httpActionResult = await _sut.CreateLottery(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task Abort_ValidId_ReturnsOk()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.OK;
            const int id = 1;

            _lotteryService
                .AbortLotteryAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Returns(true);

            // Act
            var httpActionResult = await _sut.Abort(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task Abort_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.NotFound;
            const int id = 1;

            _lotteryService
                .AbortLotteryAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Returns(false);

            // Act
            var httpActionResult = await _sut.Abort(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task RefundParticipants_ValidValues_ReturnsOk()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.OK;

            // Act
            var httpActionResult = await _sut.RefundParticipants(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task GetStatus_ValidValues_ReturnsOk()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.OK;

            // Act
            var httpActionResult = await _sut.GetStatus(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_ValidValues_ReturnsOk()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.OK;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_LotteryExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            _lotteryService
                .EditDraftedLotteryAsync(Arg.Any<LotteryDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new LotteryException("Error"));

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_IdNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }


        [Test]
        public async Task UpdateDrafted_TitleNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_StatusNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_StatusDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = (LotteryStatus)int.MaxValue,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_EndDateNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_EndDateLessThanPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(-1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_EntryFeeLessOrEqualZero_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 0,
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_EntryFeeNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                Images = new ImagesCollection(),
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_NoImages_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                GiftedTicketLimit = 10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_GiftedTicketLimitNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection()
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateDrafted_GiftedTicketLimitLessThanZero_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditDraftedLotteryViewModel
            {
                Id = 1,
                Title = "title",
                Status = LotteryStatus.Drafted,
                EndDate = DateTime.UtcNow.AddYears(1),
                EntryFee = 10,
                Images = new ImagesCollection(),
                GiftedTicketLimit = -10
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateDrafted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateStarted_ValidValues_ReturnsOk()
        {
            // Arrange
            var args = new EditStartedLotteryViewModel
            {
                Id = 10,
                Description = "test"
            };

            var expectedStatus = HttpStatusCode.OK;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateStarted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateStarted_LotteryExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditStartedLotteryViewModel
            {
                Id = 10,
                Description = "test"
            };

            _lotteryService
                .EditStartedLotteryAsync(Arg.Any<EditStartedLotteryDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new LotteryException("Error"));

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateStarted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateStarted_IdNotPresent_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditStartedLotteryViewModel
            {
                Description = "test"
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateStarted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task UpdateStarted_DescriptionOverCharacterLimit_ReturnsBadRequest()
        {
            // Arrange
            var args = new EditStartedLotteryViewModel
            {
                Id = 10,
                Description = new string('-', 100000)
            };

            var expectedStatus = HttpStatusCode.BadRequest;

            // Act
            _sut.Validate(args);

            var httpActionResult = await _sut.UpdateStarted(args);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task FinishLottery_ValidValues_ReturnsOk()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.OK;

            // Act
            var httpActionResult = await _sut.FinishLottery(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task FinishLottery_LotteryExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.BadRequest;

            _lotteryService
                .FinishLotteryAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new LotteryException("Error"));

            // Act
            var httpActionResult = await _sut.FinishLottery(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task LotteryStats_ValidValues_ReturnsOk()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.OK;

            _lotteryService
                .GetLotteryStatsAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Returns(new LotteryStatsDto());

            // Act
            var httpActionResult = await _sut.LotteryStats(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task LotteryStats_InvalidId_ReturnsNotFound()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.NotFound;

            _lotteryService
                .GetLotteryStatsAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Returns((LotteryStatsDto)null);

            // Act
            var httpActionResult = await _sut.LotteryStats(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task Export_ValidValues_ReturnsOk()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.OK;

            // Act
            var httpActionResult = await _sut.Export(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }

        [Test]
        public async Task Export_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            const int id = 1;
            var expectedStatus = HttpStatusCode.BadRequest;

            _lotteryExportService
                .ExportParticipantsAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new LotteryException("Error"));

            // Act
            var httpActionResult = await _sut.Export(id);
            var actual = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Arrange
            Assert.AreEqual(expectedStatus, actual.StatusCode);
        }
    }
}
