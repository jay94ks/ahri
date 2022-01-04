using Ahri.Http.Core.Actions;
using Ahri.Http.Core.Middlewares.Internals;
using Ahri.Http.Core.Routing.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Middlewares
{
    public class StaticFiles : IHttpMiddlewareBuilder
    {
        private List<(string Path, DirectoryInfo Directory)> m_Targets = new();
        private List<Func<FileInfo, bool>> m_Filters = new();

        /// <summary>
        /// Map <see cref="DirectoryInfo"/> to specific <paramref name="Path"/>.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Directory"></param>
        /// <returns></returns>
        public StaticFiles Map(string Path, DirectoryInfo Directory)
        {
            m_Targets.Add((RouterBuilder.Normalize(Path), Directory));
            return this;
        }

        /// <summary>
        /// Map <see cref="DirectoryInfo"/> to specific <paramref name="Path"/>.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Directory"></param>
        /// <returns></returns>
        public StaticFiles Map(string Path, string Directory)
            => Map(Path, new DirectoryInfo(Directory));

        /// <summary>
        /// Adds a filter that make specific files to be unaccessible.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public StaticFiles Except(Func<FileInfo, bool> Filter)
        {
            m_Filters.Add(Filter);
            return this;
        }

        /// <inheritdoc/>
        public Func<IHttpContext, Func<Task>, Task> Build()
        {
            /* Longest match. */
            m_Targets.Sort((A, B) => -string.Compare(A.Path, B.Path));

            /* Build delegates. */
            var Filter = PredicateDelegates.MergeAnd(m_Filters);
            var Middleware = MiddlewareDelegates.Merge(m_Targets
                .Select(X => MakeFileProvider(X.Path, X.Directory, Filter)));

            /* Return empty delegate if no files are configured. */
            if (Middleware != null)
                return Middleware;

            return (Http, Next) => Next();
        }

        private static Func<IHttpContext, Func<Task>, Task> MakeFileProvider(string HttpPath, DirectoryInfo Directory, Func<FileInfo, bool> Filter)
        {
            return (Http, Next) =>
            {
                var Action = MakeFileAction(Http, HttpPath, Directory, Filter);
                if (Action != null)
                    return Action.InvokeAsync(Http);

                return Next();
            };
        }

        private static IHttpAction MakeFileAction(IHttpContext Context, string HttpPath, DirectoryInfo Directory, Func<FileInfo, bool> Filter)
        {
            var Request = Context.Request;
            if (Request.PathString.StartsWith($"{HttpPath}/"))
            {
                var Subpath = Request.PathString.Substring(HttpPath.Length + 1).Trim();
                var FileInfo = new FileInfo(Path.Combine(Directory.FullName, Uri.UnescapeDataString(Subpath)));

                if (FileInfo.Exists && (Filter is null || Filter(FileInfo)))
                    return new FileContent(FileInfo);
            }

            return null;
        }
    }
}
