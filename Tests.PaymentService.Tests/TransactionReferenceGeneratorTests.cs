using Modules.PaymentService.Infrastructure.Helpers;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for TransactionReferenceGenerator.
    /// </summary>
    public class TransactionReferenceGeneratorTests
    {
        [Fact]
        public void Generate_WithDefaultPrefix_ReturnsValidReference()
        {
            // Act
            var reference = TransactionReferenceGenerator.Generate();

            // Assert
            Assert.NotNull(reference);
            Assert.StartsWith("TXN-", reference);
            Assert.True(TransactionReferenceGenerator.IsValid(reference));
        }

        [Fact]
        public void Generate_WithCustomPrefix_ReturnsReferenceWithPrefix()
        {
            // Arrange
            var prefix = "PAY";

            // Act
            var reference = TransactionReferenceGenerator.Generate(prefix);

            // Assert
            Assert.StartsWith("PAY-", reference);
            Assert.True(TransactionReferenceGenerator.IsValid(reference));
        }

        [Fact]
        public void Generate_CalledMultipleTimes_ReturnsUniqueReferences()
        {
            // Act
            var reference1 = TransactionReferenceGenerator.Generate();
            var reference2 = TransactionReferenceGenerator.Generate();
            var reference3 = TransactionReferenceGenerator.Generate();

            // Assert
            Assert.NotEqual(reference1, reference2);
            Assert.NotEqual(reference2, reference3);
            Assert.NotEqual(reference1, reference3);
        }

        [Fact]
        public void GenerateForEvent_WithEventId_ReturnsEventSpecificReference()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            // Act
            var reference = TransactionReferenceGenerator.GenerateForEvent(eventId);

            // Assert
            Assert.NotNull(reference);
            Assert.StartsWith("EVT-", reference);
            Assert.Contains(eventId.ToString("N")[..8].ToUpper(), reference);
            Assert.True(TransactionReferenceGenerator.IsValid(reference));
        }

        [Fact]
        public void GenerateWithTimestamp_IncludesTimestampInFormat()
        {
            // Act
            var reference = TransactionReferenceGenerator.GenerateWithTimestamp("TS");

            // Assert
            Assert.StartsWith("TS-", reference);
            Assert.True(TransactionReferenceGenerator.IsValid(reference));
            
            // Extract date from reference
            var date = TransactionReferenceGenerator.ExtractDate(reference);
            Assert.NotNull(date);
            Assert.Equal(DateTime.UtcNow.Date, date.Value.Date);
        }

        [Fact]
        public void GenerateSequential_CreatesIncrementingReferences()
        {
            // Act
            var ref1 = TransactionReferenceGenerator.GenerateSequential();
            var ref2 = TransactionReferenceGenerator.GenerateSequential();
            var ref3 = TransactionReferenceGenerator.GenerateSequential();

            // Assert - References should be different and contain SEQ
            Assert.Contains("SEQ", ref1);
            Assert.Contains("SEQ", ref2);
            Assert.Contains("SEQ", ref3);
            Assert.NotEqual(ref1, ref2);
            Assert.NotEqual(ref2, ref3);
        }

        [Fact]
        public void GenerateIdempotencyKey_WithSameInput_ReturnsSameKey()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var amount = 5000.00m;
            var currency = "NGN";
            var timestamp = DateTime.UtcNow;

            // Act
            var key1 = TransactionReferenceGenerator.GenerateIdempotencyKey(userId, amount, currency, timestamp);
            var key2 = TransactionReferenceGenerator.GenerateIdempotencyKey(userId, amount, currency, timestamp);

            // Assert
            Assert.Equal(key1, key2);
        }

        [Fact]
        public void GenerateIdempotencyKey_WithDifferentInput_ReturnsDifferentKeys()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var amount1 = 5000.00m;
            var amount2 = 6000.00m;
            var currency = "NGN";
            var timestamp = DateTime.UtcNow;

            // Act
            var key1 = TransactionReferenceGenerator.GenerateIdempotencyKey(userId, amount1, currency, timestamp);
            var key2 = TransactionReferenceGenerator.GenerateIdempotencyKey(userId, amount2, currency, timestamp);

            // Assert
            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void GenerateIdempotencyKey_ReturnsUrlSafeString()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var amount = 5000.00m;
            var currency = "NGN";
            var timestamp = DateTime.UtcNow;

            // Act
            var key = TransactionReferenceGenerator.GenerateIdempotencyKey(userId, amount, currency, timestamp);

            // Assert
            Assert.DoesNotContain("+", key);
            Assert.DoesNotContain("/", key);
            Assert.DoesNotContain("=", key);
        }

        [Theory]
        [InlineData("TXN-20240115-ABC123", true)]
        [InlineData("PAY-20240115-XYZ789", true)]
        [InlineData("EVT-12345678-20240115-ABCD1234", true)]
        [InlineData("TS-20240115123045-ABC12345", true)]
        [InlineData("", false)]
        [InlineData("INVALID", false)]
        [InlineData("TXN-INVALID-ABC", false)]
        [InlineData("TXN", false)]
        [InlineData("  ", false)]
        public void IsValid_WithVariousFormats_ReturnsExpectedResult(string? reference, bool expected)
        {
            // Act
            var isValid = TransactionReferenceGenerator.IsValid(reference);

            // Assert
            Assert.Equal(expected, isValid);
        }

        [Fact]
        public void ExtractDate_WithValidReference_ReturnsCorrectDate()
        {
            // Arrange
            var expectedDate = new DateTime(2024, 1, 15);
            var reference = "TXN-20240115-ABC123";

            // Act
            var extractedDate = TransactionReferenceGenerator.ExtractDate(reference);

            // Assert
            Assert.NotNull(extractedDate);
            Assert.Equal(expectedDate, extractedDate.Value);
        }

        [Fact]
        public void ExtractDate_WithTimestampReference_ReturnsCorrectDateTime()
        {
            // Arrange
            var expectedDateTime = new DateTime(2024, 1, 15, 12, 30, 45);
            var reference = "TS-20240115123045-ABC12345";

            // Act
            var extractedDate = TransactionReferenceGenerator.ExtractDate(reference);

            // Assert
            Assert.NotNull(extractedDate);
            Assert.Equal(expectedDateTime, extractedDate.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("INVALID")]
        [InlineData("TXN-INVALID-ABC")]
        [InlineData("  ")]
        public void ExtractDate_WithInvalidReference_ReturnsNull(string? reference)
        {
            // Act
            var extractedDate = TransactionReferenceGenerator.ExtractDate(reference);

            // Assert
            Assert.Null(extractedDate);
        }

        [Fact]
        public void GenerateForEvent_WithMultipleEventIds_ReturnsUniqueReferences()
        {
            // Arrange
            var eventId1 = Guid.NewGuid();
            var eventId2 = Guid.NewGuid();

            // Act
            var ref1 = TransactionReferenceGenerator.GenerateForEvent(eventId1);
            var ref2 = TransactionReferenceGenerator.GenerateForEvent(eventId2);

            // Assert
            Assert.NotEqual(ref1, ref2);
        }

        [Fact]
        public void Generate_ContainsCurrentDate()
        {
            // Arrange
            var expectedDateString = DateTime.UtcNow.ToString("yyyyMMdd");

            // Act
            var reference = TransactionReferenceGenerator.Generate();

            // Assert
            Assert.Contains(expectedDateString, reference);
        }

        [Fact]
        public async Task GenerateSequential_IsThreadSafe()
        {
            // Arrange
            var references = new System.Collections.Concurrent.ConcurrentBag<string>();
            var tasks = new List<Task>();

            // Act - Generate references concurrently
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var reference = TransactionReferenceGenerator.GenerateSequential();
                    references.Add(reference);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert - All references should be unique
            var uniqueReferences = references.Distinct().Count();
            Assert.Equal(100, uniqueReferences);
        }
    }
}

