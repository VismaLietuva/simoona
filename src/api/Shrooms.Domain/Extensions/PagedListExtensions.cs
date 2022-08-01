using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels;
using System.Collections.Generic;
using X.PagedList;

namespace Shrooms.Domain.Extensions
{
    public static class PagedListExtensions
    {
        /// <summary>
        /// Creates PagedViewModel<TDestination> from IPagedList<TSource>
        /// </summary>
        /// <param name="source">IPagedList<TSource> that contains paged dtos</param>
        /// <param name="destination">IEnumerable<TDestination> that contains view models that were mapped from source</param>
        /// <param name="pageable">Parameters for page and page size</param>
        public static PagedViewModel<TDestination> ToPagedViewModel<TSource, TDestination>(
            this IPagedList<TSource> source,
            IEnumerable<TDestination> destination,
            IPageable pageable)
            where TSource : class
            where TDestination : class
        {
            var pagedModel = new StaticPagedList<TDestination>(
                destination,
                pageable.Page,
                pageable.PageSize,
                source.TotalItemCount);

            var pagedViewModel = new PagedViewModel<TDestination>
            {
                PagedList = pagedModel,
                PageCount = pagedModel.PageCount,
                ItemCount = pagedModel.TotalItemCount,
                PageSize = pagedModel.PageSize
            };

            return pagedViewModel;
        }
    }
}