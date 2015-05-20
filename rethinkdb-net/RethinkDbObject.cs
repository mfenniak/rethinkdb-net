using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace RethinkDb
{
    public class RethinkDbObject : DynamicObject
    {
        private readonly Dictionary<string, object> innerDictionary;

        public RethinkDbObject(Dictionary<string, object> data)
        {
            this.innerDictionary = data;
        }

        public Dictionary<string, object> InnerDictionary
        {
            get { return innerDictionary; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return innerDictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;
            if (indexes.Length != 1)
                return false;
            return innerDictionary.TryGetValue((string)indexes[0], out result);
        }
    }
}
