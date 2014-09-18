﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Binders
{
    public class BinderHelper : IBinderHelper
    {
        static BinderHelper()
        {
            Instance = new BinderHelper();
        }

        public static BinderHelper Instance { get; set; }

        private readonly ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder> _invokeMemberBinders = new ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder>();
        private readonly ConcurrentDictionary<string, GetMemberBinder> _getMemberBinders = new ConcurrentDictionary<string, GetMemberBinder>();
        private readonly ConcurrentDictionary<string, SetMemberBinder> _setMemberBinders = new ConcurrentDictionary<string, SetMemberBinder>();
        private readonly ConcurrentDictionary<ExpressionType, VelocityBinaryMathematicalOperationBinder> _mathsBinders = new ConcurrentDictionary<ExpressionType, VelocityBinaryMathematicalOperationBinder>();
        private readonly ConcurrentDictionary<LogicalOperation, VelocityBinaryLogicalOperationBinder> _logicalBinders = new ConcurrentDictionary<LogicalOperation, VelocityBinaryLogicalOperationBinder>();

        public InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount)
        {
            return _invokeMemberBinders.GetOrAdd(
                new Tuple<string, int>(name, argumentCount),
                (key) => { return CreateInvokeMemberBinder(key.Item1, key.Item2); }
            );
        }

        public GetMemberBinder GetGetMemberBinder(string memberName)
        {
            return _getMemberBinders.GetOrAdd(memberName, CreateGetMemberBinder);
        }

        public SetMemberBinder GetSetMemberBinder(string memberName)
        {
            return _setMemberBinders.GetOrAdd(memberName, CreateSetMemberBinder);
        }

        public VelocityBinaryLogicalOperationBinder GetBinaryLogicalOperationBinder(LogicalOperation operation)
        {
            return _logicalBinders.GetOrAdd(operation, CreateVelocityBinaryLogicalOperationBinder);
        }

        public VelocityBinaryMathematicalOperationBinder GetBinaryMathematicalOperationBinder(ExpressionType type)
        {
            return _mathsBinders.GetOrAdd(type, CreateVelocityBinaryMathematicalOperationBinder);
        }


        protected virtual InvokeMemberBinder CreateInvokeMemberBinder(string name, int argumentCount)
        {
            return new VelocityInvokeMemberBinder(name, new CallInfo(argumentCount));
        }

        protected virtual GetMemberBinder CreateGetMemberBinder(string memberName)
        {
            return new VelocityGetMemberBinder(memberName);
        }

        protected virtual SetMemberBinder CreateSetMemberBinder(string memberName)
        {
            return new VelocitySetMemberBinder(memberName);
        }

        protected virtual VelocityBinaryMathematicalOperationBinder CreateVelocityBinaryMathematicalOperationBinder(ExpressionType type)
        {
            return new VelocityBinaryMathematicalOperationBinder(type);
        }
        protected virtual VelocityBinaryLogicalOperationBinder CreateVelocityBinaryLogicalOperationBinder(LogicalOperation operation)
        {
            return new VelocityBinaryLogicalOperationBinder(operation);
        }



    }

    public interface IBinderHelper
    {
        InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount);
        GetMemberBinder GetGetMemberBinder(string memberName);
        SetMemberBinder GetSetMemberBinder(string memberName);
        VelocityBinaryLogicalOperationBinder GetBinaryLogicalOperationBinder(LogicalOperation operation);
        VelocityBinaryMathematicalOperationBinder GetBinaryMathematicalOperationBinder(ExpressionType type);
    }
}
