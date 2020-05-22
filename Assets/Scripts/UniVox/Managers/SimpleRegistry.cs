using System;
using System.Collections;
using System.Collections.Generic;

namespace UniVox.Managers
{
    public class SimpleRegistry<TValue> : IRegistry<string, int, TValue>
    {
        private readonly Dictionary<string, int> _keys;
        private readonly List<Tuple<string, int, TValue>> _records;

        public SimpleRegistry(int initialSize = 0)
        {
            _keys = new Dictionary<string, int>(initialSize);
            _records = new List<Tuple<string, int, TValue>>(initialSize);
        }

        public int Register(string key, TValue value)
        {
            if (!TryRegister(key, value, out var id))
                throw new Exception();
            return id;
        }

        public int Register(string key, TValue value, RegisterOptions registerOptions)
        {
            if (!TryRegister(key, value, out var id, registerOptions))
                throw new Exception();
            return id;
        }

        public bool TryRegister(string key, TValue value, out int identity)
        {
            if (_keys.ContainsKey(key))
            {
                identity = default;
                return false;
            }

            var index = _records.Count;
            identity = index;
            var record = new Tuple<string, int, TValue>(key, identity, value);

            _keys[key] = index;
            _records.Add(record);

            return true;
        }

        public bool TryRegister(string key, TValue value, out int identity, RegisterOptions registerOptions)
        {
            if (_keys.TryGetValue(key, out var recordIndex))
            {
                if (registerOptions == RegisterOptions.ReturnExistingKey)
                {
                    identity = _records[recordIndex].Item2;
                    return true;
                }
                else if (registerOptions == RegisterOptions.Overwrite)
                {
                    var old = _records[recordIndex];
                    _records[recordIndex] = new Tuple<string, int, TValue>(old.Item1,old.Item2,value);
                    identity = _records[recordIndex].Item2;
                    return true;
                }
                else
                {
                    identity = default;                   
                    return false;
                }
                

            }

            var index = _records.Count;
            identity = index;
            var record = new Tuple<string, int, TValue>(key, identity, value);

            _keys[key] = index;
            _records.Add(record);

            return true;
        }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var record in _records)
            {
                yield return record.Item3;
            }
        }


        public bool IsRegistered(string key)
        {
            return _keys.ContainsKey(key);
        }

        public bool IsRegistered(int key)
        {
            //we could use any IdentityTo*Map, they all SHOULD funciton the same
            return 0 <= key && key < _records.Count;
        }

        public bool TryGetValue(int id, out TValue value)
        {
            if (IsRegistered(id))
            {
                value = GetValue(id);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool TryGetValue(string key, out TValue value)
        {
            if (_keys.TryGetValue(key, out var id))
            {
                value = GetValue(id);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }


        public TValue GetValue(int id)
        {
            return _records[id].Item3;
        }

        public TValue GetValue(string key)
        {
            return GetValue(GetIdentity(key));
        }

        public bool TryGetIdentity(string key, out int identity)
        {
            return _keys.TryGetValue(key, out identity);
        }

        public int GetIdentity(string key)
        {
            return _keys[key];
        }

        public TValue this[string key] => GetValue(key);


        public TValue this[int identity] => GetValue(identity);

        public int Count => _records.Count;
    }
}