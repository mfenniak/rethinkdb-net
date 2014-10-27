using System;
using System.Configuration;

namespace RethinkDb.Configuration
{
    public class ClusterElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ClusterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClusterElement)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "cluster"; }
        }
    }
}
