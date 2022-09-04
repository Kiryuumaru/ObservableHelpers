// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Linq;
using ObservableHelpers.SourceGenerators.Diagnostics;
using ObservableHelpers.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static ObservableHelpers.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ObservableHelpers.SourceGenerators;

/// <summary>
/// A source generator for the <c>ObservableObjectAttribute</c> type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ObservableObjectGenerator : TransitiveMembersGenerator<object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableObjectGenerator"/> class.
    /// </summary>
    public ObservableObjectGenerator()
        : base("global::ObservableHelpers.ComponentModel.ObservableObjectAttribute")
    {
    }

    /// <inheritdoc/>
    protected override IncrementalValuesProvider<(INamedTypeSymbol Symbol, object? Info)> GetInfo(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(INamedTypeSymbol Symbol, AttributeData AttributeData)> source)
    {
        return source.Select(static (item, _) => (item.Symbol, (object?)null));
    }

    /// <inheritdoc/>
    protected override bool ValidateTargetType(INamedTypeSymbol typeSymbol, object? info, out ImmutableArray<Diagnostic> diagnostics)
    {
        ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();

        // Check if the type already implements INotifyPropertyChanged...
        if (typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedName("global::System.ComponentModel.INotifyPropertyChanged")))
        {
            builder.Add(DuplicateINotifyPropertyChangedInterfaceForObservableObjectAttributeError, typeSymbol, typeSymbol);

            diagnostics = builder.ToImmutable();

            return false;
        }

        // ...or INotifyPropertyChanging
        if (typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedName("global::System.ComponentModel.INotifyPropertyChanging")))
        {
            builder.Add(DuplicateINotifyPropertyChangingInterfaceForObservableObjectAttributeError, typeSymbol, typeSymbol);

            diagnostics = builder.ToImmutable();

            return false;
        }

        // Check if the type uses [INotifyPropertyChanged] or [ObservableObject] already (in the type hierarchy too)
        if (typeSymbol.InheritsAttributeWithFullyQualifiedName("global::ObservableHelpers.ComponentModel.ObservableObjectAttribute") ||
            typeSymbol.HasOrInheritsAttributeWithFullyQualifiedName("global::ObservableHelpers.ComponentModel.INotifyPropertyChangedAttribute"))
        {
            builder.Add(InvalidAttributeCombinationForObservableObjectAttributeError, typeSymbol, typeSymbol);

            diagnostics = builder.ToImmutable();

            return false;
        }

        diagnostics = builder.ToImmutable();

        return true;
    }

    /// <inheritdoc/>
    protected override ImmutableArray<MemberDeclarationSyntax> FilterDeclaredMembers(object? info, ImmutableArray<MemberDeclarationSyntax> memberDeclarations)
    {
        return memberDeclarations;
    }
}
