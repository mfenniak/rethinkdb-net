wget https://github.com/rethinkdb/rethinkdb/raw/next/src/rdb_protocol/ql2.proto
mv ql2.proto rethinkdb_spec.proto

# protogen.exe comes from the protobuf-net project; specifically https://code.google.com/p/protobuf-net/downloads/detail?name=protobuf-net%20r622.zip&can=2&q=
# Two-step generation (https://code.google.com/p/protobuf-net/issues/detail?id=144)
protoc -I. rethinkdb_spec.proto -o rethinkdb_spec.bin
~/Development/protobuf-net-ProtoGen/protogen.exe -i:rethinkdb_spec.bin -o:rethinkdb_spec.cs
rm rethinkdb_spec.bin

# Adjust namespace
sed -i 's/rethinkdb_spec/RethinkDb.Spec/' rethinkdb_spec.cs

# "correct" the IsRequired flag of Query & Datum's type field, otherwise it's not transmitted when it is zero
sed -i 's/IsRequired = false, Name=@"type"/IsRequired = true, Name=@"type"/' rethinkdb_spec.cs
