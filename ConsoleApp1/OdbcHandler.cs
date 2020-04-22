using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data.Common;


namespace ConsoleApp1 {
    public class OdbcHandler {


        private string sLastError = "";
        private string sLastSql = "";

        public string LastError { get { return sLastError; } }
        public string LastSql { get { return sLastSql; } }

        public bool Connected { get { return false; } }

        public string Drivername { get; set; }
        public string Hostname { get; set; }
        public uint Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        private string _odbcConnectionString = "";



        public bool Connect() {
            try {

                OdbcConnectionStringBuilder mcsb = new OdbcConnectionStringBuilder();
                mcsb["DRIVER"] = this.Drivername;                
                mcsb["SERVER"] = this.Hostname;
                mcsb["PORT"] = this.Port == 0 ? 3306 : this.Port;
                if (this.Database != "")
                    mcsb["DATABASE"] = this.Database;
                mcsb["USER"] = this.Username;
                mcsb["PASSWORD"] = this.Password;
                mcsb["CHARSET"] = "UTF8";                                 

                _odbcConnectionString = mcsb.ToString();
                try {
                    using (OdbcConnection oc = new OdbcConnection(_odbcConnectionString)) {
                        oc.Open();
                        oc.Close();
                    }
                }
                catch (Exception hEx) {
                    sLastError = hEx.Message;
                    sLastSql = "Connect";
                    return false;
                }

                return true;

            }
            catch (Exception) { _odbcConnectionString = ""; return false; }
        }


        private OdbcConnection _connectIntern() {
            OdbcConnection mcReturn = new OdbcConnection(_odbcConnectionString);
            try {
                mcReturn.Open();
            }
            catch (OdbcException hEx) {
                // stripped
            }
            catch (Exception hEx) {
                // stripped
            }
            return mcReturn;
        }


        public DatabaseResult Query(string sQuery, object[] oParameter) {

            // stripped //
            OdbcConnection localConnection = _connectIntern();

            // Validate connection state and abort on error.            
            // stripped //


            // Beginn command parsing and execution.
            DatabaseResult drReturn = new DatabaseResult();
            using (OdbcCommand mCommand = new OdbcCommand(sQuery, localConnection)) {

                if (oParameter != null) {
                    for (int i = 0; i < oParameter.Length; i++) {
                        Type tParameterType = oParameter[i].GetType();

                        if (tParameterType == typeof(ulong)) {
                            mCommand.Parameters.AddWithValue("?", Convert.ToDecimal(oParameter[i]));
                        }
                        else if (tParameterType == typeof(DateTime)) {
                            DateTime dtParam = (DateTime)oParameter[i];
                            if (dtParam == DateTime.MinValue) {
                                mCommand.Parameters.AddWithValue("?", "0000-00-00 00:00:00");
                            }
                            else {
                                var param = mCommand.Parameters.AddWithValue("?", dtParam.ToString("yyyy-MM-dd HH:mm:ss"));
                                param.DbType = System.Data.DbType.DateTime;
                                param.OdbcType = OdbcType.Text;
                            }
                        }
                        else {
                            var cmd = mCommand.Parameters.AddWithValue("?", oParameter[i]);
                        }
                    }

                }

                mCommand.Prepare();


                string sAction = mCommand.CommandText.Substring(0, mCommand.CommandText.IndexOf(' ')).Trim().ToLowerInvariant();
                switch (sAction) {
                    default:
                    case "call":
                    case "select":
                        using (OdbcDataReader mReader = mCommand.ExecuteReader()) {
                            while (mReader.Read()) {

                                DatabaseRow d = new DatabaseRow();
                                for (int i = 0; i < mReader.VisibleFieldCount; i++) {
                                    string sFieldName = mReader.GetName(i);
                                    Type tFieldType = mReader.GetFieldType(i);

                                    if (tFieldType == typeof(DateTime)) {
                                        try {
                                            if (mReader.IsDBNull(i)) {
                                                d[sFieldName] = DateTime.MinValue;
                                            }
                                            else {
                                                d[sFieldName] = mReader.GetValue(i);
                                            }
                                        }
                                        catch (Exception) {
                                            d[sFieldName] = DateTime.MinValue;
                                        }
                                    }
                                    else {
                                        var val = mReader.GetValue(i);
                                        d[sFieldName] = val;
                                    }

                                }


                                if (drReturn.Result == null)
                                    drReturn.Result = d;

                                drReturn.ResultSet.Add(d);
                            }
                        }
                        drReturn.Affected = drReturn.ResultSet.Count;
                        drReturn.Success = true;
                        break;

                    case "insert":
                        drReturn.Affected = mCommand.ExecuteNonQuery();
                        mCommand.CommandText = "select last_insert_id()";
                        drReturn.ID = Convert.ToUInt64(mCommand.ExecuteScalar());
                        drReturn.Success = true;
                        break;

      

                }
                // stripped //
                localConnection.Dispose();

                // stripped //

                return drReturn;
            }

        }




    }

}
