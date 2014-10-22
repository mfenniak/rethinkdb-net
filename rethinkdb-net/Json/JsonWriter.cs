using System;
using System.IO;
using System.Collections.Generic;

namespace RethinkDb.Json
{
    public class JsonWriter
    {
        private class State
        {
            public bool TrailingCommaNeeded = false;
        }

        private readonly TextWriter writer;
        private readonly Stack<State> stateStack = new Stack<State>();

        public JsonWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        private void WriteTrailingCommaIfNeeded()
        {
            if (stateStack.Count != 0 && stateStack.Peek().TrailingCommaNeeded)
                writer.Write(",");
        }

        private void SetTrailingCommaNeeded()
        {
            if (stateStack.Count != 0)
                stateStack.Peek().TrailingCommaNeeded = true;
        }

        private void SetTrailingCommaNotNeeded()
        {
            stateStack.Peek().TrailingCommaNeeded = false;
        }

        public void WriteStartObject()
        {
            WriteTrailingCommaIfNeeded();
            writer.Write("{");
            stateStack.Push(new State());
        }

        public void WritePropertyName(string propertyName)
        {
            WriteTrailingCommaIfNeeded();
            InternalWriteString(propertyName);
            writer.Write(":");
            SetTrailingCommaNotNeeded();
        }

        public void WriteEndObject()
        {
            writer.Write("}");
            stateStack.Pop();
        }

        public void WriteStartArray()
        {
            WriteTrailingCommaIfNeeded();
            writer.Write("[");
            stateStack.Push(new State());
        }

        public void WriteEndArray()
        {
            writer.Write("]");
            stateStack.Pop();
        }

        public void WriteString(string value)
        {
            WriteTrailingCommaIfNeeded();
            InternalWriteString(value);
            SetTrailingCommaNeeded();
        }

        private void InternalWriteString(string value)
        {
            writer.Write('\"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\"':
                        writer.Write("\\\"");
                        break;
                    case '\\':
                        writer.Write("\\\\");
                        break;
                    case '\b':
                        writer.Write("\\\b");
                        break;
                    case '\f':
                        writer.Write("\\\f");
                        break;
                    case '\n':
                        writer.Write("\\\n");
                        break;
                    case '\r':
                        writer.Write("\\\r");
                        break;
                    case '\t':
                        writer.Write("\\\t");
                        break;
                    default:
                        writer.Write(c);
                        break;
                }
            }
            writer.Write('\"');
        }

        public void WriteNumber(int value)
        {
            WriteTrailingCommaIfNeeded();
            writer.Write(value);
            SetTrailingCommaNeeded();
        }

        public void WriteNumber(double value)
        {
            WriteTrailingCommaIfNeeded();
            writer.Write(value);
            SetTrailingCommaNeeded();
        }

        public void WriteBoolean(bool value)
        {
            WriteTrailingCommaIfNeeded();
            if (value)
                writer.Write("true");
            else
                writer.Write("false");
            SetTrailingCommaNeeded();
        }

        public void WriteNull()
        {
            WriteTrailingCommaIfNeeded();
            writer.Write("null");
            SetTrailingCommaNeeded();
        }
    }
}
