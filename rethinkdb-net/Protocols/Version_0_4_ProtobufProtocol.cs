using System;
using System.ComponentModel;

namespace RethinkDb.Protocols
{
    [ImmutableObject(true)]
    public class Version_0_4_Protobuf : Version_0_3_Protobuf
    {
        public static new readonly Version_0_4_Protobuf Instance = new Version_0_4_Protobuf();

        private readonly byte[] v04connectHeader;

        protected Version_0_4_Protobuf()
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
