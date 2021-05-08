using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ObservableHelpers.Observables
{
    public interface IAttributed
    {
        AttributeHolder Holder { get; }
    }

    public class AttributeHolder
    {
        #region Properties

        private Dictionary<string, object> attributes = new Dictionary<string, object>();

        #endregion

        #region Initializers

        public void Inherit(IAttributed attributed)
        {
            lock (attributes)
            {
                if (attributed != null) attributes = attributed.Holder.attributes;
            }
        }

        #endregion

        #region Methods

        public T GetAttribute<T>(T defaultValue = default, [CallerMemberName] string key = null)
        {
            lock (attributes)
            {
                if (!attributes.ContainsKey(key)) attributes.Add(key, defaultValue);
                return (T)attributes[key];
            }
        }

        public void SetAttribute<T>(T value, [CallerMemberName] string key = null)
        {
            lock (attributes)
            {
                if (!attributes.ContainsKey(key)) attributes.Add(key, value);
                else attributes[key] = value;
            }
        }

        public void DeleteAttribute(string key)
        {
            lock (attributes)
            {
                attributes.Remove(key);
            }
        }

        #endregion
    }
}
