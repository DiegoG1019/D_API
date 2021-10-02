using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Configurer.Lib
{
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new(prop));
        protected void NotifySet<T>(ref T prop, T value, [CallerMemberName] string? propname = null)
        {
            prop = value;
            PropertyChanged?.Invoke(this, new(propname));
        }
        
        protected internal NotifyPropertyChanged() { }
    }
}
