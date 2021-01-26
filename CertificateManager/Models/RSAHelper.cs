using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CertificateManager.Models
{
    

    class RSAHelper
    {
        static public string[] CreateCACert(Cert cert)
        {
            var RSAKey = RSA.Create((int)cert.KeySize);

            //X509Certificate2 c = new X509Certificate2("C:\\Users\\Nikita\\Desktop\\cert_export_KKAB_CA.crt");
            //string tmp = Convert.ToBase64String(c.Extensions[3].RawData);
            //string tmp2 = Encoding.UTF8.GetString(c.Extensions[3].RawData);

            var certReq = new CertificateRequest(new X500DistinguishedName(_DistingushedNameRawData(cert)), RSAKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            certReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false));
            //certReq.CertificateExtensions.Add(_GetCAKeyExtension(cert, new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false), false));
            certReq.CertificateExtensions.Add(_GetCRLExtension(false));
            var exp = cert.DateStop;

            var cacert = certReq.CreateSelfSigned(DateTime.Now, exp);

            string certBase64 = Convert.ToBase64String(cacert.RawData, Base64FormattingOptions.InsertLineBreaks);
            var keyCert = cacert.GetRSAPrivateKey();
            string keyBase64 = Convert.ToBase64String(keyCert.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
            
            return new string[] { certBase64, keyBase64};
        }

        static public string[] CreateServerCert(Cert cert, Cert ca)
        {
            var RSAKey = RSA.Create((int)cert.KeySize);

            var certReq = new CertificateRequest(new X500DistinguishedName(_DistingushedNameRawData(cert)), RSAKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false));
            certReq.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));
            certReq.CertificateExtensions.Add(_GetCAKeyExtension(new X509SubjectKeyIdentifierExtension(ca.certificate.PublicKey, false), false));
            byte[] serialNumber = BitConverter.GetBytes(DateTime.Now.ToBinary());
            var exp = cert.DateStop;

            var newcert = certReq.Create(ca.certificate, DateTimeOffset.Now, exp, serialNumber);

            string certBase64 = Convert.ToBase64String(newcert.RawData, Base64FormattingOptions.InsertLineBreaks);
            string keyBase64 = Convert.ToBase64String(RSAKey.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);

            return new string[] { certBase64, keyBase64 };
        }

        static public string[] CreateUserCert(Cert cert, Cert ca)
        {
            var RSAKey = RSA.Create((int)cert.KeySize);

            var certReq = new CertificateRequest(new X500DistinguishedName(_DistingushedNameRawData(cert)), RSAKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false));
            certReq.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.2") }, false));
            certReq.CertificateExtensions.Add(_GetCAKeyExtension(new X509SubjectKeyIdentifierExtension(ca.certificate.PublicKey, false), false));
            byte[] serialNumber = BitConverter.GetBytes(DateTime.Now.ToBinary());
            var exp = cert.DateStop;

            var newcert = certReq.Create(ca.certificate, DateTimeOffset.Now, exp, serialNumber);

            string certBase64 = Convert.ToBase64String(newcert.RawData, Base64FormattingOptions.InsertLineBreaks);
            string keyBase64 = Convert.ToBase64String(RSAKey.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
            
            return new string[] { certBase64, keyBase64 };
        }

        static public void FillInfo(ref Cert cert, string[] cert64)
        {
            var Xcert = new X509Certificate2(Convert.FromBase64String(cert64[0]));
            RSA Xkey = RSA.Create();
            int check;
            Xkey.ImportRSAPrivateKey(Convert.FromBase64String(cert64[1]), out check);
            Xcert = Xcert.CopyWithPrivateKey(Xkey);
            cert.certificate = Xcert;

            var subject = cert.certificate.SubjectName.Decode(X500DistinguishedNameFlags.UseNewLines);
            subject = subject.Replace("\r", "");
            var subjects = subject.Split('\n');
            cert.CommonName = subjects.First((s) => s.Contains("CN=")).Remove(0, 3);
            cert.OrganisationUnit = subjects.First((s) => s.Contains("OU=")).Remove(0, 3);
            cert.Organisation = subjects.First((s) => s.Contains("O=")).Remove(0, 2);
            cert.Local = subjects.First((s) => s.Contains("L=")).Remove(0, 2);
            cert.State = subjects.First((s) => s.Contains("S=")).Remove(0, 2);
            cert.Country = subjects.First((s) => s.Contains("C=")).Remove(0, 2);

            cert.KeySize = cert.certificate.PrivateKey.KeySize;
            cert.DateStart = cert.certificate.NotBefore.Date;
            cert.DateStop = cert.certificate.NotAfter.Date;
        }

        static public string[] GetCert64(Cert cert)
        {
            string[] res = new string[3];

            res[0] = Convert.ToBase64String(cert.certificate.RawData);
            res[1] = Convert.ToBase64String(cert.certificate.GetRSAPrivateKey().ExportRSAPrivateKey());
            res[2] = cert.certificate.GetRSAPrivateKey().SignatureAlgorithm;

            return res;
        }


        private static X509Extension _GetCRLExtension(bool critical)
        {
            var CRLbytes = Encoding.UTF8.GetBytes("http://127.0.0.1/crl/7.crl");
            var bytes = new byte[10];

            bytes[0] = 0x30;
            bytes[1] = Convert.ToByte(bytes.Length + CRLbytes.Length - 2);
            bytes[2] = 0x30;
            bytes[3] = Convert.ToByte(bytes.Length + CRLbytes.Length - 4);
            bytes[4] = 0xA0;
            bytes[5] = Convert.ToByte(bytes.Length + CRLbytes.Length - 6);
            bytes[6] = 0xA0;
            bytes[7] = Convert.ToByte(bytes.Length + CRLbytes.Length - 8);
            bytes[8] = 0x86;
            bytes[9] = Convert.ToByte(bytes.Length + CRLbytes.Length - 10);
            return new X509Extension(new AsnEncodedData("2.5.29.31", bytes.Concat<byte>(CRLbytes).ToArray()), critical);
        }

        private static X509Extension _GetCAKeyExtension(Cert CA, X509SubjectKeyIdentifierExtension caSubjectKey, bool critical)
        {
            byte[] fingerprintSector = caSubjectKey.RawData; //Extensions["2.5.29.14"]
            fingerprintSector[0] = 0x80;

            var allSets = _DistingushedNameRawData(CA);
            byte[] mainSector = (new byte[] { 0xA1, Convert.ToByte(allSets.Length + 4), 0xA4, Convert.ToByte(allSets.Length + 2) }).Concat(allSets).ToArray();

            byte[] sectors = fingerprintSector.Concat(mainSector).ToArray();

            byte[] allBytes = (new byte[] { 0x30, Convert.ToByte(sectors.Length) }).Concat(sectors).ToArray();
            string tmp = Convert.ToBase64String(allBytes);

            return new X509Extension(new AsnEncodedData("2.5.29.35", allBytes), critical);
        }

        private static byte[] _DistingushedNameRawData(Cert cert)
        {
            byte[] commonName = Encoding.UTF8.GetBytes(cert.CommonName);
            byte[] organisation = Encoding.UTF8.GetBytes(cert.Organisation);
            byte[] organisationUnit = Encoding.UTF8.GetBytes(cert.OrganisationUnit);
            byte[] country = Encoding.UTF8.GetBytes(cert.Country);
            byte[] local = Encoding.UTF8.GetBytes(cert.Local);
            byte[] state = Encoding.UTF8.GetBytes(cert.State);

            byte[] setCommonName = (new byte[] { 0x31, Convert.ToByte(commonName.Length + 9), 0x30, Convert.ToByte(commonName.Length + 7), 0x06, 0x03, 0x55, 0x04, 0x03, 0x0C, Convert.ToByte(commonName.Length) }).Concat(commonName).ToArray();
            byte[] setOrganisation = (new byte[] { 0x31, Convert.ToByte(organisation.Length + 9), 0x30, Convert.ToByte(organisation.Length + 7), 0x06, 0x03, 0x55, 0x04, 0x0A, 0x0C, Convert.ToByte(organisation.Length) }).Concat(organisation).ToArray();
            byte[] setOrganisationUnit = (new byte[] { 0x31, Convert.ToByte(organisationUnit.Length + 9), 0x30, Convert.ToByte(organisationUnit.Length + 7), 0x06, 0x03, 0x55, 0x04, 0x0B, 0x0C, Convert.ToByte(organisationUnit.Length) }).Concat(organisationUnit).ToArray();
            byte[] setCountry = (new byte[] { 0x31, Convert.ToByte(country.Length + 9), 0x30, Convert.ToByte(country.Length + 7), 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, Convert.ToByte(country.Length) }).Concat(country).ToArray();
            byte[] setLocal = (new byte[] { 0x31, Convert.ToByte(local.Length + 9), 0x30, Convert.ToByte(local.Length + 7), 0x06, 0x03, 0x55, 0x04, 0x07, 0x0C, Convert.ToByte(local.Length) }).Concat(local).ToArray();
            byte[] setState = (new byte[] { 0x31, Convert.ToByte(state.Length + 9), 0x30, Convert.ToByte(state.Length + 7), 0x06, 0x03, 0x55, 0x04, 0x08, 0x0C, Convert.ToByte(state.Length) }).Concat(state).ToArray();

            byte[] allSets = setCommonName.Concat(setOrganisation).Concat(setOrganisationUnit).Concat(setCountry).Concat(setLocal).Concat(setState).ToArray();

            return (new byte[] { 0x30, Convert.ToByte(allSets.Length) }).Concat(allSets).ToArray();
        }

        private static X509Extension _GetCAKeyExtension(X509SubjectKeyIdentifierExtension caSubjectKey, bool critical)
        {
            byte[] fingerprintSector = caSubjectKey.RawData; //Extensions["2.5.29.14"]
            fingerprintSector[0] = 0x80;

            byte[] allBytes = (new byte[] { 0x30, Convert.ToByte(fingerprintSector.Length) }).Concat(fingerprintSector).ToArray();

            string tmp = Convert.ToBase64String(allBytes);

            return new X509Extension(new AsnEncodedData("2.5.29.35", allBytes), critical);
        }
    }
}
