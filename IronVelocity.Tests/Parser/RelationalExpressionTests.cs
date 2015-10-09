﻿using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class RelationalExpressionTests : ParserTestBase
    {
        [TestCase(">",  VelocityLexer.GREATERTHAN)]
        [TestCase("<",  VelocityLexer.LESSTHAN)]
        [TestCase(">=",  VelocityLexer.GREATERTHANOREQUAL)]
        [TestCase("<=",  VelocityLexer.LESSTHANOREQUAL)]
        [TestCase("gt",  VelocityLexer.GREATERTHAN)]
        [TestCase("lt",  VelocityLexer.LESSTHAN)]
        [TestCase("ge",  VelocityLexer.GREATERTHANOREQUAL)]
        [TestCase("le",  VelocityLexer.LESSTHANOREQUAL)]
        public void ParseBinaryRelationalExpressionTests(string @operator, int operatorTokenKind)
        {
            var left = "$left";
            var right = "$right";
            var input = $"{left} {@operator} {right}";

            ParseBinaryExpressionTest(input, left, right, operatorTokenKind, x => x.relational_expression());
        }
    }
}
