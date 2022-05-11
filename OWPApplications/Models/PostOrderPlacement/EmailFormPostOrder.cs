using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.PostOrderPlacement
{
    public class EmailFormPostOrder : EmailForm
    {
        public bool IncludeTitle { get; set; }
        public string OrderTitle { get; set; }
        public string SalesPersonID { get; set; }
        public string CustomerName { get; set; }
        public string FromName { get; set; }
        public string Notes { get; set; }
        public string ProjectID { get; set; }

        public IEnumerable<EmailDataPostOrder> DataToSend { get; set; }
        public string Company { get; set; }
              
    }

    public class EmailDataPostOrder
    {
        public string OrderSuffixDate { get; set; }
        public string PoSuffix { get; set; }        
        public string ACK { get; set; }
        public string VendorNo { get; set; }
        public string To { get; set; }
        public string EmailType { get; set; }
        public string InputSubject { get; set; }
        public string OrderNo { get; set; }

        public string Subject(EmailFormPostOrder form)
        {
            string subject;
            if (this.EmailType == "ack")
            {
                subject = string.Format("Order Acknowledgment Needed: {0}{1}-{2}-{3} | {4} | {5}", form.Company, this.OrderNo, this.PoSuffix, form.SalesPersonID, this.OrderSuffixDate, form.CustomerName);
            }
            else if (this.EmailType == "track")
            {
                subject = string.Format("Tracking Information Needed: {0}{1}-{2}-{3} | {4} | {5}", form.Company, this.OrderNo, this.PoSuffix, form.SalesPersonID, this.OrderSuffixDate, form.CustomerName);
            }
            else //if (this.EmailType == EmailPostOrderType.Freeform)
            {
                subject = string.Format("{0}: {1}{2}-{3}-{4} | {5} | {6} | Ack# {7}", this.InputSubject, form.Company, this.OrderNo, this.PoSuffix, form.SalesPersonID, this.OrderSuffixDate, form.CustomerName, this.ACK);
            }

            if (form.IncludeTitle)
            {
                subject += string.Format(" | {0} | PID# {1}", form.OrderTitle, form.ProjectID);
            } else
            {
                subject += string.Format(" | PID# {0}", form.ProjectID);
            }
            return subject;
        }

        public string Body(EmailFormPostOrder form)
        {
            //Replace PO #
            string template = @"<html>
            <style>
            body {font:14px arial, sans-serif;color:#666;}
            table {border-collapse:collapse;}
            table, td, th {border: 1px solid #ccc; padding:10px;}
            th {font-size:14px; background-color:#ff4d4d; border:1px solid #ff4d4d; color:#fff;}
            p {
             margin: 0;
             padding: 0;
            }
            </style>
            <body>
            <br/>            
            <p>Dear Valued Partner:</p>
            <br/>      
            <p>##BODY##</p>

            <br/><p>Notes: </p>
            <p>##NOTES##</p>
            <br/>
            <p><strong>***Please be sure to reply all to e-mail thread to ensure that all necessary parties receive your response***</strong></p>
            <br/>
            <p>Please let us know if you have any other questions.</p>
            <p>Thank you!</p>
            <p>##YOURNAME##</p>
            </body>
            </html>";
            string ack_body = @"";
            string track_body = @"";
            if (form.Company == "S")
            {
                ack_body = @"            
            Our system has indicated that an acknowledgment has not been
            received for Open Square PO# ##PO_NO##. In order to maintain project 
            statusing required to ensure a seamless delivery and installation 
            experience for our clients, please provide an acknowledgement and ship 
            date/arrival information within 24 hours.
            ";

                track_body = @"            
            Our system has indicated that this acknowledgement ##ACK_NO## has shipped
            and has not been received for Open Square PO# ##PO_NO##. In order to 
            maintain project statusing required to ensure a seamless delivery 
            and installation experience for our clients, please provide the 
            following within 24 hours:
            <ul>
                <li>Tracking Information (Carrier and Tracking/PRL #)</li>
                <li>Expected Transit Time</li>
            </ul>

            ";
            } 
            else
            {
                ack_body = @"            
            Our system has indicated that an acknowledgment has not been
            received for One Workplace PO# ##PO_NO##. In order to maintain project 
            statusing required to ensure a seamless delivery and installation 
            experience for our clients, please provide an acknowledgement and ship 
            date/arrival information within 24 hours.
            ";

                track_body = @"            
            Our system has indicated that this acknowledgement ##ACK_NO## has shipped
            and has not been received for One Workplace PO# ##PO_NO##. In order to 
            maintain project statusing required to ensure a seamless delivery 
            and installation experience for our clients, please provide the 
            following within 24 hours:
            <ul>
                <li>Tracking Information (Carrier and Tracking/PRL #)</li>
                <li>Expected Transit Time</li>
            </ul>

            ";
            }
            
            string output = template;
            if (this.EmailType == "ack")
            {
                output = output.Replace("##BODY##", ack_body);

            }
            else if (this.EmailType == "track")
            {
                output = output.Replace("##BODY##", track_body);
            }
            else
            {
                output = output.Replace("##BODY##", "Ack # " + this.ACK);
                output = output.Replace(@"<br/><p>Notes: </p>", string.Empty);
            }

            output = output.Replace("##ACK_NO##", this.ACK);
            output = output.Replace("##NOTES##", form.Notes);
            output = output.Replace("##PO_NO##", string.Format("{0}{1}-{2}-{3}", form.Company, this.OrderNo, this.PoSuffix, form.SalesPersonID));

            output = output.Replace("##YOURNAME##", form.FromName);
            return output;
        }
    }
}
