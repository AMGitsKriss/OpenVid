using NUnit.Framework;
using Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("tag:testtag", 1)]
        [TestCase("tah:testtag", 0)]
        [TestCase("-tag:testtag", 1)]
        [TestCase("-tah:testtag", 0)]
        [TestCase("-tag:testtag tah:testtag", 1)]
        [TestCase("-tag:testtag tag:food", 2)]
        [TestCase("-tag:testtag tag:food minduration:0:1:0", 3)]
        [TestCase("-tag:testtag tag:food minduration:0:1:0 order:size", 4)]
        public void SearchParsingTest(string searchString, int expectedCount)
        {
            SearchService service = new SearchService(null, null);
            var searchParams = service.MapSearchQueryToParameters(searchString);
            Assert.AreEqual(expectedCount, searchParams.Count());
        }
    }
}