using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ADFSMetadataTool
{
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata", IsNullable = false, ElementName = "EntityDescriptor")]
    public class ADFSMetadata
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new[] {
            new XmlQualifiedName("md", "urn:oasis:names:tc:SAML:2.0:metadata"),
            new XmlQualifiedName("adfs", "urm:adfs:claimrules")
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
        [XmlElement(Namespace = "urm:adfs:claimrules")]
        public string IssuanceTransformRules { get; set; }
    }

    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlType(TypeName="ApplicationServiceType")]
    public class WSFederationApplicationServiceType : EntityDescriptorRoleDescriptor { };

    [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    [XmlInclude(typeof(WSFederationApplicationServiceType))]
    public partial class EntityDescriptorRoleDescriptor
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("fed", "http://docs.oasis-open.org/wsfed/federation/200706") });

        [XmlElement(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        public TargetScopes TargetScopes { get; set; }

        [XmlElement(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        public PassiveRequestorEndpoint PassiveRequestorEndpoint { get; set; }

        [XmlAttribute()]
        public string protocolSupportEnumeration { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public class TargetScopes
    {
        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
    [XmlRoot(Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
    public partial class EndpointReference
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("wsa", "http://www.w3.org/2005/08/addressing") });

        public string Address { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public class PassiveRequestorEndpoint
    {
        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public class EntityDescriptorSPSSODescriptor
    {
        [XmlElement("SingleLogoutService")]
        public EntityDescriptorSPSSODescriptorSingleLogoutService[] SingleLogoutService { get; set; }

        [XmlElement("NameIDFormat")]
        public string[] NameIDFormat  { get; set; }

        public EntityDescriptorSPSSODescriptorAssertionConsumerService AssertionConsumerService  { get; set; }
   
        [XmlAttribute()]
        public bool AuthnRequestsSigned { get; set; }

        [XmlAttribute()]
        public bool WantAssertionsSigned { get; set; }

        [XmlAttribute()]
        public string protocolSupportEnumeration { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptorSingleLogoutService
    {
        [XmlAttribute()]
        public string Binding { get; set; }
        [XmlAttribute()]
        public string Location { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptorAssertionConsumerService
    {
        [XmlAttribute()]
        public string Binding { get; set; }
        [XmlAttribute()]
        public string Location { get; set; }
        [XmlAttribute()]
        public byte index { get; set; }
        [XmlAttribute()]
        public bool isDefault { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganization
    {
        public EntityDescriptorOrganizationOrganizationName OrganizationName { get; set; }
        public EntityDescriptorOrganizationOrganizationDisplayName OrganizationDisplayName { get; set; }
        public EntityDescriptorOrganizationOrganizationURL OrganizationURL { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganizationOrganizationName
    {
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string lang { get; set; }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganizationOrganizationDisplayName
    {
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string lang { get; set; }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganizationOrganizationURL
    {
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string lang { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorContactPerson
    {
        public string Company { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string EmailAddress { get; set; }
        public string TelephoneNumber { get; set; }

        [XmlAttribute()]
        public string contactType { get; set; }
    }
}