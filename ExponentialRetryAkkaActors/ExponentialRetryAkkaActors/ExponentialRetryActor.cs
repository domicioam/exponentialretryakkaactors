using Akka.Actor;
using Akka.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Akka.Actor.Status;

namespace ExponentialRetryAkkaActors
{
    public delegate void Work();

    public class ExponentialRetryActor : ReceiveActor
    {
        #region Messages
        public class DoWork
        {
            public Work Work { get; }

            public DoWork(Work work)
            {
                Work = work;
            }
        }
        #endregion

        public ExponentialRetryActor()
        {
            Receive<DoWork>(msg =>
            {
                requestor = Sender;
                var workerProps = Props.Create(() => new Worker(msg.Work));

                var supervisorProps = BackoffSupervisor.Props(
                    Backoff.OnFailure(
                        workerProps,
                        childName: "myEcho",
                        minBackoff: TimeSpan.FromSeconds(3),
                        maxBackoff: TimeSpan.FromSeconds(30),
                        randomFactor: 0.2,
                        maxNrOfRetries: 10)
                        .WithSupervisorStrategy(new OneForOneStrategy(exception =>
                        {
                            if (exception is Exception)
                                return Directive.Restart;
                            return Directive.Escalate;
                        })));

                supervisor = Context.ActorOf(supervisorProps, "supervisor");
            });

            Receive<Success>(msg =>
            {
                Context.Stop(supervisor);
                requestor.Tell(msg);
            });
        }

        private IActorRef requestor;
        private IActorRef supervisor;

        private class Worker : ReceiveActor
        {
            private readonly Work work;

            public Worker(Work work)
            {
                this.work = work;
            }

            protected override void PreStart()
            {
                base.PreStart();
                work();
                Context.Parent.Tell(new Success(null));
            }
        }
    }
}
