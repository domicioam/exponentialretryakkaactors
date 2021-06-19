using Akka.Actor;
using Akka.TestKit.Xunit2;
using ExponentialRetryAkkaActors;
using System;
using System.Diagnostics;
using Xunit;
using static Akka.Actor.Status;

namespace ExponentialRetryAkkaActorsTests
{
    public class ExponentialRetryActorTests : TestKit
    {
        [Fact]
        public void Should_create_actor()
        {
            var subject = Sys.ActorOf(Props.Create(() => new ExponentialRetryActor()));
        }

        [Fact]
        public void Should_do_work()
        {
            var subject = Sys.ActorOf(Props.Create(() => new ExponentialRetryActor()));
            subject.Tell(new ExponentialRetryActor.DoWork(() => Debug.WriteLine("Hello world")));
            ExpectMsg<Success>();
        }
    }
}
