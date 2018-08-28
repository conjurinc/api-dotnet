﻿// <copyright file="Policy.cs" company="Conjur Inc.">
//     Copyright (c) 2016 Conjur Inc. All rights reserved.
// </copyright>
// <summary>
// Conjur Policy entity
// </summary>
namespace Conjur
{
    using System.IO;
    using System.Net;

    public class Policy : Resource
    {
        private readonly string path;

        internal Policy(Client client, string name)
                : base(client, Constants.KIND_POLICY, name)
        {
                 this.path = string.Join("/", 
                                         new string[] 
                                         {
                                            "policies",
                                            WebUtility.UrlEncode(client.GetAccountName()),
                                            Constants.KIND_POLICY,
                                            WebUtility.UrlEncode(name)
                                         });
        }

        /// <summary>
        /// Loading a Conjur policy MAML stream structure
        /// into given policy name, over REST POST request.
        /// </summary>
        /// <param name="policyContent">Stream valid MAML Conjur policy strature.</param>
        /// <returns>Policy creation response as a stream.</returns>
        public Stream LoadPolicy(Stream policyContent)
        {
            WebRequest loadPolicyRequest = Client.AuthenticatedRequest(this.path);
            loadPolicyRequest.Method = WebRequestMethods.Http.Post;
            loadPolicyRequest.ContentLength = policyContent.Length;
            loadPolicyRequest.Timeout = ApiConfigurationManager.GetInstance().GetHttpRequestTimeout();

            policyContent.Seek(0, SeekOrigin.Begin);

            using (Stream reqStream = loadPolicyRequest.GetRequestStream())
            {
                policyContent.CopyTo(reqStream);
                WebResponse loadPolicyResponse = loadPolicyRequest.GetResponse();
                return loadPolicyResponse.GetResponseStream();
            }
        }
    }
}