
namespace m4k {
public enum ComparisonType {
    GreaterOrEqual,
    Greater,
    Equal,
    LessThan,
    LessThanOrEqual
}
public class Comparisons {
    // TODO: constrained T
    // <T> where T : IComparable
    public static bool Compare(ComparisonType comparison, int left, int right) {
        switch(comparison) {
            case ComparisonType.GreaterOrEqual: {
                return left >= right;
            }
            case ComparisonType.Greater: {
                return left > right;
            }
            case ComparisonType.Equal: {
                return left == right;
            }
            case ComparisonType.LessThan: {
                return left < right;
            }
            case ComparisonType.LessThanOrEqual: {
                return left <= right;
            }
        }
        return false;
    }

    public static bool Compare(ComparisonType comparison, long left, long right) {
        switch(comparison) {
            case ComparisonType.GreaterOrEqual: {
                return left >= right;
            }
            case ComparisonType.Greater: {
                return left > right;
            }
            case ComparisonType.Equal: {
                return left == right;
            }
            case ComparisonType.LessThan: {
                return left < right;
            }
            case ComparisonType.LessThanOrEqual: {
                return left <= right;
            }
        }
        return false;
    }
}
}