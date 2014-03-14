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


namespace ADFSMetadataTool
{
    class Program
    {
        static int Main(string[] args)
        {
            string exepath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            string samplespath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "../../../samples");

            //ExportADFSMetadata(exepath);
            ImportADFSMetadata(Path.Combine(exepath, "metadata-test.xml"));
            /*
            var metadata1 = ADFSMetadata.DeserializeFromFile(Path.Combine(samplespath, "metadata-test.xml"));
            metadata1.SerializeToFile(Path.Combine(samplespath, "metadata-test2.xml"));
            return 0;*/

            return 0;
        }

        public static void ImportADFSMetadata(string filename)
        {
            var metadata = ADFSMetadata.DeserializeFromFile(filename);

            AddRelyingPartyTrustCommand addRelyingPartyTrustCommand = new AddRelyingPartyTrustCommand();
            addRelyingPartyTrustCommand.MetadataFile = filename;
            addRelyingPartyTrustCommand.Name = metadata.entityID;

            if (metadata.Extensions != null) {
                if (!String.IsNullOrEmpty(metadata.Extensions.displayName))
                {
                    addRelyingPartyTrustCommand.Name = metadata.Extensions.displayName;
                }
                if (!String.IsNullOrEmpty(metadata.Extensions.issuanceTransformRules))
                {
                    addRelyingPartyTrustCommand.IssuanceTransformRules = metadata.Extensions.issuanceTransformRules;
                }
                if (!String.IsNullOrEmpty(metadata.Extensions.signatureAlgorithm))
                {
                    addRelyingPartyTrustCommand.SignatureAlgorithm = metadata.Extensions.signatureAlgorithm;
                }
            }


            /* 
            addRelyingPartyTrustCommand.Identifier = new string[] { metadata.entityID };
            if (metadata.RoleDescriptor != null) {
                addRelyingPartyTrustCommand.WSFedEndpoint = new Uri(metadata.RoleDescriptor.PassiveRequestorEndpoint.EndpointReference.Address);
            }

            if (metadata.SPSSODescriptor != null)
            {
                addRelyingPartyTrustCommand.SignedSamlRequestsRequired = metadata.SPSSODescriptor.AuthnRequestsSigned;

                var samlEndpoints = new List<SamlEndpoint>();
                foreach (var endpoint in metadata.SPSSODescriptor.AssertionConsumerService)
                {
                    NewSamlEndpointCommand newSamlEndpointCommand = new NewSamlEndpointCommand();
                    if (endpoint.Binding.EndsWith("POST"))
                    {
                        newSamlEndpointCommand.Binding = "POST";
                    }
                    else if (endpoint.Binding.EndsWith("Redirect"))
                    {
                        newSamlEndpointCommand.Binding = "Redirect";
                    }
                    else
                    {
                        throw new Exception("Unknown binding type in Endpoint" + endpoint.Binding);
                    }

                    newSamlEndpointCommand.Uri = new Uri(endpoint.Location);
                    newSamlEndpointCommand.IsDefault = endpoint.isDefault;
                    newSamlEndpointCommand.Index = endpoint.index;
                    newSamlEndpointCommand.Protocol = "SAMLAssertionConsumer";
                    IEnumerable commandResults = newSamlEndpointCommand.Invoke();
                    foreach (SamlEndpoint commandResult in commandResults)
                    {
                        samlEndpoints.Add(commandResult);
                    }
                }

                addRelyingPartyTrustCommand.SamlEndpoint = samlEndpoints.ToArray();
            } */

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
                    if(!rp.Enabled) { continue; }

                    var filename = MakeSafeFilename("metadata-" + id.Replace("https://", "").Replace("http://", "").TrimEnd(new[] { '/' }), '-') + ".xml";

                    Console.WriteLine("Exporting " + rp.Name + " to " + filename);
                    var metadata = new ADFSMetadata("abcd1", id);
                    metadata.Extensions = new EntityDescriptorExtensions(rp.Name, rp.IssuanceTransformRules.Trim());
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
                            if (rp.SamlEndpoints[i].Protocol == "SAMLAssertionConsumer") {
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

        public static string MakeSafeFilename(string filename, char replaceChar)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, replaceChar);
            }
            return filename;
        }

        static T First<T>(IEnumerable<T> items)
        {
            using (IEnumerator<T> iter = items.GetEnumerator())
            {
                iter.MoveNext();
                return iter.Current;
            }
        }
    }
}
