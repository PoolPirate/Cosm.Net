﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public enum PropertyVisibility
{
    Public,
    Private,
    Internal
}
public enum SetterVisibility
{
    Public,
    Private,
    Init
}
public class PropertyBuilder
{
    private readonly string _type;
    private readonly string _name;
    private PropertyVisibility _visibility = PropertyVisibility.Public;
    private SetterVisibility _setterVisibility = SetterVisibility.Public;
    private bool _isRequired = false;

    public PropertyBuilder(string type, string name)
    {
        _type = type;
        _name = name;
    }

    public PropertyBuilder WithVisibility(PropertyVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public PropertyBuilder WithSetterVisibility(SetterVisibility setterVisibility)
    {
        _setterVisibility = setterVisibility;
        return this;
    }

    public PropertyBuilder WithIsRequired(bool isRequired = true)
    {
        _isRequired = isRequired;
        return this;
    }

    public string Build() 
        => $$"""
            {{_visibility.ToString().ToLower()}} {{(_isRequired ? "required" : "")}} {{_type}} {{_name}} { get; {{
            (_setterVisibility == SetterVisibility.Init 
                ? "init" 
                : $"{_setterVisibility.ToString().ToLower()} set")}}; }
            """;
}