using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Common.Services.QueryWrappers
{
    public class PageableQuery<T> : IEnumerable<T>
    {
        IQueryable<T> _query;
        int? _take = null;
        int? _skip = null;
        Expression<Func<T, bool>> _filter = null;
        List<T> _queryResult = null;
        int _maxResults = 100;
        Func<List<T>, Task> _queryExecuteFuncAsync = null;
        SortData _orderByExpression;
        Queue<SortData> _theByExpressions = new Queue<SortData>();
        int? _totalRecords = null;

        public PageableQuery(IQueryable<T> query, Func<List<T>, Task> queryExecuteFuncAsync = null, int maxResult = 100)
        {
            _query = query;

            _queryExecuteFuncAsync = queryExecuteFuncAsync;
            _maxResults = maxResult;
        }

        private PageableQuery()
        {
        }

        public static PageableQuery<T> Empty()
        {
            return new PageableQuery<T>();
        }

        public PageableQuery<T> Take(int count)
        {
            _take = count;
            return this;
        }

        public PageableQuery<T> Skip(int count)
        {
            _skip = count;
            return this;
        }

        public PageableQuery<T> Filter(Expression<Func<T, bool>> filterExpression)
        {
            _filter = filterExpression;
            return this;
        }

        public PageableQuery<T> OrderBy<TOut>(Expression<Func<T, TOut>> propertyExpression)
        {
            _orderByExpression = new SortData<TOut>(propertyExpression, SortType.Ascending);
            return this;
        }

        public PageableQuery<T> OrderByDescending<TOut>(Expression<Func<T, TOut>> propertyExpression)
        {
            _orderByExpression = new SortData<TOut>(propertyExpression, SortType.Descending);
            _theByExpressions.Clear();
            return this;
        }

        public PageableQuery<T> ThenBy<TOut>(Expression<Func<T, TOut>> propertyExpression)
        {
            _theByExpressions.Enqueue(new SortData<TOut>(propertyExpression, SortType.Ascending));
            _theByExpressions.Clear();
            return this;
        }

        public PageableQuery<T> ThenByDescending<TOut>(Expression<Func<T, TOut>> propertyExpression)
        {
            _theByExpressions.Enqueue(new SortData<TOut>(propertyExpression, SortType.Descending));
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_queryResult == null)
            {
                var task = ExecuteQueryAsync();
                task.Wait();
            }

            return _queryResult.GetEnumerator();
        }

        public async Task ExecuteQueryAsync()
        {
            if (_query != null)
            {
                IQueryable<T> query = _query;

                if (_filter != null)
                {
                    query = query.Where(_filter);
                }

                _totalRecords = await Task.Run(() => query.Count());

                query = ProcessSorting(query);

                query = query.Skip(_skip ?? 0);

                if (_take > _maxResults)
                {
                    query = query.Take(_maxResults);
                }
                else
                {
                    query = query.Take(_take ?? _maxResults);
                }

                _queryResult = await Task.Run(() => query.ToList());

                if (_queryExecuteFuncAsync != null)
                    await _queryExecuteFuncAsync.Invoke(_queryResult);
            }
            else
            {
                _queryResult = new List<T>();
            }
        }

        /// <summary>
        /// Returns the total records before paging is applied to the query
        /// Will Execute query
        /// </summary>
        /// <returns>Total possible rows</returns>
        public async Task<int> GetTotalRecords()
        {
            if (_totalRecords.HasValue)
                return _totalRecords.Value;

            await ExecuteQueryAsync();

            return _totalRecords.GetValueOrDefault();
        }

        private IQueryable<T> ProcessSorting(IQueryable<T> query)
        {
            if (_orderByExpression != null)
            {
                IOrderedQueryable<T> orderedQueryable;

                orderedQueryable = _orderByExpression.ApplySort(query);

                foreach (var theBy in _theByExpressions)
                {
                    for (int i = 0; i < _theByExpressions.Count; i++)
                    {
                        var thenByExpression = _theByExpressions.Dequeue();

                        orderedQueryable = thenByExpression.ApplySort(orderedQueryable);
                    }
                }

                return orderedQueryable;
            }

            return query;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        abstract class SortData
        {
            public abstract IOrderedQueryable<T> ApplySort(IQueryable<T> query);

            public abstract IOrderedQueryable<T> ApplySort(IOrderedQueryable<T> orderedQuery);
        }

        class SortData<TOut> : SortData
        {
            Expression<Func<T, TOut>> _propertyExpression;
            public SortType Type { get; }

            public SortData(Expression<Func<T, TOut>> propertyExpression, SortType sortType)
            {
                _propertyExpression = propertyExpression;
                Type = sortType;
            }

            public override IOrderedQueryable<T> ApplySort(IQueryable<T> query)
            {
                switch (Type)
                {
                    case SortType.Ascending:
                        {
                            return query.OrderBy(_propertyExpression);
                        }
                    case SortType.Descending:
                        {
                            return query.OrderByDescending(_propertyExpression);
                        }
                    default:
                        {
                            return query.OrderBy(_propertyExpression);
                        }
                }
            }

            public override IOrderedQueryable<T> ApplySort(IOrderedQueryable<T> orderedQuery)
            {
                switch (Type)
                {
                    case SortType.Ascending:
                        {
                            return orderedQuery.ThenBy(_propertyExpression);
                        }
                    case SortType.Descending:
                        {
                            return orderedQuery.ThenByDescending(_propertyExpression);
                        }
                }

                return orderedQuery;
            }

        }

        enum SortType
        {
            Ascending,
            Descending
        }
    }
}
