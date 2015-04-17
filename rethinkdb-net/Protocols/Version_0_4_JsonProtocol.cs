using System;
using System.ComponentModel;

namespace RethinkDb.Protocols
{
    [ImmutableObject(true)]
    public class Version_0_4_Json : Version_0_3_Json
    {
        public static new readonly Version_0_4_Json Instance = new Version_0_4_Json();

        private readonly byte[] v04connectHeader;

        protected Version_0_4_Json()
        {
            var header = BitConverter.GetBytes((int)Spec.VersionDummy.Version.V0_4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, 0, header.Length);
            v04connectHeader = header;
        }

        public override byte[] ConnectHeader
        {
            get
            {
                return v04connectHeader;
            }
        }
    }
}
    