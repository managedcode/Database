﻿using System;

namespace ManagedCode.Repository.Core.Extensions;

public static class TypeExtensions
{
    public static bool EqualsToGeneric(this Type type, Type baseType)
    {
        if (type != null)
        {
            return type.Name.Equals(baseType.Name);
        }

        return false;
    }
}