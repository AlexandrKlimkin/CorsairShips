using System;
using System.Collections.Generic;
using PeNet.Asn1;

namespace BackendCommon.Code.IapValidator
{
    class IosReceipt
    {
        public string[] TransactionIds { get; private set; }
        public string ProductId { get; private set; }

        public IosReceipt(byte[] data)
        {
            var asn = Asn1Node.ReadNode(data);
            TransactionIds = FindTransactionIds(asn);
            ProductId = FindProductId(asn);
        }

        public static IosReceipt fromBase64(string base64)
        {
            var rawData = Convert.FromBase64String(base64);
            return new IosReceipt(rawData);
        }

        private Asn1Node FindSequence(Asn1Node node, ulong id)
        {
            for (var i = 0; i < node.Nodes.Count; ++i)
            {
                var n = node.Nodes[i];
                if (n.NodeType == Asn1UniversalNodeType.OctetString)
                {
                    try
                    {
                        n = Asn1Node.ReadNode(((Asn1OctetString)n).Data);
                        node.Nodes[i] = n;
                    }
                    catch (Exception)
                    { }
                }
                if (n is Asn1Integer asnInt && i == 0)
                {
                    if (asnInt.ToUInt64() == id)
                    {
                        return node;
                    }
                }
                else if (n.Nodes.Count > 0)
                {
                    var r = FindSequence(n, id);
                    if (r != null)
                        return r;
                }
            }
            return null;
        }

        private List<Asn1Node> FindSequences(Asn1Node node, ulong id)
        {
            List<Asn1Node> result = new List<Asn1Node>();
            for (var i = 0; i < node.Nodes.Count; ++i)
            {
                var n = node.Nodes[i];
                if (n.NodeType == Asn1UniversalNodeType.OctetString)
                {
                    try
                    {
                        n = Asn1Node.ReadNode(((Asn1OctetString)n).Data);
                        node.Nodes[i] = n;
                    }
                    catch (Exception)
                    { }
                }
                if (n is Asn1Integer asnInt)
                {
                    if (asnInt.ToUInt64() == id)
                    {
                        result.Add(node);
                    }
                }
                else if (n.Nodes.Count > 0)
                {
                    var r = FindSequences(n, id);
                    
                    if (r.Count > 0)
                        result.AddRange(r);
                }
            }
            return result;
        }

        private string FindString(Asn1Node node, ulong id)
        {
            var seq = FindSequence(node, id);
            for (var i = 0; i < seq.Nodes.Count; ++i)
            {
                var n = seq.Nodes[i];
                if (n.NodeType == Asn1UniversalNodeType.OctetString)
                {
                    try
                    {
                        n = Asn1Node.ReadNode(((Asn1OctetString)n).Data);
                        seq.Nodes[i] = n;
                    }
                    catch (Exception)
                    { }
                }
                if (n.NodeType == Asn1UniversalNodeType.Utf8String)
                {
                    return ((Asn1Utf8String)n).Value;
                }
            }
            return null;
        }

        private string[] FindTransactionIds(Asn1Node node)
        {
            var nodes = FindSequences(node, 1703);
            var result = new string[nodes.Count];
            for (var i = 0; i < nodes.Count; ++i)
            {
                result[i] = FindString(nodes[i], 1703);
            }
            return result;
        }

        private string FindProductId(Asn1Node node)
        {
            return FindString(node, 2);
        }
    }
}
