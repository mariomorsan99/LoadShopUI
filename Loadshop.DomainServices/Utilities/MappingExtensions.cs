using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loadshop.DomainServices.Utilities
{
    internal static class MappingExtensions
    {
        internal static void MapList<TSource, TDestination, TKey>(
            this List<TDestination> destinationList,
            IEnumerable<TSource> sourceList,
            Func<TDestination, TKey> destinationKey,
            Func<TSource, TKey> sourceKey,
            IMapper mapper,
            bool mapMathes = false)
        {
            List<TDestination> itemsToAdd = new List<TDestination>();

            foreach (var sourceItem in sourceList)
            {
                var destinationItem = destinationList.SingleOrDefault(di => EqualityComparer<TKey>.Default.Equals(destinationKey(di), sourceKey(sourceItem)));

                if (destinationItem == null)
                {
                    destinationItem = mapper.Map<TDestination>(sourceItem);
                    itemsToAdd.Add(destinationItem);
                }
                else if(mapMathes)
                {
                    mapper.Map(sourceItem, destinationItem);
                }
            }

            List<TDestination> itemsToRemove = new List<TDestination>();
            foreach (var destinationItem in destinationList)
            {
                var sourceItem = sourceList.SingleOrDefault(si => EqualityComparer<TKey>.Default.Equals(destinationKey(destinationItem), sourceKey(si)));
                if (sourceItem == null)
                    itemsToRemove.Add(destinationItem);
            }

            foreach (var item in itemsToRemove)
                destinationList.Remove(item);

            destinationList.AddRange(itemsToAdd);
        }
    }
}
