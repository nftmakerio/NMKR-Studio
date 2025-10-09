using System;
using System.Text;
using System.Runtime.CompilerServices;

namespace nanoJSON
{
    public interface IJSONObject : IDisposable
    {
        IJSONObject addObject();
        IJSONObject addObject(string key);
        IJSONArray addArray(string key);
        IJSONObject addArrayObject(string key);

        void addNumber(string key, double number);
        void addString(string key, string text);
        void addBoolean(string key, bool bo);
        void addNull(string key);
        void addKeyValuesBytesField(string key, string value);
        void addArrayBytesField(string[] values);
    }

    public interface IJSONArray : IDisposable
    {
        IJSONObject addObject();
        IJSONObject addObject(string key);
        IJSONArray addArray(string key);
        void addNumber(double number);
        void addString(string text);
        void addBoolean(bool bo);
        void addNull();
    }

    abstract class JSONElement : IDisposable
    {
        private JSONElement parent = null;
        protected bool empty = true;
        private bool isCurrent = true;

        static readonly char[] specialChars = { '\\', '"', '/', '\n', '\r', '\t', '\b', '\f' };
        static readonly char[] replaceChars = { '\\', '"', '/', 'n', 'r', 't', 'b', 'f' };

        protected JSONElement(JSONElement par, string key)
        {
            parent = par;
            if (parent != null)
                parent.isCurrent = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void validateIsCurrent()
        {
            if (!isCurrent) throw new InvalidOperationException("Attempt to add to non-current element");
        }

        protected virtual void add(bool newLine, string str, int levelDelta = 0) => parent.add(newLine, str, levelDelta);

        protected void addKeyText(string key, string text, int levelDelta = 0)
        {
            validateIsCurrent();
            if (!empty)
                this.add(false, ",");
            if (key == "")
                add(true, text, levelDelta);
            else
                add(true, string.Format("\"{0}\": {1}", escape2(key), text), levelDelta);

            if (levelDelta == 0)
                empty = false;
        }

        public void Dispose()
        {
            if (!isCurrent)
                throw new InvalidOperationException("Attempt to close non-current element");
            if (parent != null)
                parent.isCurrent = true;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this is JSONObject)
                    add(!empty, "}", -1);
                else if (this is JSONArray)
                    add(!empty, "]", -1);
                if (parent != null)
                    parent.isCurrent = true;
                parent = null;
            }
        }

        static string escape2(string str)
        {
            int n = str.IndexOfAny(specialChars);
            if (n == -1)
                return str;

            string ret = "";
            int last = 0;

            do
            {
                for (int p = 0; p < specialChars.Length; p++)
                    if (str[n] == specialChars[p])
                    {
                        ret += str.Substring(last, n - last) + '\\' + replaceChars[p];
                        break;
                    }
                last = n + 1;
                n = str.IndexOfAny(specialChars, last);
            }
            while (n > 0);
            ret += str.Substring(last);
            return ret;
        }


        static string escape(string str)
        {
            string ret = "";

            for (int n = 0; n < str.Length; n++)
            {
                char c = str[n];
                switch (c)
                {
                    case '"':
                    case '\\':
                    case '/':
                        ret += '\\';
                        ret += c;
                        break;
                    case '\b':
                        ret += '\\';
                        ret += 'b';
                        break;
                    case '\f':
                        ret += '\\';
                        ret += 'f';
                        break;
                    case '\n':
                        ret += '\\';
                        ret += 'n';
                        break;
                    case '\r':
                        ret += '\\';
                        ret += 'r';
                        break;
                    case '\t':
                        ret += '\\';
                        ret += 't';
                        break;
                    default:
                        ret += c;
                        break;
                }
            }
            return ret;
        }

        protected class JSONObject : JSONElement, IJSONObject
        {
            protected internal JSONObject(JSONElement par, string key) : base(par, key) => addKeyText(key, "{", 1);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private string validateKey(string key)
            {
                if (key == "")
                    throw new ArgumentException("Key cannot be empty");
                return key;
            }
            public void addNumber(string key, double number) => addKeyText(validateKey(key), string.Format("{0}", number));
            public void addString(string key, string text) => addKeyText(validateKey(key), string.Format("\"{0}\"", escape2(text)));
            public void addBoolean(string key, bool bo) => addKeyText(validateKey(key), string.Format("{0}", bo).ToLowerInvariant());
            public void addNull(string key) => addKeyText(validateKey(key), "null");
            public void addKeyValuesBytesField(string key, string value)
            {
                using (var k = addObject("k"))
                {
                    k.addString("bytes", key.ToHex());
                }

                using (var v = addObject("v"))
                {
                    v.addString("bytes", value.ToHex());
                }
            }

            public void addArrayBytesField(string[] values)
            {
                using (var v = addObject("array"))
                {
                    foreach (var value in values)
                        v.addString("bytes", value.ToHex());
                }
            }

            public IJSONObject addObject()
            {
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONObject ret = new JSONObject(this, "");
                empty = false;
                return ret;
            }
            public IJSONObject addObject(string key)
            {
                validateKey(key);
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONObject ret = new JSONObject(this, key);
                empty = false;
                return ret;
            }
            public IJSONArray addArray(string key)
            {
                validateKey(key);
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONArray ret = new JSONArray(this, key);
                empty = false;
                return ret;
            }

            public IJSONObject addArrayObject(string key)
            {
                using (var fields = addArray(key))
                {
                    var fieldsobject = fields.addObject();
                    return fieldsobject;
                }
            }
        }

        protected class JSONArray : JSONElement, IJSONArray
        {
            protected internal JSONArray(JSONElement par, string key) : base(par, key) => addKeyText(key, "[", 1);

            public void addNumber(double number) => addKeyText("", string.Format("{0}", number));
            public void addString(string text) => addKeyText("", string.Format("\"{0}\"", escape2(text)));
            public void addBoolean(bool bo) => addKeyText("", string.Format("{0}", bo).ToLowerInvariant());
            public void addNull() => addKeyText("", "null");
            private string validateKey(string key)
            {
                if (key == "")
                    throw new ArgumentException("Key cannot be empty");
                return key;
            }
            public IJSONObject addObject()
            {
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONObject ret = new JSONObject(this, "");
                empty = false;
                return ret;
            }
            public IJSONObject addObject(string key)
            {
                validateKey(key);
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONObject ret = new JSONObject(this, key);
                empty = false;
                return ret;
            }
            public IJSONArray addArray(string key)
            {
                validateKey(key);
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONArray ret = new JSONArray(this, key);
                empty = false;
                return ret;
            }
        }

        public class JSONDocument : JSONElement, IJSONArray
        {
            private int level = 0;
            private uint tabSize = 4;
            private bool format = true;
            private StringBuilder output = new StringBuilder();

            public static JSONDocument newDoc(bool useFormat = true) => new JSONDocument(useFormat);

            protected JSONDocument(bool useFormat = true) : base(null, "") { format = useFormat; }

            protected new void addKeyText(string key, string text, int levelDelta = 0)
            {
                if (!empty)
                    throw new InvalidOperationException("Attempt to add second element to root");
                base.addKeyText(key, text, levelDelta);
            }

            protected override void add(bool newLine, string str, int levelDelta = 0)
            {
                if (format)
                {
                    if (levelDelta < 0)
                        level += levelDelta;
                    if (newLine)
                    {
                        if (!empty)
                            output.Append('\n');
                        output.Append(' ', (int)(level * tabSize));
                    }
                    output.Append(str);
                    if (levelDelta > 0)
                        level += levelDelta;
                }
                else
                    output.Append(str);
            }

            public string getOutput()
            {
                if (!isCurrent)
                    throw new InvalidOperationException("Attempt to obtain output with elements still open");
                return output.ToString();
            }
            public void addNumber(double number) => addKeyText("", string.Format("{0}", number));
            public void addString(string text) => addKeyText("", string.Format("\"{0}\"", text));
            public void addBoolean(bool bo) => addKeyText("", string.Format("{0}", bo).ToLowerInvariant());
            public void addNull() => addKeyText("", "null");
            private string validateKey(string key)
            {
                if (key == "")
                    throw new ArgumentException("Key cannot be empty");
                return key;
            }
            public IJSONObject addObject()
            {
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONObject ret = new JSONObject(this, "");
                empty = false;
                return ret;
            }
            public IJSONObject addObject(string key)
            {
                validateKey(key);
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONObject ret = new JSONObject(this, key);
                empty = false;
                return ret;
            }
            public IJSONArray addArray(string key)
            {
                validateKey(key);
                validateIsCurrent();
                if (!empty)
                    add(false, ",");
                JSONArray ret = new JSONArray(this, key);
                empty = false;
                return ret;
            }
        }
    }
}