using System.Reflection;
using CameraScanner.Maui.Tests.TestData;
using FluentAssertions;
using Xunit;

namespace CameraScanner.Maui.Services.Tests
{
    public class BarcodeParserTests
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();


        [Fact]
        public void ShouldParseNull()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();

            // Act
            var result = barcodeParser.Parse<TextParsedResult>(source: null);

            // Assert
            result.Should().NotBeNull();
            result.Text.Should().BeNull();
            result.DisplayResult.Should().BeNull();
        }

        [Fact]
        public void ShouldParseVCard()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            var source = ResourceLoader.Current.GetEmbeddedResourceString(Assembly, "VCard_Test1.txt");

            // Act
            var result = barcodeParser.Parse<AddressBookParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(AddressBookParsedResults.GetVCardTest1());
        }

        [Fact]
        public void ShouldParseUri()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            const string source = "https://github.com/thomasgalliker/CameraScanner.Maui";

            // Act
            var result = barcodeParser.Parse<URIParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.Uri.Should().Be(source);
        }

        [Fact]
        public void ShouldParseWifi()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            const string source = "WIFI:S:TESTWIFI;T:WPA;P:TESTPASSWORD;;";

            // Act
            var result = barcodeParser.Parse<WifiParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.Ssid.Should().Be("TESTWIFI");
            result.Password.Should().Be("TESTPASSWORD");
            result.NetworkEncryption.Should().Be("WPA");
            result.Hidden.Should().BeFalse();
        }

        [Fact]
        public void ShouldParseTel()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            const string source = "TEL:00418008080";

            // Act
            var result = barcodeParser.Parse<TelParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.Number.Should().Be("00418008080");
            result.TelUri.Should().Be(new Uri("tel:00418008080"));
        }

        [Fact]
        public void ShouldParseISBN()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            const string source = "978-0-13-235088-4";

            // Act
            var result = barcodeParser.Parse<ISBNParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.ISBN.Should().Be(source);
        }

        [Fact]
        public void ShouldParseEmail()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            const string source = "mailto:email@test.com?subject=Subject&body=Body";

            // Act
            var result = barcodeParser.Parse<EmailParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.Tos.Should().Contain("email@test.com");
            result.Subject.Should().Contain("Subject");
            result.Body.Should().Contain("Body");
        }
    }
}