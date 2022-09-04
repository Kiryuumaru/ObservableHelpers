namespace ObservableHelpers.ComponentModel.Enums;

/// <summary>
/// Use the access modifiers, public, protected, internal, or private, to specify one of the following declared accessibility levels for members.
/// </summary>
public enum AccessModifier
{
    #region PropertyAccess

    /// <summary>
    /// Access is not restricted.
    /// </summary>
    Public = 1,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class.
    /// </summary>
    Protected = 2,

    /// <summary>
    /// Access is limited to the current assembly.
    /// </summary>
    Internal = 3,

    /// <summary>
    /// Access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    ProtectedInternal = 4,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    PrivateProtected = 5,

    /// <summary>
    /// Access is limited to the containing type.
    /// </summary>
    Private = 6,

    #endregion

    #region GetterAccess

    // Public Getters

    /// <summary>
    /// Access is not restricted. With getter access is limited to the containing class or types derived from the containing class.
    /// </summary>
    PublicWithProtectedGetter = 7,

    /// <summary>
    /// Access is not restricted. With getter access is limited to the current assembly.
    /// </summary>
    PublicWithInternalGetter = 8,

    /// <summary>
    /// Access is not restricted. With getter access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    PublicWithProtectedInternalGetter = 9,

    /// <summary>
    /// Access is not restricted. With getter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    PublicWithPrivateProtectedGetter = 10,

    /// <summary>
    /// Access is not restricted. With getter access is limited to the containing type.
    /// </summary>
    PublicWithPrivateGetter = 11,

    // Protected Getters

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With getter access is limited to the current assembly.
    /// </summary>
    ProtectedWithInternalGetter = 12,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With getter access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    ProtectedWithProtectedInternalGetter = 13,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With getter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    ProtectedWithPrivateProtectedGetter = 14,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With getter access is limited to the containing type.
    /// </summary>
    ProtectedWithPrivateGetter = 15,

    // Internal Getters

    /// <summary>
    /// Access is limited to the current assembly. With getter access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    InternalWithProtectedInternalGetter = 16,

    /// <summary>
    /// Access is limited to the current assembly. With getter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    InternalWithPrivateProtectedGetter = 17,

    /// <summary>
    /// Access is limited to the current assembly. With getter access is limited to the containing type.
    /// </summary>
    InternalWithPrivateGetter = 18,

    // ProtectedInternal Getters

    /// <summary>
    /// Access is limited to the current assembly or types derived from the containing class. With getter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    ProtectedInternalWithPrivateProtectedGetter = 19,

    /// <summary>
    /// Access is limited to the current assembly or types derived from the containing class. With getter access is limited to the containing type.
    /// </summary>
    ProtectedInternalWithPrivateGetter = 20,

    // PrivateProtected Getters

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class within the current assembly. With getter access is limited to the containing type.
    /// </summary>
    PrivateProtectedWithPrivateGetter = 21,

    #endregion

    #region SetterAccess

    // Public Setters

    /// <summary>
    /// Access is not restricted. With setter access is limited to the containing class or types derived from the containing class.
    /// </summary>
    PublicWithProtectedSetter = 22,

    /// <summary>
    /// Access is not restricted. With setter access is limited to the current assembly.
    /// </summary>
    PublicWithInternalSetter = 23,

    /// <summary>
    /// Access is not restricted. With setter access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    PublicWithProtectedInternalSetter = 24,

    /// <summary>
    /// Access is not restricted. With setter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    PublicWithPrivateProtectedSetter = 25,

    /// <summary>
    /// Access is not restricted. With setter access is limited to the containing type.
    /// </summary>
    PublicWithPrivateSetter = 26,

    // Protected Setters

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With setter access is limited to the current assembly.
    /// </summary>
    ProtectedWithInternalSetter = 27,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With setter access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    ProtectedWithProtectedInternalSetter = 28,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With setter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    ProtectedWithPrivateProtectedSetter = 29,

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class. With setter access is limited to the containing type.
    /// </summary>
    ProtectedWithPrivateSetter = 30,

    // Internal Setters

    /// <summary>
    /// Access is limited to the current assembly. With setter access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    InternalWithProtectedInternalSetter = 31,

    /// <summary>
    /// Access is limited to the current assembly. With setter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    InternalWithPrivateProtectedSetter = 32,

    /// <summary>
    /// Access is limited to the current assembly. With setter access is limited to the containing type.
    /// </summary>
    InternalWithPrivateSetter = 33,

    // ProtectedInternal Setters

    /// <summary>
    /// Access is limited to the current assembly or types derived from the containing class. With setter access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    ProtectedInternalWithPrivateProtectedSetter = 34,

    /// <summary>
    /// Access is limited to the current assembly or types derived from the containing class. With setter access is limited to the containing type.
    /// </summary>
    ProtectedInternalWithPrivateSetter = 35,

    // PrivateProtected Setters

    /// <summary>
    /// Access is limited to the containing class or types derived from the containing class within the current assembly. With setter access is limited to the containing type.
    /// </summary>
    PrivateProtectedWithPrivateSetter = 36,

    #endregion
}
