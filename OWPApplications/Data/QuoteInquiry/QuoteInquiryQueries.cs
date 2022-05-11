using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data
{
    public static class QuoteInquiryQueries
    {
        public static string HEADER = @"SELECT 
C.Name as CustomerName,
OP_ORDER_HEADER.Order_Index,
OP_ORDER_HEADER.Project_Id,
OP_ORDER_HEADER.Title,
OP_ORDER_HEADER.Location_Code,
RTRIM(LTRIM(OP_ORDER_HEADER.Customer_No)) Customer_No,
OP_ORDER_HEADER.Salesperson_Id_1,
OP_ORDER_HEADER.Salesperson_1_Pct,
OP_ORDER_HEADER.Salesperson_Id_2,
OP_ORDER_HEADER.Customer_Terms_Code,
OP_ORDER_HEADER.Customer_PO_Number,
OP_ORDER_HEADER.STI_Invoice_Type,
OP_ORDER_HEADER.Order_Status,
OP_ORDER_HEADER.Invoicing_Indicator,
OP_ORDER_HEADER.Invoice_Address,
V_OP_ORDER_ADDRESS.Address_Line_1, 
RTRIM(LTRIM(V_OP_ORDER_ADDRESS.Address_Line_2)) Address_Line_2,
V_OP_ORDER_ADDRESS.City, 
V_OP_ORDER_ADDRESS.Region, 
V_OP_ORDER_ADDRESS.Postal_Code, AR_TERMSCDS.Description
FROM OP_ORDER_HEADER INNER JOIN V_OP_ORDER_ADDRESS ON OP_ORDER_HEADER.Company_Code = V_OP_ORDER_ADDRESS.Company_Code AND OP_ORDER_HEADER.Order_Index = V_OP_ORDER_ADDRESS.Order_Index 
LEFT OUTER JOIN AR_TERMSCDS ON OP_ORDER_HEADER.Customer_Terms_Code = AR_TERMSCDS.Customer_Terms_Code
LEFT JOIN V_AR_CUSTOMERS C ON C.Company_Code = 'W' and C.Customer_No = OP_ORDER_HEADER.Customer_No
WHERE (OP_ORDER_HEADER.Order_No = '{0}') AND (OP_ORDER_HEADER.Company_Code = 'W') AND (OP_ORDER_HEADER.Order_Type = 'Q') AND (V_OP_ORDER_ADDRESS.Record_Type = 'P')";

        public static string MAIL_TO = @"SELECT V_OP_ORDER_ADDRESS.Address_Line_1, RTRIM(LTRIM(V_OP_ORDER_ADDRESS.Address_Line_2))  Address_Line_2, V_OP_ORDER_ADDRESS.City, V_OP_ORDER_ADDRESS.Region, V_OP_ORDER_ADDRESS.Postal_Code
FROM OP_ORDER_HEADER INNER JOIN V_OP_ORDER_ADDRESS ON OP_ORDER_HEADER.Company_Code = V_OP_ORDER_ADDRESS.Company_Code AND OP_ORDER_HEADER.Order_Index = V_OP_ORDER_ADDRESS.Order_Index
WHERE (OP_ORDER_HEADER.Order_No = '{0}') AND (OP_ORDER_HEADER.Company_Code = 'W') AND (OP_ORDER_HEADER.Order_Type = 'Q') AND (V_OP_ORDER_ADDRESS.Record_Type = 'D')";

        public static string SOLD_TO = @"SELECT V_OP_ORDER_ADDRESS.Address_Line_1, RTRIM(LTRIM(V_OP_ORDER_ADDRESS.Address_Line_2)) Address_Line_2, V_OP_ORDER_ADDRESS.City, V_OP_ORDER_ADDRESS.Region, V_OP_ORDER_ADDRESS.Postal_Code 
FROM OP_ORDER_HEADER INNER JOIN V_OP_ORDER_ADDRESS ON OP_ORDER_HEADER.Company_Code = V_OP_ORDER_ADDRESS.Company_Code AND OP_ORDER_HEADER.Order_Index = V_OP_ORDER_ADDRESS.Order_Index 
WHERE (OP_ORDER_HEADER.Order_No = '{0}') AND (OP_ORDER_HEADER.Company_Code = 'W') AND (OP_ORDER_HEADER.Order_Type = 'Q') AND (V_OP_ORDER_ADDRESS.Record_Type = 'M')";

        public static string LINES = @"dbo.[Get_QuoteInquiry_Lines] {0}, '{1}'";

        public static string MFG_INFO = @"
SELECT DISTINCT V_OP_STCENTIN.Vnd_No, V_OP_CONTACTS.First_Name AS Dlr_First_Name, V_OP_CONTACTS.Last_Name AS Dlr_Last_Name,
OP_CONTPERS_1.First_Name AS STC_First_Name, OP_CONTPERS_1.Last_Name AS STC_Last_Name, OP_CONTPERS_2.First_Name AS Carr_First_Name,
OP_CONTPERS_2.Last_Name AS Carr_Last_Name, V_OP_STCENTIN.Carr_Cont_Formatted_Phone_No, V_OP_STCENTIN.Carr_Cont_Extension, V_OP_STCENTIN.Carrier_Email
FROM V_OP_STCENTIN
left join op_order_header on op_order_header.Company_Code = V_OP_STCENTIN.Company_Code AND op_order_header.Order_Index = V_OP_STCENTIN.Order_Index
LEFT OUTER JOIN V_OP_CONTACTS AS OP_CONTPERS_2 ON V_OP_STCENTIN.Carr_Contact_Code = OP_CONTPERS_2.Contact_Code 
AND V_OP_STCENTIN.Carr_Contact_Type = OP_CONTPERS_2.Contact_Type 
LEFT OUTER JOIN V_OP_CONTACTS AS OP_CONTPERS_1 ON V_OP_STCENTIN.STC_Contact_Code = OP_CONTPERS_1.Contact_Code 
AND V_OP_STCENTIN.STC_Contact_Type = OP_CONTPERS_1.Contact_Type 
LEFT OUTER JOIN V_OP_CONTACTS ON V_OP_STCENTIN.Dlr_Contact_Code = V_OP_CONTACTS.Contact_Code AND V_OP_STCENTIN.Dlr_Contact_Type = V_OP_CONTACTS.Contact_Type 
WHERE (V_OP_STCENTIN.Company_Code = 'W') AND (op_order_header.Order_No = '{0}') 
AND ((V_OP_STCENTIN.Vnd_No = 'STE01') OR (V_OP_STCENTIN.Vnd_No = 'BRA00'))
";

        public static string OVERALL_GP = @"exec [dbo].[Get_Quote_GP] '{0}' , {1}";

        public static string INSTRUCTIONS_DELIVERY = @"SELECT Instructions FROM dbo.OP_ORDINSTR_CUST
LEFT JOIN dbo.v_op_order_header ON v_op_order_header.Order_Index = OP_ORDINSTR_CUST.Order_Index
WHERE (Instruction_Type = 'D') AND (v_op_order_header.Order_no = '{0}')
";
        public static string INSTRUCTIONS_INVOICE = @"SELECT Instructions FROM dbo.OP_ORDINSTR_CUST
LEFT JOIN dbo.v_op_order_header ON v_op_order_header.Order_Index = OP_ORDINSTR_CUST.Order_Index
WHERE (Instruction_Type = 'I') AND (v_op_order_header.Order_no = '{0}')
";
    }
}
