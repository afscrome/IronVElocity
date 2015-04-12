﻿using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class LiteralDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        public override string Name { get { return "literal"; } }

        public override Expression Build(ASTDirective node, NVelocityNodeToExpressionConverter converter)
        {
            return new LiteralDirective(node, converter);
        }
    }
}
