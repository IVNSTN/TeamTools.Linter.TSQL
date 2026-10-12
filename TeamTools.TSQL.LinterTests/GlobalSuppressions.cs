// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Reviewed", Scope = "NameSpaceAndDescendants", Target = "~N:TeamTools.TSQL.LinterTests")]
[assembly: SuppressMessage("Structure", "CA1016:Mark assemblies with assembly version", Justification = "Tests are not published and don't need versioning")]
[assembly: SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Not relevant for tests")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "Irrelevant for tests")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "In test classes some method implementations may look static but they are not")]
[assembly: SuppressMessage("CodeSmell", "RCS1202:Avoid NullReferenceException", Justification = "Just let test fail")]
