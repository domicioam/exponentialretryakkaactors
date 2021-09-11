using Akka.Actor;
using System.Collections.Generic;
using System;

namespace ExponentialRetryAkkaActors
{
    public class SequenceActor : ReceiveActor
    {
        #region Messages
        private record Execute(Action Action);
        #endregion

        private Queue<Action> queue;
        private IActorRef requestor;

        public SequenceActor()
        {
            Receive<Execute>(HandleExecute);
            Receive<Queue<Action>>(HandleSequence);
        }

        private void HandleSequence(Queue<Action> queue) {
            if(queue.Count == 0)
                Sender.Tell(new Status.Failure(new ArgumentException("Sequence is empty")));

            this.queue = queue;
            this.requestor = Sender;
            Self.Tell(new Execute(queue.Dequeue()));
        }

        private void HandleExecute(Execute execute) {
            execute.Action(); // Danger
            if(queue.TryDequeue(out Action next))
                Self.Tell(new Execute(next));
            else
                requestor.Tell(new Status.Success(default));
        }
    }
}