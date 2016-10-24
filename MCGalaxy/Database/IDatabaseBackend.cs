﻿/*
    Copyright 2015 MCGalaxy
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.osedu.org/licenses/ECL-2.0
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Data;
using System.Text;

namespace MCGalaxy.SQL {
    
    public abstract class IDatabaseBackend {
        
        /// <summary> Describes the arguments for a database connection 
        /// (such as database name or file location) </summary>
        public abstract string ConnectionString { get; }
        
        /// <summary> Whether this backend enforces the character length in VARCHAR columns. </summary>
        public abstract bool EnforcesTextLength { get; }
        
        /// <summary> Creates the schema for this database (if required). </summary>
        public abstract void CreateDatabase();
        
        /// <summary> Returns a new BulkTransaction instance, which can be used to execute
        /// many sql statements as one single transaction. </summary>
        public abstract BulkTransaction CreateBulk();
        
        /// <summary> Returns a new ParameterisedQuery instance, which executes sql statements 
        /// and manages binding of parameters for sql queries. </summary>
        public abstract ParameterisedQuery CreateParameterised();
        
        /// <summary> Returns the shared static ParamterisedQuery instance, that is only used 
        /// for sql queries with no parameters. </summary>
        internal abstract ParameterisedQuery GetStaticParameterised();
        
        
        // == Higher level functions ==
        
        /// <summary> Returns whether a table (case sensitive) exists by that name. </summary>
        public abstract bool TableExists(string table);
        
        /// <summary> Renames the source table to the given name. </summary>
        public abstract void RenameTable(string srcTable, string dstTable);
        
        /// <summary> Removes all entries from the given table. </summary>
        public abstract void ClearTable(string table);
        
        /// <summary> Adds a new coloumn to the given table. </summary>
        /// <remarks> Note colAfter is only a hint - some database backends ignore this. </remarks>
        public abstract void AddColumn(string table, string column, 
                                       string colType, string colAfter);
        
        /// <summary> Creates a new table in the database (unless it already exists). </summary>
        public virtual void CreateTable(string table, ColumnParams[] columns) {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("CREATE TABLE if not exists `" + table + "` (");
            CreateTableColumns(sql, columns);
            sql.AppendLine(");");
            Database.Execute(sql.ToString());
        }
        
        /// <summary> Creates a new table in the database (unless it already exists). </summary>
        protected abstract void CreateTableColumns(StringBuilder sql, ColumnParams[] columns);
        
        /// <summary> Completely removes the given table from the database. </summary>
        public virtual void DeleteTable(string table) {
            string syntax = "DROP TABLE `" + table + "`";
            Database.Execute(syntax);
        }
        
        
        /// <summary> Inserts/Copies all the rows from the source table into the destination table. </summary>
        /// <remarks> Note: This may work incorrectly if the tables have different schema. </remarks>
        public virtual void CopyAllRows(string srcTable, string dstTable) {
            string syntax = "INSERT INTO `" + dstTable + "` SELECT * FROM `" + srcTable + "`";
            Database.Execute(syntax);
        }
        
        /// <summary> Retrieves rows for the given table from the database. </summary>
        /// <remarks> modifier is optional SQL which can be used to retrieve only certain rows, 
        /// return rows in a certain order, etc.</remarks>
        public virtual DataTable GetRows(string table, string columns, 
                                         string modifier = "", params object[] args) {
            string syntax = "SELECT " + columns + " FROM `" + table + "`";
            if (modifier != "") syntax += " " + modifier;
            return Database.Fill(syntax, args);
        }
        
        /// <summary> Updates rows for the given table from the database. </summary>
        /// <remarks> modifier is optional SQL which can be used to update only certain rows.</remarks>
        public virtual void UpdateRows(string table, string columns, 
                                       string modifier = "", params object[] args) {
            string syntax = "UPDATE `" + table + "` SET " + columns;
            if (modifier != "") syntax += " " + modifier;
            Database.Execute(syntax, args);
        }
        
        /// <summary> Deletes rows for the given table from the database. </summary>
        /// <remarks> modifier is optional SQL which can be used to delete only certain rows.</remarks>
        public virtual void DeleteRows(string table, string modifier = "", params object[] args) {
            string syntax = "DELETE FROM `" + table + "`";
            if (modifier != "") syntax += " " + modifier;
            Database.Execute(syntax, args);
        }
    }
}
