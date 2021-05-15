using System;

namespace VoxarGames.Observer
{
    public class PublisherSubscriber<TEventArgs> where TEventArgs : EventArgs
    {
        public event Action<object, TEventArgs> StackEvent;

        public void Subscribe(Action<object, TEventArgs> subscriber)
        {
            StackEvent += subscriber;
        }

        public void Unsubscribe(Action<object, TEventArgs> subscriber)
        {
            StackEvent -= subscriber;
        }

        public void Publish(object sender, TEventArgs eventArg)
        {
            StackEvent?.Invoke(sender, eventArg);
        }
    }
}
