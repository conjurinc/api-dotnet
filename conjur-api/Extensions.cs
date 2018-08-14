﻿// <copyright file="Extensions.cs" company="Conjur Inc.">
//     Copyright (c) 2016 Conjur Inc. All rights reserved.
// </copyright>
// <summary>
//     Utility extension methods.
// </summary>
namespace Conjur
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utility extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Imports all certificates from a PEM file.
        /// </summary>
        /// <param name="collection">Certificate collection.</param>
        /// <param name="fileName">PEM file path.</param>
        public static void ImportPem(
            this X509Certificate2Collection collection,
            string fileName)
        {
            const string HEADER = "-----BEGIN CERTIFICATE-----";
            const string FOOTER = "-----END CERTIFICATE-----";
            var re = new Regex(HEADER + "(.*?)" + FOOTER, RegexOptions.Singleline);
            foreach (Match match in re.Matches(File.ReadAllText(fileName)))
            {
                collection.Import(Convert.FromBase64String(match.Groups[1].Value));
            }
        }

        /// <summary>
        /// Read the response of a WebRequest.
        /// </summary>
        /// <returns>The contents of the response.</returns>
        /// <param name="request">Request to read from.</param>
        internal static string Read(this WebRequest request)
        {
            using (var reader
                = new StreamReader(request.GetResponse().GetResponseStream()))
                return reader.ReadToEnd();
        }

        internal static bool VerifyWithExtraRoots(
            this X509Chain chain,
            X509Certificate certificate,
            X509Certificate2Collection extraRoots)
        {
            chain.ChainPolicy.ExtraStore.AddRange(extraRoots);
            if (chain.Build(new X509Certificate2(certificate)))
                return true;
            else
            {
                // .NET returns UntrustedRoot status flag if the certificate is not in
                // the SYSTEM trust store. Check if it's the only problem with the chain.
                var onlySystemUntrusted = 
                    chain.ChainStatus.Length == 1 &&
                    chain.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot;

                // Sanity check that indeed that is the only problem with the root
                // certificate.
                var rootCert = chain.ChainElements[chain.ChainElements.Count - 1];
                var rootOnlySystemUntrusted = 
                    rootCert.ChainElementStatus.Length == 1 &&
                    rootCert.ChainElementStatus[0].Status
                    == X509ChainStatusFlags.UntrustedRoot;

                // Double check it's indeed one of the extra roots we've been given.
                var rootIsUserTrusted = extraRoots.Contains(rootCert.Certificate);

                return 
                    onlySystemUntrusted && rootOnlySystemUntrusted && rootIsUserTrusted;
            }
        }
    }
}