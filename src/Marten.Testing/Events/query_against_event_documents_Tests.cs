﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Marten.Services;
using Shouldly;
using Xunit;

namespace Marten.Testing.Events
{
    public class query_against_event_documents_Tests : DocumentSessionFixture<NulloIdentityMap>
    {
        private MembersJoined joined1 = new MembersJoined { Members = new string[] { "Rand", "Matt", "Perrin", "Thom" } };
        private MembersDeparted departed1 = new MembersDeparted { Members = new[] { "Thom" } };

        private MembersJoined joined2 = new MembersJoined { Members = new string[] { "Nynaeve", "Egwene" } };
        private MembersDeparted departed2 = new MembersDeparted { Members = new[] { "Matt" } };

        // SAMPLE: query-against-event-data
        [Fact]
        public void can_query_against_event_type()
        {
            theSession.Events.StartStream<Quest>(joined1, departed1);
            theSession.Events.StartStream<Quest>(joined2, departed2);

            theSession.SaveChanges();

            theSession.Events.QueryRawEventDataOnly<MembersJoined>().Count().ShouldBe(2);
            theSession.Events.QueryRawEventDataOnly<MembersJoined>().ToArray().SelectMany(x => x.Members).Distinct()
                .OrderBy(x => x)
                .ShouldHaveTheSameElementsAs("Egwene", "Matt", "Nynaeve", "Perrin", "Rand", "Thom");

            theSession.Events.QueryRawEventDataOnly<MembersDeparted>().Where(x => x.Members.Contains("Matt"))
                .Single().Id.ShouldBe(departed2.Id);
        }
        // ENDSAMPLE

        [Fact]
        public void will_not_blow_up_if_searching_for_events_before_event_store_is_warmed_up()
        {
            theSession.Events.QueryRawEventDataOnly<MembersJoined>().Any().ShouldBeFalse();
        }

        [Fact]
        public void can_query_against_event_type_with_different_schema_name()
        {
            StoreOptions(_ =>
            {
                _.DatabaseSchemaName = "test";
                _.Events.DatabaseSchemaName = "events";

                _.Events.AddEventType(typeof(MembersDeparted));
            });

            theStore.Schema.MappingFor(typeof(MembersDeparted)).ToQueryableDocument()
                .Table.Schema.ShouldBe("events");

            theSession.Events.StartStream<Quest>(joined1, departed1);
            theSession.Events.StartStream<Quest>(joined2, departed2);

            theSession.SaveChanges();

            theSession.Events.QueryRawEventDataOnly<MembersJoined>().Count().ShouldBe(2);
            theSession.Events.QueryRawEventDataOnly<MembersJoined>().ToArray().SelectMany(x => x.Members).Distinct()
                .OrderBy(x => x)
                .ShouldHaveTheSameElementsAs("Egwene", "Matt", "Nynaeve", "Perrin", "Rand", "Thom");

            theSession.Events.QueryRawEventDataOnly<MembersDeparted>().Where(x => x.Members.Contains("Matt"))
                .Single().Id.ShouldBe(departed2.Id);
        }
    }
}