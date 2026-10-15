using System;
using NUnit.Framework;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.Controllers.ViewModels.VideoLibrary
{
    [TestFixture]
    public class VideoLibraryListArgsViewModelTests
    {
        [Test]
        public void Should_Accept_A_Single_Day_Range()
        {
            var viewModel = new VideoLibraryListArgsViewModel
            {
                From = new DateTime(2026, 3, 1),
                To = new DateTime(2026, 3, 1)
            };

            Assert.That(viewModel.IsValid(), Is.True);
        }

        [Test]
        public void Should_Accept_A_Single_Day_Range_Whatever_The_Times()
        {
            var viewModel = new VideoLibraryListArgsViewModel
            {
                From = new DateTime(2026, 3, 1, 14, 0, 0),
                To = new DateTime(2026, 3, 1, 0, 0, 0)
            };

            Assert.That(viewModel.IsValid(), Is.True);
        }

        [Test]
        public void Should_Accept_An_Open_Ended_Range()
        {
            var viewModel = new VideoLibraryListArgsViewModel { From = new DateTime(2026, 3, 1) };

            Assert.That(viewModel.IsValid(), Is.True);
        }

        [Test]
        public void Should_Reject_A_Range_That_Ends_Before_It_Starts()
        {
            var viewModel = new VideoLibraryListArgsViewModel
            {
                From = new DateTime(2026, 3, 2),
                To = new DateTime(2026, 3, 1)
            };

            Assert.That(viewModel.IsValid(), Is.False);
        }
    }
}
