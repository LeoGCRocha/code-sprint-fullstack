using CodeSprint.Problems.Domain;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Problems.Infrastructure;

/// <summary>
/// Development-time seeder. Populates the Problems table with the ten reference
/// problems that the frontend ships as static data
/// (<c>code-sprint-fe/features/problems/data.ts</c>) so the catalog endpoints
/// return content on a fresh database. Idempotent: no-ops if any problem exists.
///
/// Each problem is published, which the domain requires at least one test case
/// for. The frontend data has no hidden test cases, so a single hidden case is
/// derived from the problem's first public example — enough to satisfy the
/// publish invariant and make the list/detail endpoints work in development.
/// Replace with real judging data before any problem is used for grading.
/// </summary>
public static class ProblemSeeder
{
    public static async Task SeedAsync(ProblemsDbContext db)
    {
        if (await db.Problems.AnyAsync())
            return;

        foreach (var seed in Seeds)
        {
            var problem = Problem.Create(
                Slug.Create(seed.Slug).Value,
                seed.Title,
                seed.Difficulty,
                seed.Points,
                seed.EstimatedMinutes,
                seed.Tags.Select(t => Tag.Create(t).Value).ToList(),
                seed.Description,
                seed.Notes,
                seed.InputFormat,
                seed.Constraints,
                seed.Examples).Value;

            // Derive one hidden test case from the first example so Publish() passes.
            var first = seed.Examples[0];
            problem.SetTestCases([new TestCase(1, first.Input, first.Output, isHidden: true)]);
            problem.Publish();

            db.Problems.Add(problem);
        }

        await db.SaveChangesAsync();
    }

    private static Example Ex(string input, string output, string? explanation = null)
        => new(0, input, output, explanation);

    private sealed record Seed(
        string Slug,
        string Title,
        Difficulty Difficulty,
        int Points,
        int EstimatedMinutes,
        string[] Tags,
        string Description,
        string[] Notes,
        string[] InputFormat,
        string[] Constraints,
        Example[] Examples);

    private static readonly Seed[] Seeds =
    [
        new(
            "two-sum", "Two Sum", Difficulty.Easy, 50, 10,
            ["Mathematical"],
            "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target. You may assume that each input would have exactly one solution, and you may not use the same element twice.",
            ["Each input has exactly one solution.", "You may not use the same element twice."],
            ["First line: integer N (size of array)", "Second line: N space-separated integers", "Third line: integer target"],
            ["2 ≤ N ≤ 10^4", "-10^9 ≤ nums[i] ≤ 10^9", "-10^9 ≤ target ≤ 10^9"],
            [
                Ex("nums = [2, 7, 11, 15]\ntarget = 9", "[0, 1]", "nums[0] + nums[1] = 2 + 7 = 9"),
                Ex("nums = [3, 2, 4]\ntarget = 6", "[1, 2]"),
            ]),
        new(
            "valid-parentheses", "Valid Parentheses", Difficulty.Easy, 50, 15,
            ["Strings"],
            "Given a string s containing just the characters '(', ')', '{', '}', '[' and ']', determine if the input string is valid. An input string is valid if open brackets are closed by the same type of brackets, and in the correct order.",
            ["Empty string is considered valid."],
            ["Single line: string s"],
            ["1 ≤ s.length ≤ 10^4", "s consists of parentheses only '()[]{}'"],
            [
                Ex("s = \"()\"", "true"),
                Ex("s = \"()[]{}\"", "true"),
                Ex("s = \"(]\"", "false", "Mismatched bracket types."),
            ]),
        new(
            "longest-substring-without-repeating-characters", "Longest Substring Without Repeating Characters", Difficulty.Medium, 100, 20,
            ["Strings"],
            "Given a string s, find the length of the longest substring without repeating characters.",
            ["A substring is a contiguous non-empty sequence of characters within a string."],
            ["Single line: string s"],
            ["0 ≤ s.length ≤ 5 × 10^4", "s consists of English letters, digits, symbols and spaces"],
            [
                Ex("s = \"abcabcbb\"", "3", "The answer is \"abc\", with the length of 3."),
                Ex("s = \"bbbbb\"", "1", "The answer is \"b\", with the length of 1."),
            ]),
        new(
            "3sum", "3Sum", Difficulty.Medium, 100, 25,
            ["Mathematical"],
            "Given an integer array nums, return all the triplets [nums[i], nums[j], nums[k]] such that i != j, i != k, j != k, and nums[i] + nums[j] + nums[k] == 0. The solution set must not contain duplicate triplets.",
            ["Solution set must not contain duplicate triplets."],
            ["First line: integer N", "Second line: N space-separated integers"],
            ["3 ≤ N ≤ 3000", "-10^5 ≤ nums[i] ≤ 10^5"],
            [
                Ex("nums = [-1, 0, 1, 2, -1, -4]", "[[-1,-1,2],[-1,0,1]]", "Two unique triplets sum to zero."),
            ]),
        new(
            "median-of-two-sorted-arrays", "Median of Two Sorted Arrays", Difficulty.Hard, 200, 40,
            ["Logical"],
            "Given two sorted arrays nums1 and nums2 of size m and n respectively, return the median of the two sorted arrays. The overall run time complexity should be O(log(m+n)).",
            ["Required time complexity: O(log(m+n))."],
            ["First line: integer m", "Second line: m sorted integers (nums1)", "Third line: integer n", "Fourth line: n sorted integers (nums2)"],
            ["0 ≤ m, n ≤ 1000", "1 ≤ m + n ≤ 2000", "-10^6 ≤ nums1[i], nums2[i] ≤ 10^6"],
            [
                Ex("nums1 = [1, 3]\nnums2 = [2]", "2.00000", "Merged array = [1,2,3], median is 2."),
            ]),
        new(
            "trapping-rain-water", "Trapping Rain Water", Difficulty.Hard, 200, 35,
            ["Geometrics"],
            "Given n non-negative integers representing an elevation map where the width of each bar is 1, compute how much water it can trap after raining.",
            ["Each bar has width 1."],
            ["First line: integer n", "Second line: n space-separated non-negative integers"],
            ["1 ≤ n ≤ 2 × 10^4", "0 ≤ height[i] ≤ 10^5"],
            [
                Ex("height = [0, 1, 0, 2, 1, 0, 1, 3, 2, 1, 2, 1]", "6", "6 units of rain water are trapped."),
            ]),
        new(
            "binary-search", "Binary Search", Difficulty.Easy, 50, 10,
            ["Logical"],
            "Given an array of integers nums which is sorted in ascending order, and an integer target, write a function to search target in nums. If target exists, return its index. Otherwise, return -1. You must write an algorithm with O(log n) runtime complexity.",
            ["Array is sorted in ascending order.", "Required time complexity: O(log n)."],
            ["First line: integer n", "Second line: n sorted integers", "Third line: integer target"],
            ["1 ≤ n ≤ 10^4", "-10^4 ≤ nums[i], target ≤ 10^4", "All integers in nums are unique"],
            [
                Ex("nums = [-1, 0, 3, 5, 9, 12]\ntarget = 9", "4", "9 exists in nums and its index is 4."),
                Ex("nums = [-1, 0, 3, 5, 9, 12]\ntarget = 2", "-1", "2 does not exist in nums so return -1."),
            ]),
        new(
            "merge-intervals", "Merge Intervals", Difficulty.Medium, 100, 20,
            ["Mathematical"],
            "Given an array of intervals where intervals[i] = [start_i, end_i], merge all overlapping intervals, and return an array of the non-overlapping intervals that cover all the intervals in the input.",
            ["Intervals may be given in any order."],
            ["First line: integer n (number of intervals)", "Next n lines: two integers start_i end_i"],
            ["1 ≤ n ≤ 10^4", "0 ≤ start_i ≤ end_i ≤ 10^4"],
            [
                Ex("intervals = [[1,3],[2,6],[8,10],[15,18]]", "[[1,6],[8,10],[15,18]]", "[1,3] and [2,6] overlap so merge to [1,6]."),
            ]),
        new(
            "word-search", "Word Search", Difficulty.Medium, 100, 30,
            ["Strings", "Logical"],
            "Given an m x n grid of characters board and a string word, return true if word exists in the grid. The word can be constructed from letters of sequentially adjacent cells (horizontally or vertically). The same letter cell may not be used more than once.",
            ["Same cell may not be used more than once."],
            ["First line: integers m n", "Next m lines: n characters each (the grid)", "Last line: string word"],
            ["1 ≤ m, n ≤ 6", "1 ≤ word.length ≤ 15", "board and word consist only of lowercase and uppercase English letters"],
            [
                Ex("board = [[\"A\",\"B\",\"C\",\"E\"],[\"S\",\"F\",\"C\",\"S\"],[\"A\",\"D\",\"E\",\"E\"]]\nword = \"ABCCED\"", "true"),
            ]),
        new(
            "minimum-window-substring", "Minimum Window Substring", Difficulty.Hard, 200, 45,
            ["Strings"],
            "Given two strings s and t of lengths m and n respectively, return the minimum window substring of s such that every character in t (including duplicates) is included in the window. If there is no such substring, return the empty string.",
            ["The answer is guaranteed to be unique.", "If no valid window exists, return empty string."],
            ["First line: string s", "Second line: string t"],
            ["1 ≤ m, n ≤ 10^5", "s and t consist of uppercase and lowercase English letters"],
            [
                Ex("s = \"ADOBECODEBANC\"\nt = \"ABC\"", "\"BANC\"", "Minimum window containing A, B, C is BANC."),
            ]),
    ];
}
