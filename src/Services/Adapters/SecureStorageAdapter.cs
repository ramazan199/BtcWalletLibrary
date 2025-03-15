using System;
using System.Collections.Generic;
using SecureStorage;

namespace BtcWalletLibrary.Services.Adapters
{
    internal class SecureStorageAdapter : ISecureStorageAdapter
    {
        public SecureStorageAdapter()
        {
            Storage secureStorage = new();
            ObjectStorage = new ObjectStorageAdapter(secureStorage.ObjectStorage);
            Values = new ValueStorageAdapter(secureStorage.Values);
        }

        public IObjectStorage ObjectStorage { get; }

        public IValueStorage Values { get; }
    }

    internal class ObjectStorageAdapter : IObjectStorage
    {
        private readonly ObjectStorage _adaptee;

        public ObjectStorageAdapter(ObjectStorage adaptee)
        {
            _adaptee = adaptee;
        }

        public void SaveObject(object obj, string key)
        {
            _adaptee.SaveObject(obj, key);       
        }

        public object LoadObject(Type type, string key)
        {
            return _adaptee.LoadObject(type, key);
        }

        public void DeleteObject(Type type, string key)
            => _adaptee.DeleteObject(type, key);
    }

    internal class ValueStorageAdapter : IValueStorage
    {
        private readonly Values _adaptee;
        private readonly Dictionary<Type, Func<string, object, object>> _getOperations;
        private readonly Dictionary<Type, Action<string, object>> _setOperations;

        public ValueStorageAdapter(Values adaptee)
        {
            _adaptee = adaptee;

            _setOperations = new Dictionary<Type, Action<string, object>>
        {
            { typeof(bool), (key, value) => _adaptee.Set(key, (bool)value) },
            { typeof(int), (key, value) => _adaptee.Set(key, (int)value) },
            { typeof(uint), (key, value) => _adaptee.Set(key, (uint)value) },
            { typeof(long), (key, value) => _adaptee.Set(key, (long)value) },
            { typeof(ulong), (key, value) => _adaptee.Set(key, (ulong)value) },
            { typeof(string), (key, value) => _adaptee.Set(key, (string)value) },
            { typeof(DateTime), (key, value) => _adaptee.Set(key, (DateTime)value) }
        };

            _getOperations = new Dictionary<Type, Func<string, object, object>>
        {
            { typeof(bool), (key, def) => _adaptee.Get(key, (bool)def) },
            { typeof(int), (key, def) => _adaptee.Get(key, (int)def) },
            { typeof(uint), (key, def) => _adaptee.Get(key, (uint)def) },
            { typeof(long), (key, def) => _adaptee.Get(key, (long)def) },
            { typeof(ulong), (key, def) => _adaptee.Get(key, (ulong)def) },
            { typeof(string), (key, def) => _adaptee.Get(key, (string)def) },
            { typeof(DateTime), (key, def) => _adaptee.Get(key, (DateTime)def) }
        };
        }

        public void Set<T>(string key, T value)
        {
            var type = typeof(T);
            if (!_setOperations.TryGetValue(type, out var operation))
            {
                throw new ArgumentException($"Unsupported type {type} for SecureStorage.Values.");
            }

            operation(key, value!);
        }

        public T Get<T>(string key, T defaultValue)
        {
            var type = typeof(T);
            if (!_getOperations.TryGetValue(type, out var operation))
            {
                throw new ArgumentException($"Unsupported type {type} for SecureStorage.Values.");
            }

            return (T)operation(key, defaultValue!);
        }
    }

    internal interface IObjectStorage
    {
        void SaveObject(object obj, string key);
        object LoadObject(Type type, string key);
        void DeleteObject(Type type, string key);
    }

    internal interface IValueStorage
    {
        void Set<T>(string key, T value);
        T Get<T>(string key, T defaultValue);
    }

    internal interface ISecureStorageAdapter
    {
        IObjectStorage ObjectStorage { get; }
        IValueStorage Values { get; }
    }
}