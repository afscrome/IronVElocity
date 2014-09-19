﻿using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class RenderedBlock : VelocityExpression
    {
        private readonly ParameterExpression _output;
        public override Type Type { get { return typeof(void); } }
        
        public RenderedBlock(IEnumerable<Expression> expressions, VelocityExpressionBuilder builder)
        {
            if (expressions == null)
                throw new ArgumentNullException("expressions");

            if (builder == null)
                throw new ArgumentNullException("builder");

            Children = expressions.ToList();
            _output = builder.OutputParameter;
        }


        public IReadOnlyCollection<Expression> Children { get; private set; }

        public override Expression Reduce()
        {
            if (!Children.Any())
                return Constants.EmptyExpression;

            var convertedExpressions = Children
                .Select(Output);

            return Expression.Block(typeof(void), convertedExpressions);
        }

        private Expression Output(Expression expression)
        {
            if (expression.Type == typeof(void))
            {
                return expression;
            }

            var reference = expression as ReferenceExpression;
            if (reference != null)
                expression = new RenderableVelocityReference(reference);


            var method = expression.Type == typeof(string)
                ? MethodHelpers.AppendStringMethodInfo
                : MethodHelpers.AppendMethodInfo;

            return Expression.Call(_output, method, expression);
        }



    }
}
