﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MyerSplash.Data
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpr = exp.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException("propertyExpression should represent access to a member");
            var memberName = memberExpr.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
    }
}