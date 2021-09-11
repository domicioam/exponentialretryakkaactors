using Akka.Actor;
using Akka.Pattern;
using System;
using static Akka.Actor.Status;

namespace ExponentialRetryAkkaActors
{
    public delegate T Work<T>();

    public class ExponentialRetryActor<T> : ReceiveActor
    {
        #region Messages
        public class DoWork
        {
            public Work<T> Work { get; }

            public DoWork(Work<T> work)
            {
                Work = work;
            }
        }
        #endregion

        private IActorRef requestor;
        private IActorRef supervisor;

        public ExponentialRetryActor()
        {
            Receive<DoWork>(msg =>
            {
                requestor = Sender;
                var workerProps = Props.Create(() => new Worker(msg.Work));

                var supervisorProps = BackoffSupervisor.Props(
                    Backoff.OnFailure(
                        workerProps,
                        childName: "worker",
                        minBackoff: TimeSpan.FromSeconds(3),
                        maxBackoff: TimeSpan.FromSeconds(5),
                        randomFactor: 0.2,
                        maxNrOfRetries: 2) // maxNrOfRetries is being ignored for some reason
                        .WithSupervisorStrategy(new OneForOneStrategy(exception =>
                        {
                            if (exception is Exception)
                                return Directive.Restart;
                            return Directive.Escalate;
                        })
                        .WithMaxNrOfRetries(2))); // must use because the parameter value ignored
                supervisor = Context.ActorOf(supervisorProps, "supervisor");
                Context.Watch(supervisor);
            });

            Receive<Success>(msg =>
            {
                Context.Stop(supervisor);
                requestor.Tell(msg);
            });

            Receive<Terminated>(msg => {
                Context.Stop(Self);
            });
        }

        private class Worker : ReceiveActor
        {
            private readonly Work<T> work;

            public Worker(Work<T> work)
            {
                this.work = work;
            }

            protected override void PreStart()
            {
                base.PreStart();
                var result = work();
                Context.Parent.Tell(new Success(result));
            }
        }
    }
}
