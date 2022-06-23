using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DropboxSync.UIL.Models;

namespace DropboxSync.UIL.Iterators
{
    public class EventsIterator : IEnumerable<EventAttempt>
    {
        /// <summary>
        /// Array of <see cref="EventAttempt"/> object. Is <c>null</c> by default
        /// </summary>
        private EventAttempt[]? _eventAttemps = null;

        /// <summary>
        /// Position in <see cref="EventsIterator"/>
        /// </summary>
        private int _position = 1;

        public event Action? OnElementAdded;

        /// <summary>
        /// Get the length of this <see cref="EventsIterator"/>
        /// </summary>
        /// <return>The length</return>
        public int Length
        {
            get
            {
                if (_eventAttemps is null) throw new NullReferenceException(nameof(_eventAttemps));
                return _eventAttemps.Length;
            }
        }

        public EventsIterator()
        {
            _eventAttemps = Array.Empty<EventAttempt>();
        }

        /// <summary>
        /// Returns a <see cref="EventAttemp"/> at the defined <paramref name="index"/>
        /// </summary>
        /// <return>The object <see cref="EventAttempt"/> at position <c>index</c></return>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public EventAttempt this[int index]
        {
            get
            {
                if (_eventAttemps is null) throw new NullReferenceException(nameof(_eventAttemps));
                if (index < 0 || index >= _eventAttemps.Length) throw new IndexOutOfRangeException(nameof(index));

                return _eventAttemps[index];
            }
        }

        /// <summary>
        /// Add an element in the collection and invoke <see cref="OnElementAdded"/> event
        /// </summary>
        /// <param name="eventAttempt"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void Add(EventAttempt eventAttempt)
        {
            if (_eventAttemps is null) throw new NullReferenceException(nameof(_eventAttemps));

            EventAttempt[] eventAttempts = new EventAttempt[_eventAttemps.Length + 1];
            Array.Copy(_eventAttemps, eventAttempts, _eventAttemps.Length);
            eventAttempts[eventAttempts.Length - 1] = eventAttempt;
            _eventAttemps = eventAttempts;
            OnElementAdded?.Invoke();
        }

        public void RemoveAt(int index)
        {
            if (_eventAttemps is null) throw new NullReferenceException(nameof(_eventAttemps));
            if (index < 0 || index >= _eventAttemps.Length) throw new IndexOutOfRangeException(nameof(index));

            EventAttempt[] eventAttemps = new EventAttempt[_eventAttemps.Length - 1];

            int inc = 0;

            for (int i = 0; i < _eventAttemps.Length; i++)
            {
                if (i != index)
                {
                    eventAttemps[inc] = _eventAttemps[i];
                    inc++;
                }
            }

            _eventAttemps = eventAttemps;
        }

        public IEnumerator<EventAttempt> GetEnumerator()
        {
            if (_eventAttemps is null) throw new NullReferenceException(nameof(_eventAttemps));
            return (_eventAttemps as IEnumerable<EventAttempt>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_eventAttemps is null) throw new NullReferenceException(nameof(_eventAttemps));
            return (_eventAttemps as IEnumerable<EventAttempt>).GetEnumerator();
        }
    }
}