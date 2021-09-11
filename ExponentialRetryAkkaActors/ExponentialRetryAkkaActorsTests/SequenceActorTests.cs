using Akka.Actor;
using Akka.TestKit.Xunit2;
using ExponentialRetryAkkaActors;
using System;
using Xunit;
using System.Collections.Generic;

namespace ExponentialRetryAkkaActorsTests
{
    public class SequenceActorTest : TestKit
    {
        [Fact]
        public void Should_execute_sequence()
        {
            //Given
            var sequence = new Queue<Action>();
            sequence.Enqueue(() => Console.WriteLine("Job #1"));
            sequence.Enqueue(() => Console.WriteLine("Job #2"));
            var probe = CreateTestProbe();
            var sequenceActor = Sys.ActorOf(Props.Create(() => new SequenceActor()));
            //When
            sequenceActor.Tell(sequence);
            //Then
            ExpectMsg<Status.Success>();
        }

        [Fact]
        public void Should_receive_status_failure_when_sequence_empty()
        {
            //Given
            var sequence = new Queue<Action>();
            var probe = CreateTestProbe();
            var sequenceActor = Sys.ActorOf(Props.Create(() => new SequenceActor()));
            //When
            sequenceActor.Tell(sequence);
            //Then
            ExpectMsg<Status.Failure>();
        }

        [Fact]
        public void Should_receive_status_failure_when_sequence_fails()
        {
            //Given
            var sequence = new Queue<Action>();
            sequence.Enqueue(() => Console.WriteLine("Job #1"));
            sequence.Enqueue(() => throw new Exception());
            var probe = CreateTestProbe();
            var sequenceActor = Sys.ActorOf(Props.Create(() => new SequenceActor()));
            //When
            sequenceActor.Tell(sequence);
            //Then
            ExpectMsg<Status.Failure>();
        }
    }
}
