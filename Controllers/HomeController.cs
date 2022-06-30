using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static Teach_GenMssqlDML.Models.HomeModel;

namespace Teach_GenMssqlDML.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 產生 DML
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public ActionResult GetDML(GetDMLIn inModel)
        {
            GetDMLOut outModel = new GetDMLOut();

            // 資料庫連線
            string connStr = "Data Source={0};Initial Catalog={1};Persist Security Info=false;User ID={2};Password={3};";
            connStr = string.Format(connStr, inModel.Q_DB_IP, inModel.Q_DB_NAME, inModel.Q_USER_ID, inModel.Q_USER_PWD);
            DbConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();

            // 取得資料表欄位
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT M.COLUMN_NAME, M.IS_NULLABLE, M.DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, R1.CONSTRAINT_NAME ");
            sql.Append("FROM INFORMATION_SCHEMA.Columns M ");
            sql.Append("LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE R1 ON R1.TABLE_NAME = M.TABLE_NAME AND R1.COLUMN_NAME = M.COLUMN_NAME AND R1.CONSTRAINT_NAME LIKE 'PK_%' ");
            sql.Append("WHERE M.TABLE_NAME = '" + inModel.Q_TABLE_NAME + "' ");
            sql.Append("ORDER BY M.ORDINAL_POSITION ");

            DbCommand cmd = new SqlCommand();
            cmd.CommandText = sql.ToString();
            cmd.Connection = conn;

            DbDataAdapter adpt = new SqlDataAdapter();
            adpt.SelectCommand = cmd;
            DataSet dsTableColumn = new DataSet();
            try
            {
                adpt.Fill(dsTableColumn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                adpt.Dispose();
                cmd.Parameters.Clear();
                cmd.Dispose();
            }

            // 取得 Primary Key Column
            List<string> KeyColumn = new List<string>();
            foreach (DataRow dr in dsTableColumn.Tables[0].Rows)
            {
                if (dr["CONSTRAINT_NAME"].ToString() != "")
                {
                    KeyColumn.Add(dr["COLUMN_NAME"].ToString());
                }
            }

            // 產生 DML SELECT 
            StringBuilder dml = new StringBuilder();
            dml.Append("SELECT ");
            for (int i = 0; i < dsTableColumn.Tables[0].Rows.Count; i++)
            {
                if (i > 0)
                {
                    dml.Append(", ");
                }
                dml.Append(dsTableColumn.Tables[0].Rows[i]["COLUMN_NAME"]);
            }
            dml.Append(" FROM " + inModel.Q_TABLE_NAME + " WHERE ");
            for (int i = 0; i < KeyColumn.Count; i++)
            {
                if (i > 0)
                {
                    dml.Append(" AND ");
                }
                dml.Append(dsTableColumn.Tables[0].Rows[i]["COLUMN_NAME"] + " = ''");
            }
            outModel.DmlSelect = dml.ToString();

            // 產生 DML INSERT 
            dml.Length = 0;
            dml.Append("INSERT INTO " + inModel.Q_TABLE_NAME + " (");
            for (int i = 0; i < dsTableColumn.Tables[0].Rows.Count; i++)
            {
                if (i > 0)
                {
                    dml.Append(", ");
                }
                dml.Append(dsTableColumn.Tables[0].Rows[i]["COLUMN_NAME"]);
            }
            dml.Append(") VALUES ( ");
            for (int i = 0; i < dsTableColumn.Tables[0].Rows.Count; i++)
            {
                if (i > 0)
                {
                    dml.Append(", ");
                }
                dml.Append("''");
            }
            dml.Append(") ");
            outModel.DmlInsert = dml.ToString();

            // 產生 DML UPDATE 
            dml.Length = 0;
            dml.Append("UPDATE " + inModel.Q_TABLE_NAME + " SET ");
            for (int i = 0; i < dsTableColumn.Tables[0].Rows.Count; i++)
            {
                if (i > 0)
                {
                    dml.Append(", ");
                }
                dml.Append(dsTableColumn.Tables[0].Rows[i]["COLUMN_NAME"] + " = ''");
            }
            dml.Append(" WHERE ");
            for (int i = 0; i < KeyColumn.Count; i++)
            {
                if (i > 0)
                {
                    dml.Append(" AND ");
                }
                dml.Append(dsTableColumn.Tables[0].Rows[i]["COLUMN_NAME"] + " = ''");
            }
            outModel.DmlUpdate = dml.ToString();

            // 產生 DML DELETE 
            dml.Length = 0;
            dml.Append("DELETE FROM " + inModel.Q_TABLE_NAME);
            dml.Append(" WHERE ");
            for (int i = 0; i < KeyColumn.Count; i++)
            {
                if (i > 0)
                {
                    dml.Append(" AND ");
                }
                dml.Append(dsTableColumn.Tables[0].Rows[i]["COLUMN_NAME"] + " = ''");
            }
            outModel.DmlDelete = dml.ToString();

            // 輸出json
            ContentResult resultJson = new ContentResult();
            resultJson.ContentType = "application/json";
            resultJson.Content = JsonConvert.SerializeObject(outModel); ;
            return resultJson;
        }

    }
}