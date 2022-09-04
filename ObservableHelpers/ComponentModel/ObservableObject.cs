// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file is inspired from the MvvmLight library (lbugnion/MvvmLight),
// more info in ThirdPartyNotices.txt in the root of the project.

// ================================== NOTE ==================================
// This file is mirrored in the trimmed-down INotifyPropertyChanged file in
// the source generator project, to be used with the [INotifyPropertyChanged],
// attribute, along with the ObservableObject annotated copy (for debugging info).
// If any changes are made to this file, they should also be appropriately
// ported to that file as well to keep the behavior consistent.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ObservableHelpers.ComponentModel.__Internals;

#pragma warning disable CS0618

namespace ObservableHelpers.ComponentModel;

/// <summary>
/// A base class for objects of which the properties must be observable.
/// </summary>
[ObservableObject]
public abstract partial class ObservableObject { }
