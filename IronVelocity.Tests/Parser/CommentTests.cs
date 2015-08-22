﻿using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    public class CommentTests : ParserTestBase
    {
        [TestCase("##")]
        [TestCase("##Comment")]
        [TestCase("##$reference")]
        [TestCase("##set")]
        [TestCase("##Comment\r")]
        [TestCase("##Comment\n")]
        [TestCase("##Comment\r\n")]
        public void ParseSingleLineComment(string input)
        {
            var expectedCommentText = input.TrimEnd('\r', '\n');
            var comment = CreateParser(input).comment();

            Assert.That(comment, Is.Not.Null);
            Assert.That(comment.GetText(), Is.EqualTo(expectedCommentText));
        }

        [TestCase("#**#")]
        [TestCase("#*Multi Line*#")]
        [TestCase("#*Multi \r Line*#")]
        [TestCase("#*Multi \n Line*#")]
        [TestCase("#*Multi \r\n Line*#")]
        public void ParseMultiLineComment(string input) => ParseBlockComment(input);

        [TestCase("#**Formal*#")]
        [TestCase("#**Formal\r\nComment*#")]
        public void ParseFormalComment(string input) => ParseBlockComment(input);

        [TestCase("#*Outer #*Nested*# Outer*#")]
        [TestCase("#**Outer Formal #* Nested Informal *# Outer*#")]
        [TestCase("#*Outer Informal #** Nested Formal *# Outer*#")]
        [TestCase("#*#**#*#")]
        [TestCase("#*  #**#*#")]
        [TestCase("#*#**#   *#")]
        [TestCase("#* #*1*#  *#")]
        public void ParseNestedComments(string input) => ParseBlockComment(input);


        public void ParseBlockComment(string input)
        {
            var comment = CreateParser(input).comment();

            Assert.That(comment, Is.Not.Null);
            Assert.That(comment.GetText(), Is.EqualTo(input));
        }
    }
}
