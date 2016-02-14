using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WPF_Tools
{
    /// <summary>
    /// This derived class of ObservableCollection<T> implements an 'AddRange(IEnumerable<T>)' method 
    /// which will not fire OnCollectionChanged for each item and so won't block the UI.
    /// This class also fires the OnPropertyChanged event for 'Count' when the collection is changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeEnabledObservableCollection<T> : ObservableCollection<T>, INotifyCollectionChanged
    {
        private Dispatcher _dispatcherForCollection;

        public RangeEnabledObservableCollection(IEnumerable<T> source) : this()
        {
            AddRange(source);
        }

        public RangeEnabledObservableCollection()
        {
            _dispatcherForCollection = Dispatcher.CurrentDispatcher;
            CollectionChanged += (o, e) => OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }

        public void AddRange(IEnumerable<T> items)
        {
            this.CheckReentrancy();
            foreach (var item in items)
                this.Items.Add(item);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ClearThenAddRange(IEnumerable<T> items)
        {
            this.CheckReentrancy();
            this.Items.Clear();
            foreach (var item in items)
                this.Items.Add(item);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveAll(Func<T, bool> where)
        {
            this.CheckReentrancy();
            var itemsToRemove = this.Items.Where(where).ToArray();
            foreach (var i in itemsToRemove)
                this.Remove(i);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public T[] ToArrayThreadSafe()
        {
            return _dispatcherForCollection.Invoke(() => this.ToArray()); // this needs checking if it ACTUALLY WORKS!
        }
    }
}
