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
        public void ShouldClearParsers()
        {
            // Arrange
            var callCount = 0;
            var barcodeParser = new BarcodeParser();
            barcodeParser.Parsers.Clear();
            barcodeParser.Parsers.Add(new DelegateResultParser(s =>
            {
                callCount++;
                return null;
            }));

            const string source = "tel:00418008080";

            // Act
            var result = barcodeParser.Parse(source);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<TextParsedResult>();

            callCount.Should().Be(1);
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

        [Fact]
        public void ShouldParseEmediplan()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            const string source = "CHMED23A.H4sIAAAAAAAACq2OOw4CMQxE7zIt2ZUTAmzcLZsGiU+KUCEKYKlokIACRbk7jkLBAWisZz/NyAmb6/gAHxJWI7hsGgqhnsIOnBDBRmF4+9cebCuBtUL0Xy38g73MnIu+DxX/1nRUkCRiv1zLl9tzOF1uIloqxj9FGTKmId1oHcnxtGM7a+28c9YtJqSZCPkD+iD8fPQAAAA=";

            // Act
            var result = barcodeParser.Parse<EmediplanParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.ReleaseYear.Should().Be("23");
            result.SubVersion.Should().Be("A");
            result.AdditionalMetaData.Should().Be(".H4sIAAAAAAAACq2OOw4CMQxE7zIt2ZUTAmzcLZsGiU+KUCEKYKlokIACRbk7jkLBAWisZz/NyAmb6/gAHxJWI7hsGgqhnsIOnBDBRmF4+9cebCuBtUL0Xy38g73MnIu+DxX/1nRUkCRiv1zLl9tzOF1uIloqxj9FGTKmId1oHcnxtGM7a+28c9YtJqSZCPkD+iD8fPQAAAA=");
            result.Data.Should().Be("");
        }

        [Fact]
        public void ShouldParseGeo()
        {
            // Arrange
            var barcodeParser = new BarcodeParser();
            const string source = "geo:37.7749,-122.4194,50.0?layer=traffic";

            // Act
            var result = barcodeParser.Parse<GeoParsedResult>(source);

            // Assert
            result.Should().NotBeNull();
            result.Latitude.Should().Be(37.7749d);
            result.Longitude.Should().Be(-122.4194d);
            result.Altitude.Should().Be(50d);
            result.Query.Should().Be("layer=traffic");
        }
    }

    public class DelegateResultParser : ResultParser
    {
        private readonly Func<string, ParsedResult> parseAction;

        public DelegateResultParser(Func<string, ParsedResult> parseAction)
        {
            this.parseAction = parseAction;
        }
        public override ParsedResult Parse(string source)
        {
            return this.parseAction(source);
        }
    }
}