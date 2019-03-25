using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ace.Shrooms.Models.Mappings;
using DataLayer;
using DataLayer.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ace.Shrooms.Controllers;
using NSubstitute;
using System.Web.Mvc;
using Ace.Shrooms.Models;

namespace Ace.Shrooms.Tests.Controllers
{
    [TestClass]
    public class FeedbackControllerTests
    {
        #region Test init

        private MockUnitOfWork _unitOfWork;
        private IRepository<Feedback> _feedbackRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            ShroomMapper<object>.ConfigureMappings();
            _unitOfWork = new MockUnitOfWork();
            _feedbackRepository = _unitOfWork.GetRepository<Feedback>();
        }

        #endregion


        public FeedbackController GetController()
        {
            var controller = new FeedbackController(_unitOfWork);
            controller.ControllerContext = Substitute.For<ControllerContext>();

            return controller;
        }


        #region Index

        [TestMethod]
        public void Feedback_Index_Should_Return_ViewResult()
        {
            var controller = this.GetController();

            var result = controller.Index();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Feedback_Index_Should_Return_Correct_ViewModel()
        {
            var controller = this.GetController();

            var result = controller.Index() as ViewResult;

            Assert.IsInstanceOfType(result.Model, typeof(FeedbackViewPagedModel));
        }

        [TestMethod]
        public void Feedback_Index_Should_Return_Correct_Results()
        {
            var controller = this.GetController();

            var result = controller.Index() as ViewResult;
            var model = result.Model as FeedbackViewPagedModel;

            Assert.IsTrue(model.PagedList.Any());
        }

        [TestMethod]
        public void Feedback_Index_Sort_Asc_Is_Working()
        {
            var controller = this.GetController();

            var result = controller.Index(sort: "TimestampSent", dir: "asc") as ViewResult;
            var model = result.Model as FeedbackViewPagedModel;

            Assert.IsTrue(string.Compare(model.PagedList[0].TimestampSent, model.PagedList[1].TimestampSent) < 0);
            Assert.IsTrue(string.Compare(model.PagedList[1].TimestampSent, model.PagedList[2].TimestampSent) < 0);
        }

        [TestMethod]
        public void Feedback_Index_Sort_Desc_Is_Working()
        {
            var controller = this.GetController();

            var result = controller.Index(sort: "TimestampSent", dir: "desc") as ViewResult;
            var model = result.Model as FeedbackViewPagedModel;

            Assert.IsTrue(string.Compare(model.PagedList[0].TimestampSent, model.PagedList[1].TimestampSent) > 0);
            Assert.IsTrue(string.Compare(model.PagedList[1].TimestampSent, model.PagedList[2].TimestampSent) > 0);
        }

        [TestMethod]
        public void Feedback_Index_Filter_Is_Working()
        {
            var controller = this.GetController();

            var result = controller.Index() as ViewResult;
            var model = result.Model as FeedbackViewPagedModel;

            var resultFiltered = controller.Index(s: "Ofise") as ViewResult;
            var modelFiltered = resultFiltered.Model as FeedbackViewPagedModel;

            Assert.IsTrue(model.PagedList.Count() > modelFiltered.PagedList.Count());
            Assert.AreEqual(model.PagedList.Count(), 3);
            Assert.AreEqual(modelFiltered.PagedList.Count(), 1);
        }

        [TestMethod]
        public void Feedback_Index_Paging_Is_Working()
        {
            var controller = this.GetController();

            var resultFirstPage = controller.Index(page: 1) as ViewResult;
            var modelFirstPage = resultFirstPage.Model as FeedbackViewPagedModel;

            var resultSecondPage = controller.Index(page: 2) as ViewResult;
            var modelSecondPage = resultSecondPage.Model as FeedbackViewPagedModel;

            Assert.AreEqual(modelFirstPage.PagedList.Count(), 3);
            Assert.AreEqual(modelFirstPage.PagedList.PageNumber, 1);
            Assert.AreEqual(modelSecondPage.PagedList.Count(), 1);
            Assert.AreEqual(modelSecondPage.PagedList.PageNumber, 2);
        }

        #endregion

        #region Create

        [TestMethod]
        public void Feedback_Create_Returns_ViewResult()
        {
            var controller = this.GetController();

            var result = controller.Create();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Feedback_Create_New_Feedback_Inserted()
        {
            var controller = this.GetController();
            FeedbackViewModel newFeedback = new FeedbackViewModel
            {
                Email = "guest-reply@mailinator.com",
                Subject = "Hi devops",
                Message = "Na-na-na, just another useless feedback",
                TimestampSent = DateTime.Now.ToString()
            };

            var countBeforeAdding = this._feedbackRepository.Get().Count();

            var result = controller.Create(newFeedback);
            var countAfterAdding = this._feedbackRepository.Get().Count();

            Assert.AreNotEqual(countBeforeAdding, countAfterAdding);
            Assert.IsTrue(countBeforeAdding < countAfterAdding);
        }

        #endregion

        #region Delete

        [TestMethod]
        public void Feedback_Delete_Item_Is_Removed()
        {
            var controller = this.GetController();

            var feedbackBeforeDelete = _feedbackRepository.GetByID(1);
            var result = controller.Delete(1, 1);
            var feedbackAfterDelete = _feedbackRepository.GetByID(1);

            Assert.IsNotNull(feedbackBeforeDelete);
            Assert.IsNull(feedbackAfterDelete);
        }

        [TestMethod]
        public void Feedback_Delete_RedirectsToIndex()
        {
            var controller = this.GetController();

            var result = controller.Delete(1, 1) as RedirectToRouteResult;

            Assert.AreEqual(result.RouteValues["action"].ToString(), "Index");
        }

        #endregion
    }
}
