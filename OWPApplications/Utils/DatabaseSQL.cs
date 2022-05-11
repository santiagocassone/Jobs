using System;
using System.Data;
using System.Data.SqlClient;

namespace OWPApplications.Utils
{
    public class DataBaseSQL : System.IDisposable
    {
        private bool disposing = false;
        private SqlConnection cnnConnection = null;
        private SqlTransaction trnTransaction = null;
        private bool InTransaction = false;
        private string strConnectionString = "";

        /// <summary>
        /// Create this object using a Connection string parameter
        /// </summary>
        /// <param name="pConnectionString"></param>
        public DataBaseSQL(string pConnectionString)
        {
            strConnectionString = pConnectionString;
        }
        ~DataBaseSQL()
        {
            Dispose(true);

        }
        /// <summary>
        /// Open the database using the current string connection
        /// </summary>
        public void Open()
        {
            SqlConnection oCnn = getConnection();
        }
        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool b)
        {
            if (!disposing)
            {
                disposing = true;
                GC.SuppressFinalize(this);
                try
                {
                    if (trnTransaction != null)
                    {
                        trnTransaction.Rollback();
                        trnTransaction = null;
                    }
                }
                catch (Exception) { }/*Ignore any error*/
                try
                {
                    if (cnnConnection != null)
                    {
                        if (cnnConnection.State != ConnectionState.Closed)
                            cnnConnection.Close();

                        //remove the connection in the pool
                        System.Data.SqlClient.SqlConnection.ClearPool(cnnConnection);

                        cnnConnection.Dispose();

                        cnnConnection = null;
                    }
                }
                catch (Exception) { }/*Ignore any error*/
            }
        }
        public SqlTransaction currentTransaction
        {
            get { return trnTransaction; }
        }
        /// <summary>
        /// Check if the connection to database is OK
        /// </summary>
        /// <returns></returns>
        public bool IsConnectionOk()
        {
            try
            {
                DataRow oRow = SQLToRow("SELECT getDate() as [DateTime]");
                return true;
            }
            catch (Exception)
            {
                //Log.Write("Error executing query on DB: ", ex);
            }
            return false;
        }
        /// <summary>
        /// Returns the current DateTime of the database
        /// </summary>
        /// <returns></returns>
        public DateTime GetDate()
        {
            try
            {
                DataRow oRow = SQLToRow("SELECT getDate() as [DateTime]");
                return Convert.ToDateTime(oRow["DateTime"]);
            }
            catch (Exception)
            {
                /*Ignore any error*/
            }
            return DateTime.Now;
        }
        /// <summary>
        /// Return the server Name of the database
        /// </summary>
        /// <returns></returns>
        public string getServerName()
        {
            try
            {
                DataRow oRow = SQLToRow("SELECT @@SERVERNAME as [ServerName]");
                return Convert.ToString(oRow["ServerName"]);
            }
            catch (Exception)
            {
                /*Ignore any error*/
            }
            return "";
        }
        /// <summary>
        /// Returns the Database Name of the current connection
        /// </summary>
        /// <returns></returns>
        public string getDatabaseName()
        {
            try
            {
                DataRow oRow = SQLToRow("SELECT db_name() as [DatabaseName]");
                return Convert.ToString(oRow["DatabaseName"]);
            }
            catch (Exception)
            {
                /*Ignore any error*/
            }
            return "";
        }
        /// <summary>
        /// Returns the current Connection 
        /// </summary>
        /// <returns></returns>
        private SqlConnection getConnection()
        {
            if (cnnConnection == null)
            {
                cnnConnection = new SqlConnection(strConnectionString);
                try
                {
                    cnnConnection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error connecting to database", ex);
                }
            }
            return cnnConnection;
        }
        /// <summary>
        /// Drop the current connection
        /// </summary>
        private void killConnection()
        {
            if (cnnConnection != null)
            {
                if (cnnConnection.State != ConnectionState.Closed)
                {
                    cnnConnection.Close();
                }
                cnnConnection = null;
            }
        }
        /// <summary>
        /// Begin a transaction
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                if (!InTransaction)
                {
                    SqlConnection oCon = getConnection();
                    trnTransaction = oCon.BeginTransaction();
                    InTransaction = true;
                }
                else
                {
                    throw (new Exception("Not in transaction"));
                }
            }
            catch (Exception err)
            {
                InTransaction = false;
                killConnection();
                throw (err);
            }
        }
        /// <summary>
        /// commit a transaction
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                trnTransaction.Commit();
            }
            catch (Exception err)
            {
                throw (err);
            }
            finally
            {
                InTransaction = false;
                killConnection();
            }
        }
        /// <summary>
        /// Rollback a transaction
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                if (InTransaction)
                {
                    trnTransaction.Rollback();
                }
            }
            catch (Exception err)
            {
                throw (err);
            }
            finally
            {
                InTransaction = false;
                killConnection();
            }
        }
        /// <summary>
        /// REturns a SQL Command created with a Stored Procedure Call instruction
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public SqlCommand GetCommandStoredProcedure(string strStoredProcedureName)
        {

            SqlCommand cmdCommand = new SqlCommand();

            cmdCommand.Connection = getConnection();
            cmdCommand.Transaction = trnTransaction;
            cmdCommand.CommandType = CommandType.StoredProcedure;
            cmdCommand.CommandText = strStoredProcedureName;
            cmdCommand.CommandTimeout = cmdCommand.Connection.ConnectionTimeout;
            return cmdCommand;

        }
        /// <summary>
        /// REturns a SQL Command created with a SQL instruction
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public SqlCommand GetCommandSQL(string strSQL)
        {

            SqlCommand cmdCommand = new SqlCommand();

            cmdCommand.Connection = getConnection();
            cmdCommand.Transaction = trnTransaction;
            cmdCommand.CommandType = CommandType.Text;
            cmdCommand.CommandText = strSQL;
            cmdCommand.CommandTimeout = cmdCommand.Connection.ConnectionTimeout;
            return cmdCommand;

        }
        /// <summary>
        /// Execute a command and return the Row resultant
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataRow SqlCommandToRow(SqlCommand cmdCommand)
        {
            DataTable oTable = SqlCommandToTable(cmdCommand);
            if (oTable.Rows.Count > 0)
                return oTable.Rows[0];
            return null;
        }
        /// <summary>
        /// Execute a command and return the Table resultant
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataTable SqlCommandToTable(SqlCommand cmdCommand)
        {
            return this.DataSetToTable(SqlCommandToDataset(cmdCommand));
        }
        /// <summary>
        /// Execute a command and return the Dataset resultant
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataSet SqlCommandToDataset(SqlCommand cmdCommand)
        {
            DataSet oDataSet = new DataSet();
            try
            {
                SqlDataAdapter adpAdapter = new SqlDataAdapter(cmdCommand);
                adpAdapter.Fill(oDataSet);
            }
            catch (Exception e)
            {
                throw e;
            }
            return oDataSet;
        }
        /// <summary>
        /// Execute a SQL instruction  and return the Row resultant
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataRow SQLToRow(string SQL)
        {
            DataTable oTable = this.DataSetToTable(SQLToDataset(SQL));
            if (oTable != null)
                if (oTable.Rows.Count > 0)
                    return oTable.Rows[0];
            return null;
        }
        /// <summary>
        /// Execute a SQL instruction and return the Table resultant
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataTable SQLToTable(string SQL)
        {
            return this.DataSetToTable(SQLToDataset(SQL));
        }
        /// <summary>
        /// Execute a SQL instruction and return the Dataset resultant
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataSet SQLToDataset(string SQL)
        {
            DataSet oDataSet = new DataSet();
            try
            {
                SqlCommand cmdCommand = GetCommandSQL(SQL);
                SqlDataAdapter adpAdapter = new SqlDataAdapter(cmdCommand);
                adpAdapter.Fill(oDataSet);
            }
            catch (Exception e)
            {
                throw e;
            }
            return oDataSet;
        }

        public bool Table_ExistColumn(string TableName, string ColumnName)
        {
            bool bExist = false;
            try
            {
                DataRow oRow = SQLToRow(@"
    SELECT *
    FROM sys.columns 
    WHERE Name      = N'" + ColumnName + @"'
      AND Object_ID = Object_ID(N'" + TableName + @"')
");
                if (oRow != null)
                    return true;
                return false;
            }
            catch (Exception)
            {
            }
            return bExist;
        }

        /// <summary>
        /// Return the first table from a DataSet
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataTable DataSetToTable(DataSet oDataSet)
        {
            if (oDataSet != null)
                if (oDataSet.Tables.Count > 0)
                    return oDataSet.Tables[0];
            return new DataTable();
        }
        /// <summary>
        /// Return a DataView sorted
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataView DataSetToDataViewSorted(DataSet oDataSet, string SortExpression)
        {
            DataTable oTable = new DataTable();
            if (oDataSet != null)
                if (oDataSet.Tables.Count > 0)
                    oTable = oDataSet.Tables[0];
            DataView oDataView = new DataView(oTable);
            if (SortExpression != "")
            {
                oDataView.Sort = SortExpression;
            }
            return oDataView;
        }
        /// <summary>
        /// Return a DataView sorted
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public DataView DataTableToDataViewSorted(DataTable oTable, string SortExpression)
        {
            DataView oDataView = new DataView(oTable);

            if (SortExpression != "")
            {
                oDataView.Sort = SortExpression;
            }

            return oDataView;
        }

        /// <summary>
        /// Delete a row of a table
        /// </summary>
        /// <param name="cmdCommand"></param>
        /// <returns></returns>
        public bool ROW_Delete(string TableName, string Condition)
        {
            try
            {
                string sql = "DELETE FROM " + TableName + " WHERE " + Condition;
                SqlCommand oCmd = GetCommandSQL(sql);
                return (oCmd.ExecuteNonQuery() > 0);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " [in Database.Delete('" + TableName + "','" + Condition + "')]");
            }
        }
        /// <summary>
        /// Add a parameter to a sql command
        /// </summary>
        public void CommandAddParameter(SqlCommand oCmd, string ParamName, SqlDbType paramType, object Value)
        {
            SqlParameter oParam = new SqlParameter(ParamName, paramType);
            oParam.Value = Value;
            oCmd.Parameters.Add(oParam);
        }
        /// <summary>
        /// Add a parameter to a sql command including the size
        /// </summary>
        public void CommandAddParameterS(SqlCommand oCmd, string ParamName, SqlDbType paramType, int size, object Value)
        {
            SqlParameter oParam = new SqlParameter(ParamName, paramType, size);
            oParam.Value = Value;
            oCmd.Parameters.Add(oParam);
        }
        /// <summary>
        /// Return the list of Databases present in the server
        /// </summary>
        /// <returns></returns>
        public DataTable getDatabases()
        {
            DataTable oTable = null;
            //try getting the databases running the SP
            SqlCommand oCmd = GetCommandSQL("sp_databases");
            try
            {
                oTable = SqlCommandToTable(oCmd);
            }
            catch (Exception)
            {
                /*Ignore any error*/
            }
            bool bReload = false;
            //if no records return, then try to get databasses asking 1 by 1
            if (oTable == null)
            {
                bReload = true;
            }
            else
            {
                bReload = (oTable.Rows.Count == 0);
            }
            if (bReload)
            {
                oTable = new DataTable();
                oTable.Columns.Add("DATABASE_NAME");
                int i = 0;
                //ask for the first 200 databases
                while (i < 20)
                {
                    //get 10 tables name in the same query
                    string sql = @"SELECT 
db_name(" + (i * 10).ToString() + @"),
db_name(" + ((i * 10) + 1).ToString() + @"),
db_name(" + ((i * 10) + 2).ToString() + @"),
db_name(" + ((i * 10) + 3).ToString() + @"),
db_name(" + ((i * 10) + 4).ToString() + @"),
db_name(" + ((i * 10) + 5).ToString() + @"),
db_name(" + ((i * 10) + 6).ToString() + @"),
db_name(" + ((i * 10) + 7).ToString() + @"),
db_name(" + ((i * 10) + 8).ToString() + @"),
db_name(" + ((i * 10) + 9).ToString() + @"),
db_name(" + ((i * 10) + 9).ToString() + @")
";
                    DataRow oRow = SQLToRow(sql);
                    if (oRow != null)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (Convert.ToString(oRow[j]).Trim() != "")
                            {
                                DataRow oNewRow = oTable.NewRow();
                                oNewRow["DATABASE_NAME"] = Convert.ToString(oRow[j]).Trim();
                                oTable.Rows.Add(oNewRow);
                            }
                            else
                            {
                                return oTable;
                            }
                        }
                    }
                    else
                    {
                        //exit
                        return oTable;
                    }
                    i++;
                }
            }
            return oTable;
        }
    }
}
