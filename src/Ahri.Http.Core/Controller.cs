using Ahri.Http.Core.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core
{
    public abstract class Controller
    {
        /// <summary>
        /// Called to execute an action.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        protected virtual Task OnEndpointExecution(ControllerContext Context) => Task.CompletedTask;

        /// <summary>
        /// Called to invoke <see cref="OnEndpointExecution(ControllerContext)"/> method internally.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        internal Task OnEndpointExecutionInternal(ControllerContext Context) => OnEndpointExecution(Context);

        /// <summary>
        /// Create an action that returns the text result.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="MimeType">Default: text/plain.</param>
        /// <param name="Encoding"></param>
        /// <returns></returns>
        public static IHttpAction Text(string Content, string MimeType = null, Encoding Encoding = null) => new StringContent(Content, MimeType, Encoding);

        /// <summary>
        /// Create an action that returns the text result.
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="Content"></param>
        /// <param name="MimeType">Default: text/plain.</param>
        /// <param name="Encoding">Default: <see cref="Encoding.UTF8"/></param>
        /// <returns></returns>
        public static IHttpAction Text(int Status, string Content, string MimeType = null, Encoding Encoding = null) => new StringContent(Status, Content, MimeType, Encoding);

        /// <summary>
        /// Create an action that returns the json content result.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IHttpAction Json(object Value) => Text(JsonConvert.SerializeObject(Value), "application/json");

        /// <summary>
        /// Create an action that returns the json content result.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IHttpAction Json(int Status, object Value) => Text(Status, JsonConvert.SerializeObject(Value), "application/json");

        /// <summary>
        /// Create an action that returns the json content result.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IHttpAction Html(string Value) => Text(JsonConvert.SerializeObject(Value), "text/html");

        /// <summary>
        /// Create an action that returns the json content result.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IHttpAction Html(int Status, string Value) => Text(Status, Value, "text/html");

        /// <summary>
        /// Create an action that returns the status code result.
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="Phrase"></param>
        /// <returns></returns>
        public static IHttpAction Status(int Status, string Phrase = null) => new StatusCode(Status, Phrase);

        /// <summary>
        /// Create an action that returns the file content result.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="MimeType"></param>
        /// <returns></returns>
        public static IHttpAction File(string Path, string MimeType = null) => new FileContent(new FileInfo(Path), MimeType);

        /// <summary>
        /// Create an action that returns the redirect result.
        /// </summary>
        /// <param name="Where"></param>
        /// <param name="Permanently"></param>
        /// <returns></returns>
        public static IHttpAction RedirectTo(string Where, bool Permanently = false) => new RedirectTo(Where, Permanently);
    }
}
