using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1 {
    public class DatabaseRow : IEnumerable {

        private Dictionary<string, DatabaseCaster> m_internalDict = new Dictionary<string, DatabaseCaster>();

        public object this[string sKey] {
            get {
                if (m_internalDict.ContainsKey(sKey)) {
                    return m_internalDict[sKey].Value;
                }
                else {
#if DEBUG
                    Console.Out.WriteLine("DatabaseRow: Field '" + sKey + "' not found!");
#endif
                }
                return null;
            }

            set {
                if (!m_internalDict.ContainsKey(sKey)) {
                    m_internalDict.Add(sKey, new DatabaseCaster(value is DBNull ? null : value));
                }
                else {
                    m_internalDict[sKey].Value = value;
                }
            }
        }

        public bool IsFieldNull(string sFieldname) {
            if (!m_internalDict.TryGetValue(sFieldname, out DatabaseCaster val)) {
#if DEBUG
                Console.Out.WriteLine("DatabaseRow: Field '" + sFieldname + "' not found!");
#endif
                return true;
            }
            return val.IsNull();
        }

        public DatabaseCaster AutoCast(string sFieldname) {
            if (!m_internalDict.TryGetValue(sFieldname, out DatabaseCaster val)) {
#if DEBUG
                Console.Out.WriteLine("DatabaseRow: Field '" + sFieldname + "' not found!");
#endif
            }
            return val;
        }

 
        public IEnumerator GetEnumerator() {
            foreach (KeyValuePair<string, DatabaseCaster> kvp in m_internalDict) {
                yield return kvp;
            }
        }

        public DatabaseRow Add(string sKey, object oValue) {
            this[sKey] = oValue;
            return this;
        }

        public DatabaseRow Remove(string sKey) {
            m_internalDict.Remove(sKey);
            return this;
        }

        public DatabaseRow Subset(string key, bool removeKeyFromIndex = true) {
            Dictionary<string, DatabaseCaster> subset = new Dictionary<string, DatabaseCaster>();
            foreach (var kvp in m_internalDict) {
                if (kvp.Key.StartsWith(key)) {
                    subset.Add(removeKeyFromIndex ? kvp.Key.Replace(key, "") : kvp.Key, kvp.Value);
                }
            }
            return new DatabaseRow { m_internalDict = subset };
        }

        public override string ToString() {
            List<string> lFields = new List<string>();
            foreach (KeyValuePair<string, DatabaseCaster> kvp in m_internalDict) {
                lFields.Add("\"" + kvp.Key + "\" : \"" + kvp.Value.Value + "\"");
            }
            return "DatabaseRow { " + String.Join(", ", lFields) + " }";
        }
    }

    public class DatabaseCaster {
        public object Value { get; set; }

        public DatabaseCaster(object val) {
            Value = val;
        }

        public bool IsNull() {
            return Value == null;
        }


        public static implicit operator double(DatabaseCaster x) {
            return x.Value != null ? Convert.ToDouble(x.Value) : 0;
        }
        public static implicit operator ulong(DatabaseCaster x) {
            return x.Value != null ? Convert.ToUInt64(x.Value) : 0;
        }
        public static implicit operator string(DatabaseCaster x) {
            return x.Value != null ? x.Value.ToString() : "";
        }
        public static implicit operator short(DatabaseCaster x) {
            return x.Value != null ? Convert.ToInt16(x.Value) : (short)0;
        }
        public static implicit operator DateTime(DatabaseCaster x) {
            return x.Value != null ? Convert.ToDateTime(x.Value) : DateTime.MinValue;
        }
        public static implicit operator decimal(DatabaseCaster x) {
            return x.Value != null ? Convert.ToDecimal(x.Value) : (decimal)0;
        }
        public static implicit operator bool(DatabaseCaster x) {
            return x.Value != null ? Convert.ToBoolean(x.Value) : false;
        }
    }

    public class DatabaseResult : IEnumerable {

        public ulong ID { get; set; }
        public bool Success { get; set; }
        public int Affected { get; set; }
        public DatabaseRow Result { get; set; }
        public List<DatabaseRow> ResultSet { get; set; }
        public Exception Exception { get; set; }

        public DatabaseResult() {
            ID = 0; Success = false; Affected = 0; Result = null; ResultSet = new List<DatabaseRow>(); Exception = null;
        }

        public object this[string sKey] {
            get {
                return Result?[sKey];
            }
        }

        public static implicit operator bool(DatabaseResult x) {
            return x.Success;
        }

        public IEnumerator GetEnumerator() {
            foreach (DatabaseRow r in ResultSet) {
                yield return r;
            }
        }
    }
}
