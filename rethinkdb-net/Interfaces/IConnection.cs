using System;
using System.Collections.Generic;
using System.Net;

namespace RethinkDb
{
    public interface IConnection
    {
        IDatumConverterFactory DatumConverterFactory
        {
            get;
            set;
        }

        ILogger Logger
        {
            get;
            set;
        }

        void Connect(params EndPoint[] endpoints);

        T Run<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject);

        T Run<T>(ISingleObjectQuery<T> queryObject);

        IEnumerable<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject);

        IEnumerable<T> Run<T>(ISequenceQuery<T> queryObject);

        DmlResponse Run(IDatumConverterFactory datumConverterFactory, IDmlQuery queryObject);

        DmlResponse Run(IDmlQuery queryObject);

        DmlResponse Run<T>(IDatumConverterFactory datumConverterFactory, IWriteQuery<T> queryObject);

        DmlResponse Run<T>(IWriteQuery<T> queryObject);
    }
}

