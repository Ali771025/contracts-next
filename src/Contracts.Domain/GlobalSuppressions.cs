using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "RolePermission is the canonical entity name mandated by the P03 contract.",
    Scope = "type",
    Target = "~T:Contracts.Domain.Security.RolePermission")]
