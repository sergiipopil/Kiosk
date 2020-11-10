using System;
using System.Collections.Generic;
using System.Linq;
using KioskBrains.Common.Contracts;
using KioskBrains.Waf.Helpers.Contracts;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Waf.Helpers.Collections
{
    public static class CollectionMergeHelper
    {
        public static void ApplyOneToManyViewModels<TEntity, TViewModelRecord>(
            TEntity[] currentEntities,
            TViewModelRecord[] viewModelRecords,
            Action<TViewModelRecord> entityCreateHandler,
            Action<TEntity, TViewModelRecord> entityUpdateHandler,
            Action<TEntity> entityRemoveHandler)
            where TEntity : IEntityBase
            where TViewModelRecord : IEntityIdProvider
        {
            Assure.ArgumentNotNull(currentEntities, nameof(currentEntities));
            Assure.ArgumentNotNull(viewModelRecords, nameof(viewModelRecords));
            Assure.ArgumentNotNull(entityCreateHandler, nameof(entityCreateHandler));
            Assure.ArgumentNotNull(entityUpdateHandler, nameof(entityUpdateHandler));
            Assure.ArgumentNotNull(entityRemoveHandler, nameof(entityRemoveHandler));

            var currentEntitiesById = currentEntities
                .ToDictionary(x => x.Id);
            foreach (var viewModelRecord in viewModelRecords)
            {
                if (viewModelRecord.Id == null)
                {
                    entityCreateHandler(viewModelRecord);
                }
                else
                {
                    var currentEntity = currentEntitiesById.GetValueOrDefault(viewModelRecord.Id.Value);
                    if (currentEntity == null)
                    {
                        // ignore - entity is already removed
                        continue;
                    }
                    entityUpdateHandler(currentEntity, viewModelRecord);
                    // remove entity from list
                    currentEntitiesById.Remove(viewModelRecord.Id.Value);
                }
            }

            // remove remaining entities
            foreach (var removingEntity in currentEntitiesById.Values)
            {
                entityRemoveHandler(removingEntity);
            }
        }

        public static void ApplyManyToManyViewModels<TManyToManyEntity>(
            List<TManyToManyEntity> parentEntityManyToManyCollection,
            int[] viewModelValues,
            DbSet<TManyToManyEntity> manyToManyEntityRepository,
            Action<TManyToManyEntity, int> foreignKeySetter)
            where TManyToManyEntity : class, new()
        {
            Assure.ArgumentNotNull(manyToManyEntityRepository, nameof(manyToManyEntityRepository));
            Assure.ArgumentNotNull(foreignKeySetter, nameof(foreignKeySetter));

            if (viewModelValues == null
                || viewModelValues.Length == 0)
            {
                manyToManyEntityRepository.RemoveRange(parentEntityManyToManyCollection);
                return;
            }

            // equalize collection sizes
            if (parentEntityManyToManyCollection.Count != viewModelValues.Length)
            {
                if (parentEntityManyToManyCollection.Count > viewModelValues.Length)
                {
                    for (var i = viewModelValues.Length; i < parentEntityManyToManyCollection.Count; i++)
                    {
                        manyToManyEntityRepository.Remove(parentEntityManyToManyCollection[i]);
                    }
                }
                else
                {
                    for (var i = parentEntityManyToManyCollection.Count; i < viewModelValues.Length; i++)
                    {
                        parentEntityManyToManyCollection.Add(new TManyToManyEntity());
                    }
                }
            }

            for (var i = 0; i < viewModelValues.Length; i++)
            {
                foreignKeySetter(parentEntityManyToManyCollection[i], viewModelValues[i]);
            }
        }
    }
}