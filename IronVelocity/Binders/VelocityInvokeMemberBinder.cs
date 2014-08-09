﻿using IronVelocity.Compilation;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace IronVelocity.Binders
{
    public class VelocityInvokeMemberBinder : InvokeMemberBinder
    {
        private readonly bool _supportAsync = true;

        public VelocityInvokeMemberBinder(String name, CallInfo callInfo)
            : base(name, true, callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            //Don't support static invocation
            return errorSuggestion;
        }


        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (args == null)
                throw new ArgumentNullException("args");

            //TODO: Support dictionary --> arguments
            //TODO: Support optional params?  BindingFlags.OptionalParamBinding

            //If any of the Dynamic Meta Objects don't yet have a value, defer until they have values.  Failure to do this may result in an infinite loop
            if (!target.HasValue)
                return Defer(target);

            // If the target has a null value, then we won't be able to get any fields or properties, so escape early
            // Failure to escape early like this results in an infinite loop
            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
            }

            // If an argument has a null value, use a null type so that the resolution algorithm can do implicit null conversions
            var argTypeArray = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
			{
                var arg = args[i];
    			 argTypeArray[i] = arg.Value == null
                     ? null
                     : arg.LimitType;
			}

            MethodInfo method;
            Expression result;
            try
            {
                method = ReflectionHelper.ResolveMethod(target.LimitType, Name, argTypeArray);
            }
            catch (AmbiguousMatchException)
            {
                method = null;
            }

            if (method == null)
            {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to resolve method '{0}' on type '{1}'", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                result = Constants.VelocityUnresolvableResult;
            }
            else
            {
                result = ReflectionHelper.ConvertMethodParameters(method, target.Expression, args);

                //Not keen on returning empty string, but this maintains consistency with NVelocity.
                // Otherwise returning void fails with an exception because the DLR can't convert 
                // Returning null causes problems as null indicates the method call failed, and so
                // causes the Identifier to be emitted instead of blank.


                //Dynamic return type is object, but primitives are not objects
                // DLR does not handle boxing to make primitives objects, so do it ourselves
            }

            return new DynamicMetaObject(
                result,
                BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
            );
        }

    }
}
