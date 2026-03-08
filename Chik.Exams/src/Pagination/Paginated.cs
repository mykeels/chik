using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chik.Exams;

/// <summary>
/// A paginated result
/// </summary>
/// <typeparam name="T"></typeparam>
public record Paginated<T>: IEnumerable<T>
{
    [Required]
    public List<T> Items { get; set; } = default!;

    [Required]
    public long TotalCount { get; set; }

    [Required]
    public int PageSize { get; set; }

    [Required]
    public int Page { get; set; }

    [Required]
    public int TotalPages
    {
        get
        {
            var totalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            return Math.Max(totalPages == 0 ? 1 : totalPages, 0);
        }
    }

    [Required]
    public int NextPage
    {
        get
        {
            var nextPage = Page + 1;
            return Math.Max(nextPage > TotalPages ? TotalPages : nextPage, 0);
        }
    }

    [Required]
    public int PreviousPage
    {
        get
        {
            var previousPage = Page - 1;
            return previousPage < 1 ? 1 : previousPage;
        }
    }

    [Required]
    public int Count => Items.Count;

    private Func<PaginationOptions, Task<Paginated<T>>>? GetPage { get; set; }

    public Paginated(
        List<T> items, 
        long totalCount, 
        int page, 
        int pageSize,
        Func<PaginationOptions, Task<Paginated<T>>> getPage
    )
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        GetPage = getPage;
    }

    public Paginated(
        List<T> items, 
        long totalCount, 
        PaginationOptions pagination,
        Func<PaginationOptions, Task<Paginated<T>>> getPage
    )
    {
        Items = items;
        TotalCount = totalCount;
        Page = pagination.Page;
        PageSize = pagination.PageSize;
        GetPage = getPage;
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in Items)
        {
            yield return item;
        }
        Paginated<T>? nextPage = null;
        int page = Page;
        if (GetPage is not null)
        {
            nextPage = GetPage(new PaginationOptions(++page, PageSize)).Result;
        }
        while (nextPage?.Items is not null && nextPage.Items.Count > 0 && GetPage is not null)
        {
            foreach (var item in nextPage.Items)
            {
                yield return item;
            }
            nextPage = GetPage(new PaginationOptions(++page, PageSize)).Result;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class PaginatedExtensions
{
    public static Paginated<T> ToPaginated<T>(
        this List<T> items,
        int totalCount,
        int page,
        int pageSize,
        Func<PaginationOptions, Task<Paginated<T>>> getPage
    )
    {
        return new Paginated<T>(items, totalCount, page, pageSize, getPage);
    }

    public static Paginated<T> ToPaginated<T>(
        this List<T> items,
        long totalCount,
        PaginationOptions pagination,
        Func<PaginationOptions, Task<Paginated<T>>> getPage
    )
    {
        return new Paginated<T>(items, totalCount, pagination, getPage);
    }
}
