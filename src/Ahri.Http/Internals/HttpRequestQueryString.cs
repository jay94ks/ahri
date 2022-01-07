using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Internals
{
    public class HttpRequestQueryString : IDictionary<string, string>
    {
        private Dictionary<string, string> m_KeyValues = new();
        private IHttpRequest m_Request;
        private string m_QueryString;

        /// <summary>
        /// Initialize a new <see cref="HttpRequestQueryString"/> instance.
        /// </summary>
        /// <param name="Request"></param>
        public HttpRequestQueryString(IHttpRequest Request) => m_Request = Request;

        /// <summary>
        /// Update Key Values from the <see cref="IHttpRequest"/> instance.
        /// </summary>
        private void UpdateFrom()
        {
            if (m_QueryString != m_Request.QueryString)
            {
                var KeyValues = (m_QueryString = m_Request.QueryString)
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
                    .Where(X => !string.IsNullOrWhiteSpace(X))
                    .Select(X => X.Split('=', 2, StringSplitOptions.None))
                    .ToArray();

                m_KeyValues.Clear();
                foreach (var Each in KeyValues)
                {
                    if (Each.Length != 2)
                        continue;

                    var Key = Each.First();
                    var Value = Each.Last();

                    if (string.IsNullOrWhiteSpace(Key) ||
                        string.IsNullOrWhiteSpace(Value))
                        continue;

                    m_KeyValues[Key] = Value;
                }
            }
        }

        /// <summary>
        /// Update QueryString to the <see cref="IHttpRequest"/> instance. 
        /// </summary>
        private void UpdateTo()
        {
            m_QueryString = m_Request.QueryString = string
                .Join('&', m_KeyValues.Select(X => $"{X.Key}={Uri.EscapeDataString(X.Value)}"));
        }

        /// <inheritdoc/>
        public string this[string Key]
        {
            get => m_KeyValues[Key];
            set => Add(Key, value);
        }

        /// <inheritdoc/>
        public ICollection<string> Keys
        {
            get
            {
                UpdateFrom();
                return m_KeyValues.Keys;
            }
        }

        /// <inheritdoc/>
        public ICollection<string> Values
        {
            get
            {
                UpdateFrom();
                return m_KeyValues.Values;
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                UpdateFrom();
                return m_KeyValues.Count;
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public void Clear()
        {
            m_KeyValues.Clear();
            UpdateTo();
        }

        /// <inheritdoc/>
        public void Add(string Key, string Value)
        {
            UpdateFrom();
            if (!m_KeyValues.TryGetValue(Key, out var Prev) || Prev != Value)
            {
                if (string.IsNullOrWhiteSpace(Value))
                {
                    Remove(Key);
                    return;
                }

                m_KeyValues[Key] = Value;
                UpdateTo();
            }
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, string> Item) => Add(Item.Key, Item.Value);

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, string> Item)
        {
            UpdateFrom();
            return m_KeyValues.Contains(Item);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string Key)
        {
            UpdateFrom();
            return m_KeyValues.ContainsKey(Key);
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) 
            => ((ICollection<KeyValuePair<string, string>>)m_KeyValues).CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            UpdateFrom();
            if (m_KeyValues.Remove(key))
            {
                UpdateTo();
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool TryGetValue(string Key, [MaybeNullWhen(false)] out string Value)
        {
            UpdateFrom();
            return m_KeyValues.TryGetValue(Key, out Value);
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> Item)
        {
            UpdateFrom();
            if (((ICollection<KeyValuePair<string, string>>)m_KeyValues).Remove(Item))
            {
                UpdateTo();
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            UpdateFrom();
            return m_KeyValues.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
