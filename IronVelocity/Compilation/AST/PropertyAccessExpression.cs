﻿using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class PropertyAccessExpression : VelocityExpression
    {
        public Expression Target { get; private set; }
        public string Name { get; private set; }

        public PropertyAccessExpression(Expression target, string name, SymbolInformation symbolInformation)
        {
            Target = target;
            Name = name;
            Symbols = symbolInformation;
        }

        public override Expression Reduce()
        {
            return Expression.Dynamic(
                BinderHelper.Instance.GetGetMemberBinder(Name),
                typeof(object),
                Target
            );
        }

        public PropertyAccessExpression Update(Expression target)
        {
            if (target == Target)
                return this;

            return new PropertyAccessExpression(target, Name, Symbols);
        }

    }
}
