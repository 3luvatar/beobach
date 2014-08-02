using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;

namespace observableBindings
{
    public class ObjectMap
    {
        private static readonly Dictionary<Type, ObjectMap> mapCache = new Dictionary<Type, ObjectMap>();

        private ObjectMap()
        {
        }

        public static ObjectMap GetMap(Type type)
        {
            if (mapCache.ContainsKey(type)) return mapCache[type];
            var map = new ObjectMap();
            foreach (var propertyInfo in type.Properties())
            {
                var getter = propertyInfo.IsReadable() ? propertyInfo.DelegateForGetPropertyValue() : null;
                var setter = propertyInfo.IsWritable() ? propertyInfo.DelegateForSetPropertyValue() : null;
                var destinationType = propertyInfo.PropertyType;
                map.Properties.Add(new Property(destinationType, propertyInfo.Name, getter, setter));
            }
            mapCache.Add(type, map);
            return map;
        }

        public Type mapType;
        public List<Property> Properties = new List<Property>();
    }

    public class Property
    {
        public Property(Type destinationType, string propertyName, MemberGetter get, MemberSetter set)
        {
            this.destinationType = destinationType;
            this.propertyName = propertyName;
            _get = get;
            _set = set;
        }

        public override string ToString()
        {
            return propertyName;
        }

        private readonly Type destinationType;
        private readonly string propertyName;
        private readonly MemberGetter _get;
        private readonly MemberSetter _set;

        public MemberSetter Set
        {
            get { return _set; }
        }

        public MemberGetter Get
        {
            get { return _get; }
        }

        public string PropertyName
        {
            get { return propertyName; }
        }

        public Type DestinationType
        {
            get { return destinationType; }
        }
    }
}