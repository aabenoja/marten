using System;
using System.Collections.Generic;
using Marten.Events.Projections;

namespace Marten.Events
{
    public interface IEventStoreConfiguration
    {
        string DatabaseSchemaName { get; set; }

        void AddEventType(Type eventType);
        void AddEventTypes(IEnumerable<Type> types);

        EventMapping EventMappingFor(Type eventType);
        EventMapping EventMappingFor<T>() where T : class, new();
        IEnumerable<EventMapping> AllEvents();
        IEnumerable<IAggregator> AllAggregates();
        EventMapping EventMappingFor(string eventType);
        bool IsActive { get; }

        void AggregateFor<T>(IAggregator<T> aggregator) where T : class, new();
        IAggregator<T> AggregateFor<T>() where T : class, new();
        Type AggregateTypeFor(string aggregateTypeName);
        IAggregator<T> AggregateStreamsInlineWith<T>() where T : class, new();

        void TransformEventsInlineWith<TEvent, TView>(ITransform<TEvent, TView> transform);
        void InlineTransformation(IProjection projection);

        
    }
}