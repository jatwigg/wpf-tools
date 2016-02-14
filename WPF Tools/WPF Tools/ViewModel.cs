using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Tools
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        protected void Set(object value = default(object), [CallerMemberName] string propertyName = null)
        {
            if (_properties.ContainsKey(propertyName))
            {
                if (_properties[propertyName] != value)
                {
                    _properties[propertyName] = value;
                    NotifyPropertyChanged(propertyName);
                }
            }
            else
            {
                _properties.Add(propertyName, value);
                NotifyPropertyChanged(propertyName);
            }
        }

        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            if (_properties.ContainsKey(propertyName))
            {
                return (T)_properties[propertyName];
            }
            return default(T);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
