using System;

namespace m4k {
public enum ComparisonType {
    GreaterOrEqual,
    Greater,
    Equal,
    LessThan,
    LessThanOrEqual
}
public class Comparisons {
    public static bool Compare<T>(ComparisonType comparison, T left, T right) where T : IComparable 
    {
        int result = left.CompareTo(right);

        return Compare(comparison, result);
    }

    public static bool Compare(ComparisonType comparisonType, int result) {
        switch(comparisonType) {
            case ComparisonType.GreaterOrEqual: {
                return result >= 0;
            }
            case ComparisonType.Greater: {
                return result > 0;
            }
            case ComparisonType.Equal: {
                return result == 0;
            }
            case ComparisonType.LessThan: {
                return result < 0;
            }
            case ComparisonType.LessThanOrEqual: {
                return result <= 0;
            }
        }
        return false;
    }
}
}