using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelWorksheetBuilder
    {
        IExcelWorksheetBuilder AddHeader(params string[] headerParts);

        IExcelWorksheetBuilder AddHeader(IEnumerable<string> headerParts);

        IExcelWorksheetBuilder AddColumnSequence<T>(IEnumerable<T> data, Func<T, IExcelColumn> mapFuction, int moveToNextRowAfterCount, bool applyAutoFit = true);

        IExcelWorksheetBuilder AddRows(IExcelRowCollection rowCollection, bool applyAutoFit = true);

        IExcelWorksheetBuilder AddRows<T>(IQueryable<T> data, Expression<Func<T, IExcelRow>> mapExpression, bool applyAutoFit = true);

        IExcelWorksheetBuilder AutoFitColumns(int minimumWidth, int maximumWidth);

        IExcelWorksheetBuilder AutoFitColumns();

        IExcelWorksheetBuilder AddRowsPadding(double padding);

        IExcelWorksheetBuilder AddColumnsPadding(double padding);
    }
}
