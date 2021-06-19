using Akka.Actor;
using Akka.TestKit.Xunit2;
using ExponentialRetryAkkaActors;
using Xunit;

namespace ExponentialRetryAkkaActorsTests
{
    public class ExponentialRetryActorTests : TestKit
    {
        [Fact]
        public void Should_create_actor()
        {
            var subject = Sys.ActorOf(Props.Create(() => new ExponentialRetryActor()));
        }
    }
}
