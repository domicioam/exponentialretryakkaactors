using Akka.Actor;
using Akka.TestKit.Xunit2;
using ExponentialRetryAkkaActors;
using System;
using Xunit;
using System.Collections.Generic;
using static Akka.Actor.Status;

namespace ExponentialRetryAkkaActorsTests
{
    public class SequenceActorTest : TestKit
    {
        [Fact]
        public void Should_execute_sequence()
        {
            //Given
            var sequence = new Queue<Work<Status>>();
            sequence.Enqueue(() => new Success(default));
            sequence.Enqueue(() => new Success(default));
            var probe = CreateTestProbe();
            var sequenceActor = Sys.ActorOf(Props.Create(() => new SequenceActor(new Timeout(TimeSpan.FromSeconds(3)))));
            //When
            sequenceActor.Tell(sequence);
            //Then
            ExpectMsg<Status.Success>();
        }

        [Fact]
        public void Should_receive_status_failure_when_sequence_empty()
        {
            //Given
            var sequence = new Queue<Work<Status>>();
            var probe = CreateTestProbe();
            var sequenceActor = Sys.ActorOf(Props.Create(() => new SequenceActor(new Timeout(TimeSpan.FromSeconds(3)))));
            //When
            sequenceActor.Tell(sequence);
            //Then
            ExpectMsg<Status.Failure>();
        }

        [Fact]
        public void Should_receive_status_failure_when_sequence_fails()
        {
            //Given
            var sequence = new Queue<Work<Status>>();
            sequence.Enqueue(() => new Success(default));
            sequence.Enqueue(() => throw new Exception());
            var probe = CreateTestProbe();
            var sequenceActor = Sys.ActorOf(Props.Create(() => new SequenceActor(new Timeout(TimeSpan.FromSeconds(3)))));
            Watch(sequenceActor);
            //When
            sequenceActor.Tell(sequence);
            //Then
            ExpectMsg<SequenceActor.SequenceFailed>(TimeSpan.FromSeconds(10));
        }
    }
}
