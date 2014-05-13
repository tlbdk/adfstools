using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ADFSMetadataTool
{
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata", IsNullable = false, ElementName = "EntityDescriptor")]
    public class ADFSMetadata
    {
        public ADFSMetadata() { }

        public ADFSMetadata(string id, string entityid)
        {
            this.ID = id;
            this.entityID = entityid;
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new[] {
            new XmlQualifiedName("md", "urn:oasis:names:tc:SAML:2.0:metadata"),
            new XmlQualifiedName("adfs", "urm:adfs")
        });

        [XmlAttribute()]    
        public string ID { get; set; }

        [XmlAttribute()]
        public string entityID { get; set; }

        public EntityDescriptorExtensions Extensions { get; set; }
        public EntityDescriptorRoleDescriptor RoleDescriptor { get; set; }
        public EntityDescriptorSPSSODescriptor SPSSODescriptor { get; set; }
        public EntityDescriptorOrganization Organization { get; set; }
        public EntityDescriptorContactPerson ContactPerson { get; set; }

        public static ADFSMetadata DeserializeFromFile(string filename)
        {
            XmlSerializer s = new XmlSerializer(typeof(ADFSMetadata));
            using (System.IO.TextReader r = new System.IO.StreamReader(filename))
            {
                ADFSMetadata metadata = (ADFSMetadata)s.Deserialize(r);
                return metadata;
            }
        }

        public void SerializeToFile(string filename)
        {
            XmlSerializer s = new XmlSerializer(typeof(ADFSMetadata));
            using (System.IO.TextWriter w = new System.IO.StreamWriter(filename, false, Encoding.UTF8))
            {
                s.Serialize(w, this, this.xmlns);
            }
        }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorExtensions
    {
        public EntityDescriptorExtensions()
        {
        }

        public EntityDescriptorExtensions(string displayname, string issuancetransformrules, string issuanceAuthorizationRules)
        {
            this.displayName = displayname;
            this.issuanceTransformRules = issuancetransformrules;
            this.issuanceAuthorizationRules = issuanceAuthorizationRules;
        }

        [XmlElement(Namespace = "urm:adfs")]
        public string displayName { get; set; }
        [XmlElement(Namespace = "urm:adfs")]
        public string issuanceTransformRules { get; set; }
        [XmlElement(Namespace = "urm:adfs")]
        public string issuanceAuthorizationRules { get; set; }
        [XmlElement(Namespace = "urm:adfs")]
        public string signatureAlgorithm { get; set; }
    }

    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlType(TypeName="ApplicationServiceType")]
    public class WSFederationApplicationServiceTypeRoleDescriptor : EntityDescriptorRoleDescriptor { };

    [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    [XmlInclude(typeof(WSFederationApplicationServiceTypeRoleDescriptor))]
    public partial class EntityDescriptorRoleDescriptor
    {
        public EntityDescriptorRoleDescriptor()
        {
            this.protocolSupportEnumeration = "http://docs.oasis-open.org/wsfed/federation/200706";
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new[] { 
            new XmlQualifiedName("fed", "http://docs.oasis-open.org/wsfed/federation/200706"),
            new XmlQualifiedName("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        });

        [XmlElement(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        public TargetScope[] TargetScopes { get; set; }

        [XmlElement(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        public PassiveRequestorEndpoint PassiveRequestorEndpoint { get; set; }

        [XmlAttribute()]
        public string protocolSupportEnumeration { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public class TargetScope
    {
        public TargetScope() {}

        public TargetScope(string EndpointReferenceAddress)
        {
            this.EndpointReference = new EndpointReference(EndpointReferenceAddress);
        }

        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
    [XmlRoot(Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
    public partial class EndpointReference
    {
        public EndpointReference() { }
        public EndpointReference(string address)
        {
            Address = address;
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("wsa", "http://www.w3.org/2005/08/addressing") });

        public string Address { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public class PassiveRequestorEndpoint
    {
        public PassiveRequestorEndpoint() { }
        public PassiveRequestorEndpoint(string address)
        {
            this.EndpointReference = new EndpointReference(address);
        }

        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public class EntityDescriptorSPSSODescriptor
    {
        public EntityDescriptorSPSSODescriptor()
        {
            this.protocolSupportEnumeration = "urn:oasis:names:tc:SAML:2.0:protocol";
        }

        [XmlElement("SingleLogoutService")]
        public EntityDescriptorSPSSODescriptorSingleLogoutService[] SingleLogoutService { get; set; }

        [XmlElement("NameIDFormat")]
        public string[] NameIDFormat  { get; set; }

        [XmlElement("AssertionConsumerService")]
        public EntityDescriptorSPSSODescriptorAssertionConsumerService[] AssertionConsumerService  { get; set; }
   
        [XmlAttribute()]
        public bool AuthnRequestsSigned { get; set; }

        [XmlAttribute()]
        public bool WantAssertionsSigned { get; set; }

        [XmlAttribute()]
        public string protocolSupportEnumeration { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public class EntityDescriptorSPSSODescriptorSingleLogoutService
    {
        [XmlAttribute()]
        public string Binding { get; set; }
        [XmlAttribute()]
        public string Location { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public class EntityDescriptorSPSSODescriptorAssertionConsumerService
    {
        [XmlAttribute()]
        public string Binding { get; set; }
        [XmlAttribute()]
        public string Location { get; set; }
        [XmlAttribute()]
        public int index { get; set; }
        [XmlAttribute()]
        public bool isDefault { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public class EntityDescriptorOrganization
    {
        public EntityDescriptorOrganization() { }

        public EntityDescriptorOrganization(string organizationname, string organizationdisplayname, string organizationurl) {
            this.OrganizationName = new EntityDescriptorOrganizationOrganizationName(organizationname);
            this.OrganizationDisplayName = new EntityDescriptorOrganizationOrganizationDisplayName(organizationdisplayname);
            this.OrganizationURL = new EntityDescriptorOrganizationOrganizationURL(organizationurl);
        }

        public EntityDescriptorOrganizationOrganizationName OrganizationName { get; set; }
        public EntityDescriptorOrganizationOrganizationDisplayName OrganizationDisplayName { get; set; }
        public EntityDescriptorOrganizationOrganizationURL OrganizationURL { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganizationOrganizationName
    {
        public EntityDescriptorOrganizationOrganizationName() { }
        public EntityDescriptorOrganizationOrganizationName(string name) 
        {
            this.lang = "en";
            this.Value = name;
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string lang { get; set; }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganizationOrganizationDisplayName
    {
        public EntityDescriptorOrganizationOrganizationDisplayName() { }
        public EntityDescriptorOrganizationOrganizationDisplayName(string name) 
        {
            this.lang = "en";
            this.Value = name;
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string lang { get; set; }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganizationOrganizationURL
    {
        public EntityDescriptorOrganizationOrganizationURL() { }
        public EntityDescriptorOrganizationOrganizationURL(string name) 
        {
            this.lang = "en";
            this.Value = name;
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string lang { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorContactPerson
    {
        public EntityDescriptorContactPerson() { }
 
        public string Company { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string EmailAddress { get; set; }
        public string TelephoneNumber { get; set; }

        [XmlAttribute()]
        public string contactType { get; set; }
    }
}