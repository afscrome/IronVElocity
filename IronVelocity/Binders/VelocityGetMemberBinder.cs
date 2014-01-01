﻿using IronVelocity;
using IronVelocity.RuntimeHelpers;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VelocityExpressionTree.Binders
{
    public class VelocityGetMemberBinder : GetMemberBinder
    {
        public VelocityGetMemberBinder(string name)
            : base(name, ignoreCase: true)
        {
        }

        //TODO: Should we allow binding to static members?

        private MemberInfo GetMember(Type type, bool caseSensitive)
        {
            var flags = caseSensitive
                ? BindingFlags.Public | BindingFlags.Instance
                : BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var property = type.GetProperty(Name, flags);

            if (caseSensitive && property != null)
                return property;

            var field = type.GetField(Name, flags);

            if (property != null && field != null)
                throw new AmbiguousMatchException();

            if (property != null)
                return property;
            else
                return field;

        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            //If any of the Dynamic Meta Objects don't yet have a value, defer until they have values.  Failure to do this may result in an infinite loop
            if (!target.HasValue)
                return Defer(target);

            // If the target has a null value, then we won't be able to get any fields or properties, so escape early
            // Failure to escape early like this do this results in an infinite loop
            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
            }

            //NVelocity has two special case 'properties' for use on identifiers
            Expression result = null;
            var targetType = target.LimitType;
            if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal))
            {
                if (Name.Equals("to_quote", StringComparison.OrdinalIgnoreCase))
                    result = VelocityStrings.EscapeDoubleQuote(target.Expression);
                else if (Name.Equals("to_squote", StringComparison.OrdinalIgnoreCase))
                    result = VelocityStrings.EscapeSingleQuote(target.Expression);

            }
            
            if (result == null)
            {
                MemberInfo member = null;
                try
                {
                    member = GetMember(target.LimitType, false);
                }
                catch (AmbiguousMatchException)
                {
                    try
                    {
                        member = GetMember(target.LimitType, true);
                    }
                    catch (AmbiguousMatchException)
                    {
                        Debug.WriteLine(string.Format("Ambiguous match for member '{0}' on type '{1}'", Name, target.LimitType.AssemblyQualifiedName), "Velocity");

                    }
                }


                if (member == null)
                {
                    //If no matching property or field, fall back to indexer with string param
                    var indexer = target.LimitType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, new[] { typeof(string) }, null);
                    if (indexer == null)
                    {
                        Debug.WriteLine(string.Format("Unable to resolve Property '{0}' on type '{1}'", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                        result = Constants.VelocityUnresolvableResult;
                    }
                    else
                    {
                        result = Expression.MakeIndex(
                                VelocityExpressions.ConvertIfNeeded(target, indexer),
                                (PropertyInfo)indexer,
                                new[] { Expression.Constant(Name) }
                            );
                    }
                }
                else
                {

                    if (member is PropertyInfo)
                    {
                        result = Expression.Property(
                                VelocityExpressions.ConvertIfNeeded(target, member),
                                (PropertyInfo)member
                            );
                    }
                    else if (member is FieldInfo)
                    {
                        result = Expression.Field(
                                VelocityExpressions.ConvertIfNeeded(target, member),
                                (FieldInfo)member
                            );
                    }
                    else
                    {
                        throw new InvalidProgramException();
                    }

                }
            }

            //Dynamic return type is object, but primitives are not objects
            // DLR does not handle boxing to make primitives objects, so do it ourselves
            result = VelocityExpressions.BoxIfNeeded(result);

            return new DynamicMetaObject(
                result,
                BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
            );
        }
    }

}
