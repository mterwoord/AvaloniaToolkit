using Avalonia.Media;
using AvaloniaToolkit.Imaging;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AvaloniaToolkit.Tests.Imaging
{
    [TestClass]
    public class ColorExtensionsTestsHsv
    {
        [TestMethod]
        public void TestToHsv_Hue()
        {
            //Colors.Red.ToHsv().H.Should().Be(0);
            Colors.Fuchsia.ToHsv().H.Should().Be(300);
        }
    }
}