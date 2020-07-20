using System.Collections.Generic;
using System;
using System.Linq;

public static class Message
{
    private static readonly List<MessageSubscription> EventSubs = new List<MessageSubscription>();
    private static readonly MessageQueue Messages = new MessageQueue();

    public static void Publish(object payload) => Messages.Enqueue(payload);
    public static void Subscribe<T>(Action<T> onEvent, object owner) => Subscribe(MessageSubscription.Create(onEvent, owner));
    
    private static void Subscribe(MessageSubscription subscription)
    {
        Messages.Subscribe(subscription);
        EventSubs.Add(subscription);
    }

    public static void Unsubscribe(object owner)
    {
        Messages.Unsubscribe(owner);
        foreach(var sub in EventSubs.Where(x => x.Owner.Equals(owner)).ToList())
            EventSubs.Remove(sub);
    }
    
    private sealed class MessageSubscription
    {
        public Type EventType { get; }
        public Action<object> OnEvent { get; }
        public object Owner { get; }

        private MessageSubscription(Type eventType, Action<object> onEvent, object owner)
        {
            EventType = eventType;
            OnEvent = onEvent;
            Owner = owner;
        }

        public static MessageSubscription Create<T>(Action<T> onEvent, object owner) 
            => new MessageSubscription(typeof(T), (o) => { if (o.GetType() == typeof(T)) onEvent((T)o); }, owner);
    }
    
    private sealed class MessageQueue
    {
        private readonly Dictionary<string, List<object>> _eventActions = new Dictionary<string, List<object>>();
        private readonly Dictionary<object, List<MessageSubscription>> _ownerSubscriptions = new Dictionary<object, List<MessageSubscription>>();

        private readonly Queue<object> _eventQueue = new Queue<object>();
        private bool _isPublishing;

        public int SubscriptionCount => _eventActions.Sum(e => e.Value.Count);

        public void Enqueue(object payload)
        {
            _eventQueue.Enqueue(payload);
            ProcessQueuedMessages();
        }

        public void Subscribe(MessageSubscription subscription)
        {
            var eventType = subscription.EventType.Name;
            if (!_eventActions.ContainsKey(eventType))
                _eventActions[eventType] = new List<object>();
            if (!_ownerSubscriptions.ContainsKey(subscription.Owner))
                _ownerSubscriptions[subscription.Owner] = new List<MessageSubscription>();
            _eventActions[eventType].Add(subscription.OnEvent);
            _ownerSubscriptions[subscription.Owner].Add(subscription);
        }

        public void Unsubscribe(object owner)
        {
            if (!_ownerSubscriptions.ContainsKey(owner))
                return;
            var events = _ownerSubscriptions[owner];
            for (var i = 0; i < _eventActions.Count; i++)
                _eventActions.ElementAt(i).Value.RemoveAll(x => events.Any(y => y.OnEvent.Equals(x)));
            _ownerSubscriptions.Remove(owner);
        }

        private void ProcessQueuedMessages()
        {
            if (_isPublishing) return;
            
            _isPublishing = true;
            while (_eventQueue.Any()) 
                Publish(_eventQueue.Dequeue());
            _isPublishing = false;
        }

        private void Publish(object payload)
        {
            var eventType = payload.GetType().Name;

            if (_eventActions.ContainsKey(eventType))
                foreach (var action in _eventActions[eventType].ToList())
                    ((Action<object>)action)(payload);
        }
    }
}
