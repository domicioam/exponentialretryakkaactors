using Akka.Actor;
using System;

namespace ExponentialRetryAkkaActors
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystem = ActorSystem.Create("ActorSystem");
        }
    }
}
