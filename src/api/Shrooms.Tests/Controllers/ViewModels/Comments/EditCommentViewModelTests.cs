using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts.Comments;
using Shrooms.Tests.Extensions;
using System.Collections.Generic;

namespace Shrooms.Tests.Controllers.ViewModels.Comments
{
    [TestFixture]
    public class EditCommentViewModelTests
    {
        [Test]
        public void NewInstance_ValidValues_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            // Assert
            Assert.IsTrue(viewModel.IsValid());
        }

        [Test]
        public void NewInstance_InvalidIdValue_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            viewModel.Id = 0;

            // Assert
            Assert.IsFalse(viewModel.IsValid());
        }

        [Test]
        public void NewInstance_WithoutMessageBodyAndWithImages_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            viewModel.MessageBody = null;

            // Assert
            Assert.IsTrue(viewModel.IsValid());
        }

        [Test]
        public void NewInstance_WithoutMessageBodyAndWithoutImages_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            viewModel.Images = null;
            viewModel.MessageBody = null;

            // Assert
            Assert.IsFalse(viewModel.IsValid());
        }

        [Test]
        public void NewInstance_WithoutMessageBodyAndEmptyImageCollection_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            viewModel.Images = new List<string>();
            viewModel.MessageBody = null;

            // Assert
            Assert.IsFalse(viewModel.IsValid());
        }

        [Test]
        public void NewInstance_WithoutMessageBodyAndImagesContainNullValues_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            viewModel.Images = new List<string>
            {
                "image",
                null,
                "image"
            };

            viewModel.MessageBody = null;

            // Assert
            Assert.IsFalse(viewModel.IsValid());
        }

        [Test]
        public void NewInstance_MessageBodyOverCharacterLimit_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            viewModel.MessageBody = new string('-', ValidationConstants.MaxCommentMessageBodyLength + 1);

            // Assert
            Assert.IsFalse(viewModel.IsValid());
        }

        [Test]
        public void NewInstance_WithoutMessageBodyAndEmptyImageCollectionButPictureIdIsSet_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateValidViewModel();

            viewModel.Images = null;
            viewModel.MessageBody = null;
            viewModel.PictureId = "id";

            // Assert
            Assert.IsTrue(viewModel.IsValid());
        }

        private EditCommentViewModel CreateValidViewModel()
        {
            return new EditCommentViewModel
            {
                MessageBody = "body",
                Id = 1,
                Images = new List<string>
                {
                    "image"
                }
            };
        }
    }
}
