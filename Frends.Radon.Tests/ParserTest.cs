using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    public class ParserTest
    {
        [Test]
        public void TryParseInt_ShouldReturnCorrectResult()
        {
            var result = Parser.TryParseInt("123", -1);

            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void TryParseInt_ShouldReturnDefaultValueIfParsingFails()
        {
            var result = Parser.TryParseInt("abc", 123);

            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void TryParseBool_ShouldReturnCorrectResult()
        {
            var result = Parser.TryParseBool("true", false);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TryParseBool_ShouldReturnDefaultValueIfParsingFails()
        {
            var result = Parser.TryParseBool("abc", false);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryParseTimeSpan_ShouldReturnCorrectResult()
        {
            var result = Parser.TryParseTimeSpan("2:00:00", TimeSpan.FromMinutes(1));

            Assert.That(result, Is.EqualTo(TimeSpan.Parse("2:00:00")));
        }

        [Test]
        public void TryParseTimeSpan_ShouldReturnDefaultValueIfParsingFails()
        {
            var result = Parser.TryParseTimeSpan("abc", TimeSpan.FromMinutes(1));

            Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(1)));
        }

        [TestCase(null)]
        [TestCase("")]
        public void ParseRecipientsToList_ShouldReturnAnEmptyListIfRecipientsIsInvalid(string recipients)
        {
            var result = Parser.ParseRecipientsToList(recipients);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParseRecipientsToList_ShouldParseRecipientsCorrectly()
        {
            var result = Parser.ParseRecipientsToList("mikael;jouko");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo("mikael"));
            Assert.That(result[1], Is.EqualTo("jouko"));
        }

        [Test]
        public void ParseRecipientsToList_ShouldTrimRecipients()
        {
            var result = Parser.ParseRecipientsToList("     mikael   ;     jouko    ");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo("mikael"));
            Assert.That(result[1], Is.EqualTo("jouko"));
        }

        [Test]
        public void ParseRecipientsToList_ShouldRemoveEmptyEntries()
        {
            var result = Parser.ParseRecipientsToList("mikael;;;;jouko");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo("mikael"));
            Assert.That(result[1], Is.EqualTo("jouko"));
        }

        [Test]
        public void ParseRecipientsToString_ShouldReturnCorrectRecipients()
        {
            var result = Parser.ParseRecipientsToString(new List<string> {"mikael", "jouko"});

            Assert.That(result, Is.EqualTo("mikael;jouko"));
        }

        [Test]
        public void ParseRecipientsToString_ShouldTrimRecipients()
        {
            var result = Parser.ParseRecipientsToString(new List<string> { "       mikael      ", "   jouko   " });

            Assert.That(result, Is.EqualTo("mikael;jouko"));
        }

        [Test]
        public void ParseRecipientsToString_ShouldRemoveEmptyEntries()
        {
            var result = Parser.ParseRecipientsToString(new List<string> { "mikael", null, string.Empty, "jouko" });

            Assert.That(result, Is.EqualTo("mikael;jouko"));
        }

        [Test]
        public void ParseRecipientsToString_ShouldRemoveWhitespaceEntries()
        {
            var result = Parser.ParseRecipientsToString(new List<string> { "mikael", "   ", "             ", "jouko" });

            Assert.That(result, Is.EqualTo("mikael;jouko"));
        }

        [Test]
        public void TrimLineOrReturnStringEmptyIfNull_ShouldReturnCorrectValue()
        {
            var result = Parser.TrimLineOrReturnStringEmptyIfNull("mikael");

            Assert.That(result, Is.EqualTo("mikael"));
        }

        [Test]
        public void TrimLineOrReturnStringEmptyIfNull_ShouldReturnStringEmptyIfValueNull()
        {
            var result = Parser.TrimLineOrReturnStringEmptyIfNull(null);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TrimLineOrReturnStringEmptyIfNull_ShouldTrimLine()
        {
            var result = Parser.TrimLineOrReturnStringEmptyIfNull("    mikael    ");

            Assert.That(result, Is.EqualTo("mikael"));
        }
    }
}
