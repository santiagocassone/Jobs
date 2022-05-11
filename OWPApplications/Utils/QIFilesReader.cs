using OWPApplications.Models.QIAuditTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OWPApplications.Utils
{
    public class QIFilesReader
    {

        public static TransactionType ReadQISalesXMLFile(string Xml)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TransactionType));

                using (var reader = new StringReader(Xml))
                {
                    return (TransactionType)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing QI Sales XML - {e.Message}", e);
            }
        }


        public static IEnumerable<QIOrderFile> ReadQIOrderXMLFile(string XmlContents)
        {
            var results = new List<QIOrderFile>();
            try
            {
                // Parse the string recevied and get the /PurchaseOrder/OrderLineItem nodes 
                var xml = XDocument.Parse(XmlContents);
                var orderLines = xml.Element("Envelope").Element("PurchaseOrder").Elements("OrderLineItem");

                // Iterate over the nodes and extract their information
                foreach (var orderLine in orderLines)
                {
                    var prices = orderLine.Element("Price");
                    var spec = orderLine.Element("SpecItem");
                    var catalog = spec.Element("Catalog");
                    var q = new QIOrderFile
                    {
                        Quantity = Convert.ToInt32(orderLine.Element("Quantity").Value),
                        LineItemNumber = Convert.ToInt32(orderLine.Element("LineItemNumber").Value),
                        LineItemIdentifier = orderLine.Element("LineItemIdentifier")?.Value,
                        PublishedPrice = Convert.ToDecimal(prices.Element("PublishedPrice").Value),
                        PublishedPriceExt = Convert.ToDecimal(prices.Element("PublishedPriceExt").Value),
                        EndCustomerPrice = Convert.ToDecimal(prices.Element("EndCustomerPrice").Value),
                        EndCustomerPriceExt = Convert.ToDecimal(prices.Element("EndCustomerPriceExt").Value),
                        OrderDealerPrice = Convert.ToDecimal(prices.Element("OrderDealerPrice").Value),
                        OrderDealerPriceExt = Convert.ToDecimal(prices.Element("OrderDealerPriceExt").Value),
                        SpecItemNumber = spec.Element("Number").Value,
                        SpecItemDescription = spec.Element("Description").Value,
                        SpecItemCatalogCode = catalog.Element("Code").Value
                    };

                    results.Add(q);
                }

                return results;
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing QI Order XML - {e.Message}", e);
            }
        }
    }
}
