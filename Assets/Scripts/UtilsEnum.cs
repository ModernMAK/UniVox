using System;

public static class UtilsEnum
{
	public static bool HasFlag(this Enum mask, Enum flags) // Same behavior than Enum.HasFlag is .NET 4
	{
#if DEBUG
		if (mask.GetType() != flags.GetType())
			throw new System.ArgumentException(
				string.Format("The argument type, '{0}', is not the same as the enum type '{1}'.",
					flags.GetType(), mask.GetType()));
#endif
		return ((int)(IConvertible)mask & (int)(IConvertible)flags) == (int)(IConvertible)flags;
	}
}