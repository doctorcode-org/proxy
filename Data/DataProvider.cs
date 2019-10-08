using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using DoctorProxy.EventLoger;
using System.Reflection;

namespace DoctorProxy
{
    public class SQLiteDataProvider
    {
        string strConnection;

        public SQLiteDataProvider()
        {
            strConnection = String.Format("Data Source={0}\\db.s3db;Version=3;", AppDomain.CurrentDomain.BaseDirectory);
        }

        //___________________________________________________________________________________________________________________________________

        public DataTable GetData(List<SQLiteParameter> param, string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            foreach (SQLiteParameter par in param)
            {
                if (par.Value == null)
                {
                    par.Value = DBNull.Value;
                }
                cmd.Parameters.Add(par);
            }
            return GetData(cmd);
        }

        public DataTable GetData(SQLiteParameter param, string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(param);
            return GetData(cmd);
        }

        public DataTable GetData(string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            return GetData(cmd);
        }

        public DataTable GetData(SQLiteCommand command)
        {
            var table = new DataTable();
            using (var cnn = new SQLiteConnection(strConnection))
            {
                try
                {
                    cnn.Open();
                    command.Connection = cnn;
                    using (var reader = command.ExecuteReader())
                    {
                        table.Load(reader);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(MethodInfo.GetCurrentMethod(), ex);
                }

                return table;
            }
        }

        //___________________________________________________________________________________________________________________________________

        public bool Run(List<SQLiteParameter> param, string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            foreach (var par in param)
            {
                if (par.Value == null)
                {
                    par.Value = DBNull.Value;
                }
                cmd.Parameters.Add(par);
            }
            return Run(cmd);
        }

        public bool Run(SQLiteParameter param, string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(param);
            return Run(cmd);
        }

        public bool Run(string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            return Run(cmd);
        }

        public bool Run(SQLiteCommand command)
        {
            var result = false;
            using (var cnn = new SQLiteConnection(strConnection))
            {
                try
                {
                    cnn.Open();
                    command.Connection = cnn;
                    result = (command.ExecuteNonQuery() > 0);
                }
                catch (Exception ex)
                {
                    Log.Write(MethodInfo.GetCurrentMethod(), ex);
                }

                return result;
            }
        }

        //___________________________________________________________________________________________________________________________________

        public T GetValue<T>(List<SQLiteParameter> param, string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            foreach (SQLiteParameter par in param)
            {
                if (par.Value == null)
                {
                    par.Value = DBNull.Value;
                }
                cmd.Parameters.Add(par);
            }
            return GetValue<T>(cmd);
        }

        public T GetValue<T>(SQLiteParameter param, string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(param);
            return GetValue<T>(cmd);
        }

        public T GetValue<T>(string commandText)
        {
            var cmd = new SQLiteCommand(commandText);
            cmd.CommandType = CommandType.Text;
            return GetValue<T>(cmd);
        }

        public T GetValue<T>(SQLiteCommand command)
        {
            var result = default(T);
            using (var cnn = new SQLiteConnection(strConnection))
            {
                try
                {
                    cnn.Open();
                    command.Connection = cnn;
                    var dbResult = command.ExecuteScalar();
                    result = (T)Convert.ChangeType(dbResult, typeof(T));
                }
                catch (Exception ex)
                {
                    Log.Write(MethodInfo.GetCurrentMethod(), ex);
                }

                return result;
            }
        }

    }
}
