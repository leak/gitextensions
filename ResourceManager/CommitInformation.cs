using System;
using System.Net;
using GitCommands;

namespace ResourceManager
{
    public class CommitInformation
    {
        /// <summary>
        /// Private constructor
        /// </summary>
        private CommitInformation(string header, string body)
        {
            Header = header;
            Body = body;
        }

        public string Header { get; private set; }
        public string Body { get; private set; }

        /// <summary>
        /// Gets the commit info for module.
        /// </summary>
        /// <param name="module">Git module.</param>
        /// <param name="linkFactory"></param>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static CommitInformation GetCommitInfo(GitModule module, LinkFactory linkFactory, string sha1)
        {
            // TEMP, will be moved in the follow up refactor
            ICommitDataManager commitDataManager = new CommitDataManager(() => module);

            string error = "";
            CommitData data = commitDataManager.GetCommitData(sha1, ref error);
            if (data == null)
                return new CommitInformation(error, "");

            string header = data.GetHeader(linkFactory, false);
            string body = "\n" + WebUtility.HtmlEncode(data.Body.Trim());

            return new CommitInformation(header, body);
        }

        /// <summary>
        /// Gets the commit info from CommitData.
        /// </summary>
        /// <returns></returns>
        public static CommitInformation GetCommitInfo(CommitData data, LinkFactory linkFactory, bool showRevisionsAsLinks, GitModule module = null)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            string header = data.GetHeader(linkFactory, showRevisionsAsLinks);
            string body = "\n" + WebUtility.HtmlEncode(data.Body.Trim());

            if (showRevisionsAsLinks)
                body = GitRevision.Sha1HashShortRegex.Replace(body, match => ProcessHashCandidate(module, linkFactory, hash: match.Value));
            return new CommitInformation(header, body);
        }

        private static string ProcessHashCandidate(GitModule module, LinkFactory linkFactory, string hash)
        {
            if (module == null)
                return hash;
            string fullHash;
            if (!module.IsExistingCommitHash(hash, out fullHash))
                return hash;
            return linkFactory.CreateCommitLink(guid: fullHash, linkText: hash, preserveGuidInLinkText: true);
        }
    }
}