using System;

namespace Integrations.Options;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class OptionsSectionAttribute(string sectionName) : Attribute
{
    public string SectionName { get; } = sectionName;
}
