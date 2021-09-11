using Akka.Actor;
using System.Collections.Generic;
using System;
using static Akka.Actor.Status;

namespace ExponentialRetryAkkaActors
{
    public class SequenceActor : ReceiveActor
    {
        #region Messages
        private record Execute(Work<Status> Action);
        #endregion

        private Queue<Work<Status>> queue;
        private IActorRef requestor;

        public SequenceActor()
        {
            Receive<Execute>(HandleExecute);
            Receive<Queue<Work<Status>>>(HandleSequence);
            Receive<Success>(_ => HandleExecuteSuccess());
            Receive<Terminated>(_ => Context.Stop(Self));
        }

        private void HandleSequence(Queue<Work<Status>> queue) {
            if(queue.Count == 0)
                Sender.Tell(new Status.Failure(new ArgumentException("Sequence is empty")));

            this.queue = queue;
            this.requestor = Sender;
            Self.Tell(new Execute(queue.Dequeue()));
        }

        private void HandleExecute(Execute execute) {
            var retryActor = Context.ActorOf(Props.Create(() => new ExponentialRetryActor<Status>()));
            retryActor.Tell(new ExponentialRetryActor<Status>.DoWork(execute.Action)); // Danger
            Context.Watch(retryActor);
        }

        private void HandleExecuteSuccess() {
            if(queue.TryDequeue(out Work<Status> next))
                Self.Tell(new Execute(next));
            else
                requestor.Tell(new Status.Success(default));
        }
    }
}