using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers
{
    public class PropertyHolder
    {
        public object Property { get; set; }
        public string Key { get; set; }
        public string PropertyName { get; set; }
        public string Group { get; set; }

        public bool UpdateProperty(object property)
        {
            if (!(Property?.Equals(property) ?? property == null))
            {
                Property = property;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CopyToSelf(object property, string key, string propertyName, string group)
        {
            var hasChanges = false;
            if (Key != key)
            {
                Key = key;
                hasChanges = true;
            }

            if (PropertyName != propertyName)
            {
                PropertyName = propertyName;
                hasChanges = true;
            }

            if (Group != group)
            {
                Group = group;
                hasChanges = true;
            }

            if (UpdateProperty(property))
            {
                Property = property;
                hasChanges = true;
            }
            return hasChanges;
        }

        public bool CopyToSelf(PropertyHolder propertyHolder)
        {
            return CopyToSelf(propertyHolder.Property, propertyHolder.Key, propertyHolder.PropertyName, propertyHolder.Group);
        }

        public override bool Equals(object obj) => Equals(obj as PropertyHolder);

        public bool Equals(PropertyHolder prop)
        {
            if (prop is null)
            {
                return false;
            }

            if (ReferenceEquals(this, prop))
            {
                return true;
            }

            if (GetType() != prop.GetType())
            {
                return false;
            }

            return
                (Key == prop.Key) &&
                (PropertyName == prop.PropertyName) &&
                (Group == prop.Group) &&
                (Property?.Equals(prop.Property) ?? prop.Property == null);
        }

        public override int GetHashCode() => (Key, PropertyName, Group, Property).GetHashCode();

        public static bool operator ==(PropertyHolder left, PropertyHolder right)
        {
            if (left is null)
            {
                if (right is null)
                {
                    return true;
                }

                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(PropertyHolder lhs, PropertyHolder rhs) => !(lhs == rhs);

    }
}
