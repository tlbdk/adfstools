using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.IdentityServer.PowerShell.Commands;
using Microsoft.IdentityServer.PowerShell.Resources;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace ADFSMetadataTool
{
    class Program
    {
        static int Main(string[] args)
        {
            string exepath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            string samplespath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "../../../samples");

            bool show_help = false;

            string command = null;
            var p = new OptionSet() {
                { "h|help", v => show_help = true },
            };

            var arguments = new List<string>();
            try
            {
                if (args.Length > 1)
                {
                    command = args[0];
                    args = ArrayRemoveAt(args, 0); // shift one value for the array
                    arguments = p.Parse(args);
                }
                else
                {
                    p.Parse(args);
                }

                if (show_help || (command == "import" && arguments.Count == 0) || (command == "export" && arguments.Count != 1))
                {
                    ShowHelp(p);
                    return 255;
                }

            }
            catch (OptionException e)
            {
                Console.WriteLine("Unknown options or missing command");
                return 255;
            }

            switch (command)
            {
                case "import":
                    foreach (var argument in arguments)
                    {
                        if (File.Exists(argument))
                        {
                            Console.WriteLine("{0}", argument);
                            ImportADFSMetadata(argument);
                        }
                        else if (Directory.Exists(argument))
                        {
                            string[] files = Directory.GetFiles(argument, "*.xml", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                Console.WriteLine("{0}", file);
                                ImportADFSMetadata(file);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Could not find arguments {0}", argument);
                        }
                    }

                    return 0;
                case "export":
                    ExportADFSMetadata(arguments[0]);
                    return 0;

                default:
                    Console.WriteLine("Unknown command");
                    ShowHelp(p);
                    return 255;
            }
        }

        public static void ImportADFSMetadata(string filename)
        {
            var metadata = ADFSMetadata.DeserializeFromFile(filename);

            AddRelyingPartyTrustCommand addRelyingPartyTrustCommand = new AddRelyingPartyTrustCommand();
            addRelyingPartyTrustCommand.MetadataFile = filename;
            addRelyingPartyTrustCommand.Name = metadata.entityID;

            // Set default permissions
            addRelyingPartyTrustCommand.IssuanceAuthorizationRules =
                "@RuleTemplate = \"AllowAllAuthzRule\"\r\n"
                + " => issue(Type = \"http://schemas.microsoft.com/authorization/claims/permit\", Value = \"true\");";

            if (metadata.Extensions != null)
            {
                if (!String.IsNullOrEmpty(metadata.Extensions.displayName))
                {
                    addRelyingPartyTrustCommand.Name = metadata.Extensions.displayName;
                }
                if (!String.IsNullOrEmpty(metadata.Extensions.issuanceTransformRules))
                {
                    addRelyingPartyTrustCommand.IssuanceTransformRules = metadata.Extensions.issuanceTransformRules;
                }
                if (!String.IsNullOrEmpty(metadata.Extensions.issuanceAuthorizationRules))
                {
                    addRelyingPartyTrustCommand.IssuanceAuthorizationRules = metadata.Extensions.issuanceAuthorizationRules;
                }
                if (!String.IsNullOrEmpty(metadata.Extensions.signatureAlgorithm))
                {
                    addRelyingPartyTrustCommand.SignatureAlgorithm = metadata.Extensions.signatureAlgorithm;
                }
            }

            

            IEnumerable result = addRelyingPartyTrustCommand.Invoke();
            try
            {
                result.GetEnumerator().MoveNext();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Relying party cannot be updated.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return;
            }

            // Set signatureAlgorithm as it seems to impossible to set it the same time adding a metadata file
            if (metadata.Extensions != null && !String.IsNullOrEmpty(metadata.Extensions.signatureAlgorithm))
            {
                SetRelyingPartyTrustCommand setRelyingPartyTrustCommand = new SetRelyingPartyTrustCommand();
                setRelyingPartyTrustCommand.TargetName = addRelyingPartyTrustCommand.Name;
                setRelyingPartyTrustCommand.SignatureAlgorithm = metadata.Extensions.signatureAlgorithm;
                result = setRelyingPartyTrustCommand.Invoke();
                try
                {
                    result.GetEnumerator().MoveNext();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Relying party cannot be updated.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return;
                }
            }
        }

        public static void ExportADFSMetadata(string outdir, string identifier = null)
        {
            // Export all Relying Parties from ADFS
            Console.WriteLine("Get all relaying parties");
            GetRelyingPartyTrustCommand getRelyingPartyTrustCommand = new GetRelyingPartyTrustCommand();
            IEnumerable result = getRelyingPartyTrustCommand.Invoke();
            if (identifier != null)
            {
                getRelyingPartyTrustCommand.Identifier = new string[1] { identifier };
            }

            foreach (object obj in result)
            {
                var rp = obj as RelyingPartyTrust;
                foreach (var id in rp.Identifier)
                {
                    // Skip if disabled
                    if (!rp.Enabled) { continue; }

                    var filename = MakeSafeFilename("metadata-" + id.Replace("https://", "").Replace("http://", "").TrimEnd(new[] { '/' }), '-') + ".xml";

                    Console.WriteLine("Exporting " + rp.Name + " to " + filename);
                    var metadata = new ADFSMetadata("abcd1", id);
                    metadata.Extensions = new EntityDescriptorExtensions(rp.Name, rp.IssuanceTransformRules.Trim(), rp.IssuanceAuthorizationRules.Trim());
                    metadata.Extensions.signatureAlgorithm = rp.SignatureAlgorithm;

                    if (rp.SamlEndpoints.Length > 0)
                    {
                        metadata.SPSSODescriptor = new EntityDescriptorSPSSODescriptor();
                        metadata.SPSSODescriptor.NameIDFormat = new string[] { 
                            "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress",
                            "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
                            "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified",
                            "urn:oasis:names:tc:SAML:1.1:nameid-format:persistent",
                            "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName"
                        };

                        var consumers = new List<EntityDescriptorSPSSODescriptorAssertionConsumerService>();
                        var logouts = new List<EntityDescriptorSPSSODescriptorSingleLogoutService>();
                        for (int i = 0; i < rp.SamlEndpoints.Length; i++)
                        {
                            if (rp.SamlEndpoints[i].Protocol == "SAMLAssertionConsumer")
                            {
                                consumers.Add(new EntityDescriptorSPSSODescriptorAssertionConsumerService()
                                {
                                    Binding = rp.SamlEndpoints[i].BindingUri.ToString(),
                                    index = rp.SamlEndpoints[i].Index,
                                    isDefault = rp.SamlEndpoints[i].IsDefault,
                                    Location = rp.SamlEndpoints[i].Location.ToString()
                                });
                            }
                            else if (rp.SamlEndpoints[i].Protocol == "SAMLLogout")
                            {
                                logouts.Add(new EntityDescriptorSPSSODescriptorSingleLogoutService()
                                {
                                    Binding = rp.SamlEndpoints[i].BindingUri.ToString(),
                                    Location = rp.SamlEndpoints[i].Location.ToString()
                                });
                            }
                        }

                        metadata.SPSSODescriptor.AssertionConsumerService = consumers.ToArray();
                        metadata.SPSSODescriptor.SingleLogoutService = logouts.ToArray();
                    }

                    if (rp.WSFedEndpoint != null)
                    {
                        metadata.RoleDescriptor = new WSFederationApplicationServiceTypeRoleDescriptor();
                        metadata.RoleDescriptor.TargetScopes = new TargetScope[1] { new TargetScope(rp.WSFedEndpoint.ToString()) };
                        metadata.RoleDescriptor.PassiveRequestorEndpoint = new PassiveRequestorEndpoint(rp.WSFedEndpoint.ToString());
                    }

                    if (!String.IsNullOrEmpty(rp.OrganizationInfo))
                    {
                        string orgname = null;
                        string orgurl = null;
                        string techname = null;
                        string techemail = null;
                        string techphone = null;

                        var match = Regex.Match(rp.OrganizationInfo, @"\s*Organization Names:\s+(.+?)\s*\r?\n\r?\n", RegexOptions.Singleline);
                        if (match.Success)
                        {
                            orgname = match.Groups[1].Value;
                        }
                        match = Regex.Match(rp.OrganizationInfo, @"\s*Organization URLs:\s+(.+?)\s*\r?\n\r?\n", RegexOptions.Singleline);
                        if (match.Success)
                        {
                            orgurl = match.Groups[1].Value;
                        }

                        if (!String.IsNullOrEmpty(orgname))
                        {
                            metadata.Organization = new EntityDescriptorOrganization(orgname, orgname, orgurl);
                        }
                        else
                        {
                            metadata.Organization = new EntityDescriptorOrganization(rp.OrganizationInfo, null, orgurl);
                        }

                        match = Regex.Match(rp.OrganizationInfo, @"\s*Technical Contact:\s+Name:\s*(.+?)\s*Emails:\s+(.+?)\s+Telephones:\s+(.+?)\s+", RegexOptions.Singleline);
                        if (match.Success)
                        {
                            techname = match.Groups[1].Value;
                            techemail = match.Groups[2].Value;
                            techphone = match.Groups[3].Value;
                        }

                        if (!String.IsNullOrEmpty(techname))
                        {
                            metadata.ContactPerson = new EntityDescriptorContactPerson();
                            metadata.ContactPerson.GivenName = techname;
                            metadata.ContactPerson.TelephoneNumber = techphone;
                            metadata.ContactPerson.EmailAddress = techemail;
                        }
                    }

                    metadata.SerializeToFile(Path.Combine(outdir, filename));
                }
            }
        }


        public static void ShowHelp(OptionSet p = null)
        {
            Console.WriteLine("ADFSMetadataTool import|export pathtometadataxml");
            if (p != null)
            {
                Console.WriteLine();
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
            }
        }

        public static string MakeSafeFilename(string filename, char replaceChar)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, replaceChar);
            }
            return filename;
        }

        public static T[] ArrayRemoveAt<T>(T[] source, int idx)
        {
            T[] destination = new T[source.Length - 1];
            if (idx > 0)
                Array.Copy(source, 0, destination, 0, idx);

            if (idx < source.Length - 1)
                Array.Copy(source, idx + 1, destination, idx, source.Length - idx - 1);

            return destination;
        }



        public static T First<T>(IEnumerable<T> items)
        {
            using (IEnumerator<T> iter = items.GetEnumerator())
            {
                iter.MoveNext();
                return iter.Current;
            }
        }
    }
}
