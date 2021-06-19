﻿using Akka.Actor;
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
        private bool shouldWork;

        [Fact]
        public void Should_create_actor()
        {
            var subject = Sys.ActorOf(Props.Create(() => new ExponentialRetryActor<string>()));
        }

        [Fact]
        public void Should_do_work()
        {
            var subject = Sys.ActorOf(Props.Create(() => new ExponentialRetryActor<string>()));
            subject.Tell(new ExponentialRetryActor<string>.DoWork(() => "Hello world"));
            var msg = ExpectMsg<Success>();
            Assert.Equal("Hello world", msg.Status);
        }

        [Fact]
        public void Should_do_work_after_failure()
        {
            var subject = Sys.ActorOf(Props.Create(() => new ExponentialRetryActor<string>()));
            subject.Tell(new ExponentialRetryActor<string>.DoWork(Work));
            var msg = ExpectMsg<Success>(TimeSpan.FromSeconds(10));
            Assert.Equal("Hello world", msg.Status);
        }

        private string Work()
        {
            if (shouldWork)
            {
                return "Hello world";
            }
            else
            {
                shouldWork = true;
                throw new Exception();
            }
        }
    }
}
