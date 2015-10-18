﻿using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class AndExpressionTests : ParserTestBase
    {
        [TestCase("$x&&$y", "$x", "$y")]
        [TestCase(" false && true ", "false", "true")]
        [TestCase("${x}and${y} ", "${x}", "${y}")]
        [TestCase(" false and true ", "false", "true")]
        public void ParseBinaryAndExpressionTests(string input, string left, string right)
        {
            ParseBinaryExpressionTest(input, left, right, VelocityLexer.AND, x => x.andExpression());
        }

        [Test]
        public void ParseTernaryAndExpressionTests()
        {
            var input = "$a && $b and $c";
            ParseTernaryExpressionWithEqualPrecedenceTest(input, VelocityLexer.AND, VelocityLexer.AND, x => x.andExpression());
        }
    }
}
