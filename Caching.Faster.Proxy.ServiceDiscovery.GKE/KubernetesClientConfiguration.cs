using k8s;
using k8s.Exceptions;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace Caching.Faster.Proxy.ServiceDiscovery.GKE
{
    public static class KubernetesDiscoveryClientConfiguration
    {
        private const string ServiceAccountPath = "/var/run/secrets/kubernetes.io/serviceaccount/";
        private const string ServiceAccountTokenKeyFileName = "token";
        private const string ServiceAccountRootCAKeyFileName = "ca.crt";

        public static bool IsInCluster()
        {
            var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
            var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(port))
            {
                return false;
            }

            var tokenPath = Path.Combine(ServiceAccountPath, ServiceAccountTokenKeyFileName);

            if (!File.Exists(tokenPath))
            {
                return false;
            }
            var certPath = Path.Combine(ServiceAccountPath, ServiceAccountRootCAKeyFileName);

            return File.Exists(certPath);
        }

        public static X509Certificate2Collection LoadPemFileCert(string file)
        {
            var cert = new X509CertificateParsers().ReadCertificate(File.OpenRead(file));

            var certCollection = new X509Certificate2Collection
            {
                new X509Certificate2(cert.GetEncoded())
            };

            return certCollection;
        }

        public static KubernetesClientConfiguration LocalClusterConfig()
        {
            try
            {
                var token = File.ReadAllText("token");

                var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
                var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");

                return new KubernetesClientConfiguration
                {
                    Host = new UriBuilder(host == "localhost" ? "http" : "https", host, Convert.ToInt32(port)).ToString(),
                    AccessToken = token,
                    SslCaCerts = LoadPemFileCert(@"ca.crt")
                };
            }
            catch
            {
                return null;
            }
           
        }

        public static KubernetesClientConfiguration InClusterConfig()
        {
            if (!IsInCluster())
            {
                var local = LocalClusterConfig();
                if (local is null)
                {
                    throw new KubeConfigException("unable to load in-cluster configuration token and ca.crt must exists, KUBERNETES_SERVICE_HOST and KUBERNETES_SERVICE_PORT must be defined");
                }

                return local;
                
            }

            var token = File.ReadAllText(Path.Combine(ServiceAccountPath, ServiceAccountTokenKeyFileName));
            var rootCAFile = Path.Combine(ServiceAccountPath, ServiceAccountRootCAKeyFileName);

            var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
            var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");

            return new KubernetesClientConfiguration
            {
                Host = new UriBuilder("https", host, Convert.ToInt32(port)).ToString(),
                AccessToken = token,
                SslCaCerts = CertUtils.LoadPemFileCert(rootCAFile)
            };

        }

        #region Certificate Parsers
        internal class PemParser
        {
            private readonly string _header1;
            private readonly string _header2;
            private readonly string _footer1;
            private readonly string _footer2;

            internal PemParser(
                string type)
            {
                _header1 = "-----BEGIN " + type + "-----";
                _header2 = "-----BEGIN X509 " + type + "-----";
                _footer1 = "-----END " + type + "-----";
                _footer2 = "-----END X509 " + type + "-----";
            }

            private string ReadLine(
                Stream inStream)
            {
                int c;
                var l = new StringBuilder();

                do
                {
                    while (((c = inStream.ReadByte()) != '\r') && c != '\n' && (c >= 0))
                    {
                        if (c == '\r')
                        {
                            continue;
                        }

                        l.Append((char)c);
                    }
                }
                while (c >= 0 && l.Length == 0);

                if (c < 0)
                {
                    return null;
                }

                return l.ToString();
            }

            internal Asn1Sequence ReadPemObject(
                Stream inStream)
            {
                var pemBuf = new StringBuilder();

                string line;
                while ((line = ReadLine(inStream)) != null)
                {
                    if (line.Contains(_header1) || line.Contains(_header2))
                    {
                        break;
                    }
                }

                while ((line = ReadLine(inStream)) != null)
                {
                    if (line.Contains(_footer1) || line.Contains(_footer2))
                    {
                        break;
                    }

                    pemBuf.Append(line);
                }

                if (pemBuf.Length != 0)
                {
                    var o = Asn1Object.FromByteArray(Base64.Decode(pemBuf.ToString()));

                    if (!(o is Asn1Sequence))
                    {
                        throw new IOException("malformed PEM data encountered");
                    }

                    return (Asn1Sequence)o;
                }

                return null;
            }
        }

        internal class X509CertificateParsers
        {
            private static readonly PemParser PemCertParser = new PemParser("CERTIFICATE");

            private Asn1Set sData;
            private int sDataObjectCount;
            private Stream currentStream;

            private Org.BouncyCastle.X509.X509Certificate ReadDerCertificate(
                Asn1InputStream dIn)
            {
                var seq = (Asn1Sequence)dIn.ReadObject();

                if (seq.Count > 1 && seq[0] is DerObjectIdentifier)
                {
                    if (seq[0].Equals(PkcsObjectIdentifiers.SignedData))
                    {
                        sData = SignedData.GetInstance(
                            Asn1Sequence.GetInstance((Asn1TaggedObject)seq[1], true)).Certificates;

                        return GetCertificate();
                    }
                }

                return CreateX509Certificate(X509CertificateStructure.GetInstance(seq));
            }

            private Org.BouncyCastle.X509.X509Certificate GetCertificate()
            {
                if (sData != null)
                {
                    while (sDataObjectCount < sData.Count)
                    {
                        object obj = sData[sDataObjectCount++];

                        if (obj is Asn1Sequence)
                        {
                            return CreateX509Certificate(
                                X509CertificateStructure.GetInstance(obj));
                        }
                    }
                }

                return null;
            }

            private Org.BouncyCastle.X509.X509Certificate ReadPemCertificate(
                Stream inStream)
            {
                var seq = PemCertParser.ReadPemObject(inStream);

                return seq == null
                    ? null
                    : CreateX509Certificate(X509CertificateStructure.GetInstance(seq));
            }

            protected virtual Org.BouncyCastle.X509.X509Certificate CreateX509Certificate(
                X509CertificateStructure c)
            {
                return new Org.BouncyCastle.X509.X509Certificate(c);
            }

            public Org.BouncyCastle.X509.X509Certificate ReadCertificate(
                Stream inStream)
            {
                if (inStream == null)
                    throw new ArgumentNullException("inStream");
                if (!inStream.CanRead)
                    throw new ArgumentException("inStream must be read-able", "inStream");

                if (currentStream == null)
                {
                    currentStream = inStream;
                    sData = null;
                    sDataObjectCount = 0;
                }
                else if (currentStream != inStream) // reset if input stream has changed
                {
                    currentStream = inStream;
                    sData = null;
                    sDataObjectCount = 0;
                }

                try
                {
                    if (sData != null)
                    {
                        if (sDataObjectCount != sData.Count)
                        {
                            return GetCertificate();
                        }

                        sData = null;
                        sDataObjectCount = 0;
                        return null;
                    }

                    var pis = new PushbackStream(inStream);
                    int tag = pis.ReadByte();

                    if (tag < 0)
                        return null;

                    pis.Unread(tag);

                    if (tag != 0x30)  // assume ascii PEM encoded.
                    {
                        return ReadPemCertificate(pis);
                    }

                    return ReadDerCertificate(new Asn1InputStream(pis));
                }
                catch (Exception e)
                {
                    throw new CertificateException("Failed to read certificate", e);
                }
            }
        }
        #endregion

    }
}
