// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ObservableHelpers.SourceGenerators.Extensions;
using ObservableHelpers.SourceGenerators.Helpers;

namespace ObservableHelpers.SourceGenerators.Input.Models;

/// <summary>
/// A model with gathered info on a given command method.
/// </summary>
/// <param name="MethodName">The name of the target method.</param>
/// <param name="FieldName">The resulting field name for the generated command.</param>
/// <param name="PropertyName">The resulting property name for the generated command.</param>
/// <param name="CommandInterfaceType">The command interface type name.</param>
/// <param name="CommandClassType">The command class type name.</param>
/// <param name="DelegateType">The delegate type name for the wrapped method.</param>
/// <param name="CommandTypeArguments">The type arguments for <paramref name="CommandInterfaceType"/> and <paramref name="CommandClassType"/>, if any.</param>
/// <param name="DelegateTypeArguments">The type arguments for <paramref name="DelegateType"/>, if any.</param>
/// <param name="CanExecuteMemberName">The member name for the can execute check, if available.</param>
/// <param name="CanExecuteExpressionType">The can execute expression type, if available.</param>
/// <param name="AllowConcurrentExecutions">Whether or not concurrent executions have been enabled.</param>
/// <param name="FlowExceptionsToTaskScheduler">Whether or not exceptions should flow to the task scheduler.</param>
/// <param name="IncludeCancelCommand">Whether or not to also generate a cancel command.</param>
internal sealed record CommandInfo(
    string MethodName,
    string FieldName,
    string PropertyName,
    string CommandInterfaceType,
    string CommandClassType,
    string DelegateType,
    ImmutableArray<string> CommandTypeArguments,
    ImmutableArray<string> DelegateTypeArguments,
    string? CanExecuteMemberName,
    CanExecuteExpressionType? CanExecuteExpressionType,
    bool AllowConcurrentExecutions,
    bool FlowExceptionsToTaskScheduler,
    bool IncludeCancelCommand)
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="CommandInfo"/>.
    /// </summary>
    public sealed class Comparer : Comparer<CommandInfo, Comparer>
    {
        /// <inheritdoc/>
        protected override void AddToHashCode(ref HashCode hashCode, CommandInfo obj)
        {
            hashCode.Add(obj.MethodName);
            hashCode.Add(obj.FieldName);
            hashCode.Add(obj.PropertyName);
            hashCode.Add(obj.CommandInterfaceType);
            hashCode.Add(obj.CommandClassType);
            hashCode.Add(obj.DelegateType);
            hashCode.AddRange(obj.CommandTypeArguments);
            hashCode.AddRange(obj.DelegateTypeArguments);
            hashCode.Add(obj.CanExecuteMemberName);
            hashCode.Add(obj.CanExecuteExpressionType);
            hashCode.Add(obj.AllowConcurrentExecutions);
            hashCode.Add(obj.FlowExceptionsToTaskScheduler);
            hashCode.Add(obj.IncludeCancelCommand);
        }

        /// <inheritdoc/>
        protected override bool AreEqual(CommandInfo x, CommandInfo y)
        {
            return
                x.MethodName == y.MethodName &&
                x.FieldName == y.FieldName &&
                x.PropertyName == y.PropertyName &&
                x.CommandInterfaceType == y.CommandInterfaceType &&
                x.CommandClassType == y.CommandClassType &&
                x.DelegateType == y.DelegateType &&
                x.CommandTypeArguments.SequenceEqual(y.CommandTypeArguments) &&
                x.DelegateTypeArguments.SequenceEqual(y.CommandTypeArguments) &&
                x.CanExecuteMemberName == y.CanExecuteMemberName &&
                x.CanExecuteExpressionType == y.CanExecuteExpressionType &&
                x.AllowConcurrentExecutions == y.AllowConcurrentExecutions &&
                x.FlowExceptionsToTaskScheduler == y.FlowExceptionsToTaskScheduler &&
                x.IncludeCancelCommand == y.IncludeCancelCommand;
        }
    }
}
