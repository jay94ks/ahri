namespace Ahri.Http
{
    public struct HttpHeader
    {
        /// <summary>
        /// Initialize a new <see cref="HttpHeader"/>.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public HttpHeader(string Key, string Value)
        {
            this.Key = Key;
            this.Value = Value;
        }

        /// <summary>
        /// Header Key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Header Value.
        /// </summary>
        public string Value { get; set; }
    }
}
