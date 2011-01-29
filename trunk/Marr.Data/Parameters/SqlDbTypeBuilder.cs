﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Marr.Data.Mapping;

namespace Marr.Data.Parameters
{
    public class SqlDbTypeBuilder : IDbTypeBuilder
    {
        public Enum GetDbType(Type type)
        {
            if (type == typeof(String))
                return SqlDbType.VarChar;

            else if (type == typeof(Int32))
                return SqlDbType.Int;

            else if (type == typeof(Decimal))
                return SqlDbType.Decimal;

            else if (type == typeof(DateTime))
                return SqlDbType.DateTime;

            else if (type == typeof(Boolean))
                return SqlDbType.Bit;

            else if (type == typeof(Int16))
                return SqlDbType.SmallInt;

            else if (type == typeof(Int64))
                return SqlDbType.BigInt;

            else if (type == typeof(Double))
                return SqlDbType.Float;

            else if (type == typeof(Char))
                return SqlDbType.Char;

            else if (type == typeof(Byte))
                return SqlDbType.Binary;

            else if (type == typeof(Byte[]))
                return SqlDbType.VarBinary;

            else if (type == typeof(Guid))
                return SqlDbType.UniqueIdentifier;

            else
                return SqlDbType.Variant;
        }

        public void SetDbType(System.Data.IDbDataParameter param, ColumnMap column)
        {
            var sqlDbParam = (SqlParameter)param;
            sqlDbParam.SqlDbType = (SqlDbType)column.DBType;
        }
    }
}
