using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertificateManager.Models
{
    

    class RSAHelper
    {
        static public string[] CreateCACert(Cert cert)
        {
            var RSAKey = RSA.Create((int)cert.KeySize);
            string param = $"CN={cert.CommonName}/O={cert.Organisation}/OU={cert.OrganisationUnit}/C={cert.Country}";

            var certReq = new CertificateRequest(param, RSAKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, true));
            var exp = DateTime.Now.AddYears(10);

            var cacert = certReq.CreateSelfSigned(DateTime.Now, exp);

            string certBase64 = Convert.ToBase64String(cacert.RawData, Base64FormattingOptions.InsertLineBreaks);
            var keyCert = cacert.GetRSAPrivateKey();
            string keyBase64 = Convert.ToBase64String(keyCert.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
            string keySignature = keyCert.SignatureAlgorithm;
            
            return new string[] { certBase64, keyBase64, keySignature};
        }

        static public string[] CreateChildCert(Cert cert, string[] cacert64)
        {
            var RSAKey = RSA.Create((int)cert.KeySize);
            string param = $"CN={cert.CommonName}/O={cert.Organisation}/OU={cert.OrganisationUnit}/C={cert.Country}";

            var certReq = new CertificateRequest(param, RSAKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            certReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, false));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false));
            byte[] serialNumber = BitConverter.GetBytes(DateTime.Now.ToBinary());
            var exp = DateTime.Now.AddYears(5);

            var ca = new X509Certificate2(Convert.FromBase64String(cacert64[0]));
            RSA cakey = RSA.Create();
            int check;
            cakey.ImportRSAPrivateKey(Convert.FromBase64String(cacert64[1]), out check);
            ca = ca.CopyWithPrivateKey(cakey);

            var newcert = certReq.Create(ca, DateTimeOffset.Now, exp, serialNumber);

            string certBase64 = Convert.ToBase64String(newcert.RawData, Base64FormattingOptions.InsertLineBreaks);
            string keyBase64 = Convert.ToBase64String(RSAKey.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
            string keySignature = RSAKey.SignatureAlgorithm;

            return new string[] { certBase64, keyBase64, keySignature };
        }
    }
}
